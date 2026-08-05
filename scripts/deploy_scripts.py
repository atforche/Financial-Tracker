#!/usr/bin/env python3
"""Helper scripts for deploying the Financial Tracker"""

import os
import shutil
import subprocess
import tempfile
from datetime import UTC, datetime
from pathlib import Path
from typing import Annotated

from shared.command import Command
from shared.command_collection import CommandCollection
from shared.configuration import Configuration
from shared.migrator import run_migrator
from shared.release_manifest import ReleaseManifest
from shared.step import Step

RECOVERY_DIRECTORY_NAME = ".rollback"
STAGING_RECOVERY_DIRECTORY_NAME = ".rollback-staging"
RECOVERY_FILE_NAMES = (".env", "compose.yaml", "Caddyfile")
OPTIONAL_RECOVERY_FILE_NAMES = ("release-manifest.json",)
DATABASE_DIRECTORY_NAME = "data"
DATABASE_FILE_NAME = "database.db"


def prepare_database_directory(instance_path: Path) -> None:
    """Creates the writable SQLite directory and migrates the legacy database file."""

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


def main():
    """Builds and runs the command collection for this script"""

    commands = CommandCollection("Helper scripts for deploying the Financial Tracker")
    commands.commands.append(BootstrapCommand())
    commands.commands.append(BootstrapAdminCommand())
    commands.commands.append(DeployCommand())
    commands.commands.append(RollbackCommand())
    commands.run()


class DeployCommand(Command):
    """Transactionally deploys an immutable release to an existing instance."""

    path: Annotated[str, "Path to the instance directory"]
    release_manifest: Annotated[str, "Path to the release manifest"]
    change_configuration: Annotated[
        bool,
        "True to prompt to overwrite existing configuration values, false otherwise",
    ]

    def __init__(self):
        """Constructs a new instance of this class."""

        super().__init__(
            "deploy",
            "Transactionally deploys an immutable release and rolls back on failure",
        )
        self.steps.append(
            Step("Deploy Release", "Release deployed successfully", self.deploy_release)
        )

    def validate_arguments(self):
        """Validates the instance and release artifact paths."""

        if not os.path.isdir(self.path):
            raise ValueError(f"Path {self.path} does not point to a valid directory")
        manifest_path = Path(self.release_manifest).resolve()
        if not manifest_path.is_file():
            raise ValueError(f"Release manifest {manifest_path} does not exist")
        for file_name in ("compose.yaml", "Caddyfile"):
            if not (manifest_path.parent / file_name).is_file():
                raise ValueError(f"Release artifact is missing {file_name}")

    def deploy_release(self) -> None:
        """Deploys the requested release and restores the previous state on failure."""

        instance_path = Path(self.path).resolve()
        manifest_path = Path(self.release_manifest).resolve()
        manifest = ReleaseManifest.read(manifest_path)
        configuration = Configuration.build_from_existing_instance(
            str(instance_path), self.change_configuration
        )
        configuration.path = str(instance_path)
        configuration.backend_image = manifest.backend_image
        configuration.frontend_image = manifest.frontend_image
        configuration.migrator_image = manifest.migrator_image

        TransactionalDeployment(instance_path).deploy(configuration, manifest_path)


class BootstrapAdminCommand(Command):
    """Creates the first administrator invitation for an existing instance."""

    path: Annotated[str, "Path to the deployed instance directory"]
    email: Annotated[str, "Email address for the bootstrap administrator invitation"]

    def __init__(self):
        """Constructs a bootstrap administrator command."""

        super().__init__(
            "bootstrap-admin",
            "Creates the first administrator invitation when no active administrator exists",
        )
        self.steps.append(
            Step(
                "Bootstrap Administrator",
                "Bootstrap administrator invitation created",
                self.bootstrap_admin,
            )
        )

    def validate_arguments(self) -> None:
        """Validates the existing instance path and email argument."""

        if not os.path.isdir(self.path):
            raise ValueError(f"Path {self.path} does not point to a valid directory")
        if not self.email.strip():
            raise ValueError("Email must not be empty")
        configuration = Configuration.build_from_existing_instance(
            str(Path(self.path).resolve()), False
        )
        if not configuration.migrator_image:
            raise ValueError("The instance migrator image is not configured")

    def bootstrap_admin(self) -> None:
        """Runs the application-owned bootstrap capability in the migrator image."""

        instance_path = Path(self.path).resolve()
        configuration = Configuration.build_from_existing_instance(
            str(instance_path), False
        )
        run_migrator(
            configuration.migrator_image,
            instance_path / DATABASE_DIRECTORY_NAME,
            {"BOOTSTRAP_ADMIN_EMAIL": self.email.strip()},
        )


class BootstrapCommand(Command):
    """Creates and starts the first release for an otherwise empty instance path."""

    path: Annotated[str, "New path to create for the instance"]
    release_manifest: Annotated[str, "Path to the release manifest"]

    def __init__(self):
        """Constructs a new instance bootstrap command."""

        super().__init__(
            "bootstrap", "Bootstraps a new instance from an immutable release"
        )
        self.steps.append(
            Step(
                "Bootstrap Instance",
                "Instance bootstrapped successfully",
                self.bootstrap_instance,
            )
        )

    def validate_arguments(self) -> None:
        """Validates that bootstrapping cannot overwrite an existing instance."""

        instance_path = Path(self.path).resolve()
        manifest_path = Path(self.release_manifest).resolve()
        if instance_path == Path("/"):
            raise ValueError("The filesystem root cannot be used as an instance path")
        if instance_path.exists():
            raise ValueError(f"Bootstrap path {instance_path} already exists")
        if not instance_path.parent.is_dir():
            raise ValueError(f"Parent path {instance_path.parent} does not exist")
        if not manifest_path.is_file():
            raise ValueError(f"Release manifest {manifest_path} does not exist")
        for file_name in ("compose.yaml", "Caddyfile"):
            if not (manifest_path.parent / file_name).is_file():
                raise ValueError(f"Release artifact is missing {file_name}")

    def bootstrap_instance(self) -> None:
        """Installs the initial state, migrations, and running services atomically."""

        instance_path = Path(self.path).resolve()
        manifest_path = Path(self.release_manifest).resolve()
        manifest = ReleaseManifest.read(manifest_path)
        configuration = Configuration.build_from_environment(
            str(instance_path),
            manifest.backend_image,
            manifest.frontend_image,
            manifest.migrator_image,
        )
        deployment = TransactionalDeployment(instance_path)

        instance_path.mkdir(mode=0o750)
        try:
            (instance_path / "logs").mkdir(mode=0o777)
            (instance_path / "archive").mkdir(mode=0o750)
            prepare_database_directory(instance_path)

            deployment.validate_release(configuration, manifest_path.parent)
            deployment.pull_images(configuration)
            deployment.install_release_files(configuration, manifest_path)
            ApplyMigrations(configuration).run([])
            deployment.start_instance()
        except Exception:
            deployment.stop_instance(throw_on_error=False)
            shutil.rmtree(instance_path)
            raise


class RollbackCommand(Command):
    """Restores the immediately previous healthy deployment."""

    path: Annotated[str, "Path to the instance directory"]

    def __init__(self) -> None:
        """Constructs a new instance of this class."""

        super().__init__(
            "rollback",
            "Restores the previous healthy release and retains the current release for recovery",
        )
        self.steps.append(
            Step(
                "Rollback Release",
                "Release rolled back successfully",
                self.rollback_release,
            )
        )

    def validate_arguments(self) -> None:
        """Validates that the instance and recovery point exist."""

        instance_path = Path(self.path).resolve()
        if not instance_path.is_dir():
            raise ValueError(
                f"Path {instance_path} does not point to a valid directory"
            )
        if not (instance_path / RECOVERY_DIRECTORY_NAME).is_dir():
            raise ValueError(
                f"Instance {instance_path} does not have a rollback recovery point"
            )

    def rollback_release(self) -> None:
        """Rolls the instance back to its retained recovery point."""

        TransactionalDeployment(Path(self.path).resolve()).rollback()


class TransactionalDeployment:
    """Coordinates image preparation, migration, health verification, and recovery."""

    def __init__(self, instance_path: Path) -> None:
        """Constructs a deployment coordinator for an instance directory."""

        self.instance_path = instance_path
        self.recovery_path = instance_path / RECOVERY_DIRECTORY_NAME
        self.staging_recovery_path = instance_path / STAGING_RECOVERY_DIRECTORY_NAME

    def deploy(self, configuration: Configuration, release_manifest_path: Path) -> None:
        """Deploys a release, automatically restoring the previous healthy state on failure."""

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
            ApplyMigrations(configuration).run([])
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
        """Swaps the active deployment with the retained recovery point."""

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
        """Refuses to overwrite evidence from an incomplete deployment transaction."""

        if self.staging_recovery_path.exists():
            raise RuntimeError(
                f"Incomplete deployment recovery point exists at {self.staging_recovery_path}; inspect it before retrying"
            )

    def pull_images(self, configuration: Configuration) -> None:
        """Pulls every immutable image before taking the instance offline."""

        for image in (
            configuration.backend_image,
            configuration.frontend_image,
            configuration.migrator_image,
        ):
            subprocess.run(["docker", "pull", image], check=True)

    def ensure_images(self, configuration: Configuration) -> None:
        """Ensures rollback images exist locally, pulling only missing references."""

        for image in (
            configuration.backend_image,
            configuration.frontend_image,
            configuration.migrator_image,
        ):
            result = subprocess.run(
                ["docker", "image", "inspect", image],
                check=False,
                stdout=subprocess.DEVNULL,
                stderr=subprocess.DEVNULL,
            )
            if result.returncode != 0:
                subprocess.run(["docker", "pull", image], check=True)

    def validate_release(
        self, configuration: Configuration, release_directory: Path
    ) -> None:
        """Validates the candidate Compose configuration before stopping the instance."""

        with tempfile.NamedTemporaryFile(
            mode="w",
            prefix=".deployment-environment-",
            dir=self.instance_path,
            delete=False,
        ) as file:
            environment_file_path = Path(file.name)
        try:
            configuration.write_to_file(str(environment_file_path))
            subprocess.run(
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
                check=True,
            )
        finally:
            environment_file_path.unlink(missing_ok=True)

    def create_recovery_point(self, destination: Path) -> None:
        """Copies all mutable release state into a private recovery directory."""

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
        """Installs the release configuration and runtime definitions."""

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
        """Restores configuration, runtime definitions, and database from a recovery point."""

        for file_name in RECOVERY_FILE_NAMES:
            recovery_file = source / file_name
            instance_file = self.instance_path / file_name
            if recovery_file.exists():
                shutil.copy2(recovery_file, instance_file)
            else:
                instance_file.unlink(missing_ok=True)
        for file_name in OPTIONAL_RECOVERY_FILE_NAMES:
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
        """Retains the replaced deployment as the next rollback target."""

        if self.recovery_path.exists():
            shutil.rmtree(self.recovery_path)
        os.replace(self.staging_recovery_path, self.recovery_path)

    def stop_instance(self, throw_on_error: bool = True) -> None:
        """Stops the Compose project without deleting persistent volumes."""

        result = subprocess.run(self.compose_command("down"), check=False)
        if throw_on_error and result.returncode != 0:
            raise RuntimeError("The running instance could not be stopped")

    def start_instance(self) -> None:
        """Starts the Compose project and waits for service health checks."""

        subprocess.run(
            self.compose_command("up", "--detach", "--wait", "--wait-timeout", "120"),
            check=True,
        )

    def compose_command(self, *arguments: str) -> list[str]:
        """Builds a Compose command bound to the instance files."""

        return [
            "docker",
            "compose",
            "--file",
            str(self.instance_path / "compose.yaml"),
            "--env-file",
            str(self.instance_path / ".env"),
            *arguments,
        ]


class ApplyMigrations(Command):
    """Command class that applies all missing migrations to the database"""

    configuration: Configuration

    def __init__(self, configuration: Configuration) -> None:
        """Constructs a new instance of this class

        Args:
            configuration: The configuration for the instance
        """

        super().__init__(
            "apply-migrations",
            "Applies all the missing migrations to the instance database",
        )
        self.configuration = configuration
        self.steps.append(
            Step(
                "Apply Missing Migrations",
                "Database up to date",
                self.apply_missing_migrations,
            )
        )

    def apply_missing_migrations(self) -> None:
        """Applies all the missing migrations to the instance database"""

        print(
            f"Applying all missing migrations to database {self.configuration.get_database_file_path()}"
        )
        database_file_path = self.configuration.get_database_file_path()
        upgrade_directory = tempfile.mkdtemp(
            prefix=".migration-", dir=self.configuration.path
        )
        os.chmod(upgrade_directory, 0o777)
        upgrade_database_file_path = f"{upgrade_directory}/database.db"
        shutil.copy(database_file_path, upgrade_database_file_path)
        os.chmod(upgrade_database_file_path, 0o666)

        try:
            run_migrator(self.configuration.migrator_image, Path(upgrade_directory))
        except Exception:
            shutil.rmtree(upgrade_directory)
            raise

        archive_directory = f"{self.configuration.path}/archive"
        os.makedirs(archive_directory, exist_ok=True)
        archive_timestamp = datetime.now(UTC).strftime("%Y%m%dT%H%M%SZ")
        archive_database_file_path = (
            f"{archive_directory}/database-{archive_timestamp}.db"
        )
        shutil.copy2(database_file_path, archive_database_file_path)
        os.replace(upgrade_database_file_path, database_file_path)
        shutil.rmtree(upgrade_directory)


if __name__ == "__main__":
    main()
