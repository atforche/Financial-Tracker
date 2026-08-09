"""Transactional deployment and rollback operations."""

from __future__ import annotations

import os
import shutil
import tempfile
from datetime import UTC, datetime
from pathlib import Path

from ..core.runner import Runner
from .configuration import Configuration
from .instance import resolve_instance_path
from .migrator import run_migrator
from .release_manifest import ReleaseManifest

RECOVERY_DIRECTORY_NAME = ".rollback"
STAGING_RECOVERY_DIRECTORY_NAME = ".rollback-staging"
RECOVERY_FILE_NAMES = (".env", "compose.yaml", "Caddyfile")
OPTIONAL_RECOVERY_FILE_NAMES = ("release-manifest.json",)
DATABASE_DIRECTORY_NAME = "data"
DATABASE_FILE_NAME = "database.db"


def prepare_database_directory(instance_path: Path) -> None:
    """Create the writable SQLite directory and migrate a legacy database file."""

    database_directory = instance_path / DATABASE_DIRECTORY_NAME
    database_path = database_directory / DATABASE_FILE_NAME
    legacy_database_path = instance_path / DATABASE_FILE_NAME
    database_directory.mkdir(mode=0o777, exist_ok=True)
    os.chmod(database_directory, 0o777)
    if legacy_database_path.exists() and not database_path.exists():
        os.replace(legacy_database_path, database_path)
    if not database_path.exists():
        database_path.touch(mode=0o666)
    os.chmod(database_path, 0o666)


def apply_migrations(
    configuration: Configuration,
    bootstrap_admin_email: str | None = None,
    runner: Runner | None = None,
) -> None:
    """Apply migrations in a disposable copy, then atomically replace the database."""

    database_file_path = Path(configuration.get_database_file_path())
    upgrade_directory = Path(
        tempfile.mkdtemp(prefix=".migration-", dir=configuration.path)
    )
    os.chmod(upgrade_directory, 0o777)
    upgrade_database_file_path = upgrade_directory / DATABASE_FILE_NAME
    shutil.copy(database_file_path, upgrade_database_file_path)
    os.chmod(upgrade_database_file_path, 0o666)
    try:
        environment = (
            {"BOOTSTRAP_ADMIN_EMAIL": bootstrap_admin_email}
            if bootstrap_admin_email
            else None
        )
        if runner is None:
            run_migrator(configuration.migrator_image, upgrade_directory, environment)
        else:
            run_migrator(
                configuration.migrator_image,
                upgrade_directory,
                environment,
                runner=runner,
            )
    except Exception:
        shutil.rmtree(upgrade_directory)
        raise

    archive_directory = Path(configuration.path) / "archive"
    archive_directory.mkdir(parents=True, exist_ok=True)
    archive_timestamp = datetime.now(UTC).strftime("%Y%m%dT%H%M%SZ")
    shutil.copy2(
        database_file_path,
        archive_directory / f"database-{archive_timestamp}.db",
    )
    os.replace(upgrade_database_file_path, database_file_path)
    shutil.rmtree(upgrade_directory)


def validate_deploy_request(instance_path: str, release_manifest: str) -> None:
    """Validate instance and release artifact paths before changing anything."""

    path = resolve_instance_path(instance_path)
    if path.exists() and not path.is_dir():
        raise ValueError(f"Path {path} does not point to a valid directory")
    if not path.exists() and not path.parent.is_dir():
        raise ValueError(f"Parent path {path.parent} does not exist")
    if path.exists() and not (path / ".env").is_file():
        raise ValueError(f"Path {path} is not an initialized instance")
    manifest_path = Path(release_manifest).resolve()
    if not manifest_path.is_file():
        raise ValueError(f"Release manifest {manifest_path} does not exist")
    for file_name in ("compose.yaml", "Caddyfile"):
        if not (manifest_path.parent / file_name).is_file():
            raise ValueError(f"Release artifact is missing {file_name}")


def deploy_release(
    instance_path_value: str,
    release_manifest_value: str,
    change_configuration: bool,
    runner: Runner | None = None,
) -> None:
    """Deploy an immutable release, bootstrapping a missing instance when needed."""

    instance_path = Path(instance_path_value).resolve()
    manifest_path = Path(release_manifest_value).resolve()
    validate_deploy_request(instance_path_value, release_manifest_value)
    manifest = ReleaseManifest.read(manifest_path)
    bootstrap_admin_email = os.environ.get("INITIAL_ADMIN_EMAIL", "").strip()
    if not bootstrap_admin_email:
        raise ValueError("INITIAL_ADMIN_EMAIL must be configured")

    if instance_path.exists():
        configuration = Configuration.build_from_existing_instance(
            str(instance_path), change_configuration
        )
        configuration.path = str(instance_path)
        configuration.backend_image = manifest.backend_image
        configuration.frontend_image = manifest.frontend_image
        configuration.migrator_image = manifest.migrator_image
        TransactionalDeployment(instance_path, runner).deploy(
            configuration, manifest_path, bootstrap_admin_email
        )
        return

    configuration = Configuration.build_from_environment(
        str(instance_path),
        manifest.backend_image,
        manifest.frontend_image,
        manifest.migrator_image,
    )
    deployment = TransactionalDeployment(instance_path, runner)
    instance_path.mkdir(mode=0o750)
    try:
        (instance_path / "logs").mkdir(mode=0o777)
        (instance_path / "archive").mkdir(mode=0o750)
        prepare_database_directory(instance_path)
        deployment.validate_release(configuration, manifest_path.parent)
        deployment.pull_images(configuration)
        deployment.install_release_files(configuration, manifest_path)
        apply_migrations(configuration, bootstrap_admin_email, deployment.runner)
        deployment.start_instance()
    except Exception:
        deployment.stop_instance(throw_on_error=False)
        shutil.rmtree(instance_path)
        raise


def rollback_release(instance_path_value: str, runner: Runner | None = None) -> None:
    """Restore the immediately previous healthy deployment."""

    path = Path(instance_path_value).resolve()
    if not path.is_dir():
        raise ValueError(f"Path {path} does not point to a valid directory")
    if not (path / RECOVERY_DIRECTORY_NAME).is_dir():
        raise ValueError(f"Instance {path} does not have a rollback recovery point")
    TransactionalDeployment(path, runner).rollback()


class TransactionalDeployment:
    """Coordinate image preparation, migration, health verification, and recovery."""

    def __init__(self, instance_path: Path, runner: Runner | None = None) -> None:
        self.instance_path = instance_path
        self.recovery_path = instance_path / RECOVERY_DIRECTORY_NAME
        self.staging_recovery_path = instance_path / STAGING_RECOVERY_DIRECTORY_NAME
        self.runner = runner or Runner()

    def deploy(
        self,
        configuration: Configuration,
        release_manifest_path: Path,
        bootstrap_admin_email: str,
    ) -> None:
        self.ensure_no_incomplete_transaction()
        release_directory = release_manifest_path.parent
        self.validate_release(configuration, release_directory)
        self.pull_images(configuration)
        stopped = False
        recovery_created = False
        try:
            self.stop_instance()
            stopped = True
            self.create_recovery_point(self.staging_recovery_path)
            recovery_created = True
            self.install_release_files(configuration, release_manifest_path)
            prepare_database_directory(self.instance_path)
            apply_migrations(configuration, bootstrap_admin_email, self.runner)
            self.start_instance()
            self.promote_recovery_point()
        except Exception as deployment_error:
            if stopped:
                self.stop_instance(throw_on_error=False)
                if recovery_created:
                    self.restore_recovery_point(self.staging_recovery_path)
                try:
                    self.start_instance()
                except Exception as rollback_error:
                    raise RuntimeError(
                        "Deployment failed and the previous release could not be restarted"
                    ) from rollback_error
                if self.staging_recovery_path.exists():
                    shutil.rmtree(self.staging_recovery_path)
            raise RuntimeError(
                "Deployment failed; the previous release was restored"
            ) from deployment_error

    def rollback(self) -> None:
        self.ensure_no_incomplete_transaction()
        rollback_configuration = Configuration.build_from_existing_instance(
            str(self.recovery_path), False
        )
        self.ensure_images(rollback_configuration)
        self.stop_instance()
        recovery_created = False
        try:
            self.create_recovery_point(self.staging_recovery_path)
            recovery_created = True
            self.restore_recovery_point(self.recovery_path)
            self.start_instance()
        except Exception as rollback_error:
            self.stop_instance(throw_on_error=False)
            if recovery_created:
                self.restore_recovery_point(self.staging_recovery_path)
            try:
                self.start_instance()
            except Exception as recovery_error:
                raise RuntimeError(
                    "Rollback failed and the original release could not be restarted"
                ) from recovery_error
            if self.staging_recovery_path.exists():
                shutil.rmtree(self.staging_recovery_path)
            raise RuntimeError(
                "Rollback failed; the original release was restored"
            ) from rollback_error

        shutil.rmtree(self.recovery_path)
        os.replace(self.staging_recovery_path, self.recovery_path)

    def ensure_no_incomplete_transaction(self) -> None:
        if self.staging_recovery_path.exists():
            raise RuntimeError(
                f"Incomplete deployment recovery point exists at {self.staging_recovery_path}; inspect it before retrying"
            )

    def pull_images(self, configuration: Configuration) -> None:
        for image in self._images(configuration):
            self.runner.run(["docker", "pull", image])

    def ensure_images(self, configuration: Configuration) -> None:
        for image in self._images(configuration):
            result = self.runner.run(
                ["docker", "image", "inspect", image],
                check=False,
                capture_output=True,
            )
            if result.returncode != 0:
                self.runner.run(["docker", "pull", image])

    @staticmethod
    def _images(configuration: Configuration) -> tuple[str, str, str]:
        return (
            configuration.backend_image,
            configuration.frontend_image,
            configuration.migrator_image,
        )

    def validate_release(
        self, configuration: Configuration, release_directory: Path
    ) -> None:
        with tempfile.NamedTemporaryFile(
            mode="w",
            prefix=".deployment-environment-",
            dir=self.instance_path,
            delete=False,
        ) as file:
            environment_file_path = Path(file.name)
        try:
            configuration.write_to_file(str(environment_file_path))
            self.runner.run(
                [
                    "docker",
                    "compose",
                    "--file",
                    str(release_directory / "compose.yaml"),
                    "--env-file",
                    str(environment_file_path),
                    "config",
                    "--quiet",
                ],
            )
        finally:
            environment_file_path.unlink(missing_ok=True)

    def create_recovery_point(self, destination: Path) -> None:
        destination.mkdir(mode=0o700)
        try:
            for file_name in RECOVERY_FILE_NAMES:
                shutil.copy2(self.instance_path / file_name, destination / file_name)
            for file_name in OPTIONAL_RECOVERY_FILE_NAMES:
                source = self.instance_path / file_name
                if source.exists():
                    shutil.copy2(source, destination / file_name)
            database_directory = self.instance_path / DATABASE_DIRECTORY_NAME
            if database_directory.exists():
                shutil.copytree(
                    database_directory, destination / DATABASE_DIRECTORY_NAME
                )
        except Exception:
            shutil.rmtree(destination)
            raise

    def install_release_files(
        self, configuration: Configuration, release_manifest_path: Path
    ) -> None:
        release_directory = release_manifest_path.parent
        shutil.copy2(
            release_directory / "compose.yaml", self.instance_path / "compose.yaml"
        )
        shutil.copy2(release_directory / "Caddyfile", self.instance_path / "Caddyfile")
        shutil.copy2(
            release_manifest_path, self.instance_path / "release-manifest.json"
        )
        configuration.write_to_file()

    def restore_recovery_point(self, source: Path) -> None:
        for file_name in RECOVERY_FILE_NAMES + OPTIONAL_RECOVERY_FILE_NAMES:
            recovery_file = source / file_name
            instance_file = self.instance_path / file_name
            if recovery_file.exists():
                shutil.copy2(recovery_file, instance_file)
            else:
                instance_file.unlink(missing_ok=True)
        recovery_database_directory = source / DATABASE_DIRECTORY_NAME
        instance_database_directory = self.instance_path / DATABASE_DIRECTORY_NAME
        if instance_database_directory.exists():
            shutil.rmtree(instance_database_directory)
        if recovery_database_directory.exists():
            shutil.copytree(recovery_database_directory, instance_database_directory)

    def promote_recovery_point(self) -> None:
        if self.recovery_path.exists():
            shutil.rmtree(self.recovery_path)
        os.replace(self.staging_recovery_path, self.recovery_path)

    def stop_instance(self, throw_on_error: bool = True) -> None:
        result = self.runner.run(self.compose_command("down"), check=False)
        if throw_on_error and result.returncode != 0:
            raise RuntimeError("The running instance could not be stopped")

    def start_instance(self) -> None:
        self.runner.run(
            self.compose_command("up", "--detach", "--wait", "--wait-timeout", "120"),
        )

    def compose_command(self, *arguments: str) -> list[str]:
        return [
            "docker",
            "compose",
            "--file",
            str(self.instance_path / "compose.yaml"),
            "--env-file",
            str(self.instance_path / ".env"),
            *arguments,
        ]
