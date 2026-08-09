#!/usr/bin/env python3
"""Helper scripts for debugging the Financial Tracker"""

import getpass
import os
import re
import secrets
import shutil
import subprocess
import tempfile
from pathlib import Path
from typing import Annotated
from urllib.parse import urlsplit
from uuid import uuid4

from backup_scripts import BACKUP_TAG, BackupCommand
from shared.command import Command
from shared.command_collection import CommandCollection
from shared.configuration import Configuration, Environment
from shared.restic import run_restic
from shared.step import Step

DEBUG_BACKEND_PORT = 8081
DEBUG_FRONTEND_PORT = 3001
DEBUG_SUBJECT = "local-developer"
PROJECT_ROOT = Path(__file__).resolve().parent.parent


def main():
    """Builds and runs the command collection for this script"""

    commands = CommandCollection("Helper scripts for debugging the Financial Tracker")
    commands.commands.append(CreateDebugEnvironment())
    commands.commands.append(UpgradeDebugEnvironment())
    commands.commands.append(RestoreDebugDatabase())
    commands.commands.append(DestroyDebugEnvironment())
    commands.commands.append(StartDebugStack())
    commands.commands.append(StopDebugStack())
    commands.commands.append(DestroyDebugStack())
    commands.commands.append(RunDebugFrontend())
    commands.commands.append(RunDebugBackend())
    commands.run()


def get_debug_configuration() -> Configuration:
    """Gets the configuration for the debug environment"""

    return Configuration(
        name="Debug",
        path=str(PROJECT_ROOT / "debug"),
        environment=Environment.DEVELOPMENT,
        public_origin="https://localhost:3001",
        google_client_id=os.environ.get("GOOGLE_CLIENT_ID", ""),
        google_client_secret=os.environ.get("GOOGLE_CLIENT_SECRET", ""),
        auth_secret=os.environ.get("AUTH_SECRET", ""),
        backend_image="backend-Debug",
        frontend_image="frontend-Debug",
        migrator_image="migrator-Debug",
    )


def get_debug_environment() -> dict[str, str]:
    """Returns the current environment augmented with the debug instance settings."""

    environment = os.environ.copy()
    environment_file_path = get_debug_configuration().get_environment_file_path()
    if not os.path.isfile(environment_file_path):
        raise RuntimeError(
            "Debug configuration is missing. Run './debug_scripts.py create' before starting the application."
        )
    with open(environment_file_path, encoding="utf-8") as file:
        for line in file:
            variable_match = re.fullmatch(r'([A-Z][A-Z0-9_]*)="(.*)"\n?', line)
            if variable_match is not None:
                environment[variable_match.group(1)] = variable_match.group(2)
    return environment


def create_debug_environment_file() -> None:
    """Creates a local-only debug configuration with a unique session secret."""

    configuration = get_debug_configuration()
    environment_file_path = Path(configuration.get_environment_file_path())
    if environment_file_path.exists():
        return

    template_path = PROJECT_ROOT / "scripts" / "debug.env.example"
    template = template_path.read_text(encoding="utf-8")
    environment_file_path.write_text(
        template.replace(
            'AUTH_SECRET=""', f'AUTH_SECRET="{secrets.token_urlsafe(48)}"'
        ),
        encoding="utf-8",
    )
    os.chmod(environment_file_path, 0o600)


def prepare_debug_configuration() -> None:
    """Creates the local debug directory and configuration when they are absent."""

    os.makedirs(get_debug_configuration().path, exist_ok=True)
    create_debug_environment_file()


class CreateDebugEnvironment(Command):
    """Command class that creates the debug environment"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("create", "Creates the debug environment")
        self.steps.append(
            Step(
                "Create Debug Directory", "Debug directory ready", self.create_directory
            )
        )
        self.steps.append(
            Step(
                "Create Debug Configuration",
                "Debug configuration ready",
                create_debug_environment_file,
            )
        )
        self.steps.append(
            Step("Create Debug Database", "Debug database ready", self.create_database)
        )
        self.steps.append(Step("", "", lambda: ApplyDebugMigrations().run([])))

    def create_directory(self) -> None:
        """Creates the debug directory and its runtime log directory."""

        configuration = get_debug_configuration()
        os.makedirs(configuration.path, exist_ok=True)
        os.makedirs(f"{configuration.path}/data", exist_ok=True)
        os.makedirs(f"{configuration.path}/logs", exist_ok=True)

    def create_database(self) -> None:
        """Creates the writable debug database file."""

        database_path = get_debug_configuration().get_database_file_path()
        os.makedirs(os.path.dirname(database_path), exist_ok=True)
        if not os.path.exists(database_path):
            with open(database_path, "w", encoding="utf-8"):
                pass
        os.chmod(database_path, 0o666)


class UpgradeDebugEnvironment(Command):
    """Command class that upgrades the debug environment"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("upgrade", "Upgrades the debug environment")
        self.steps.append(Step("", "", lambda: ApplyDebugMigrations().run([])))


class RestoreDebugDatabase(Command):
    """Restores a downloaded encrypted Restic backup into native debug data."""

    repository: Annotated[str | None, "Path to a downloaded local Restic repository"]
    s3_uri: Annotated[
        str | None, "S3 bucket or prefix containing the Restic repository"
    ]
    aws_profile: Annotated[str | None, "AWS CLI profile used for the S3 download"]

    def __init__(self):
        """Constructs a debug database restoration command."""

        super().__init__(
            "restore",
            "Restores a downloaded encrypted backup into the native debug database",
        )
        self.repository = None
        self.s3_uri = None
        self.aws_profile = None
        self.steps.append(
            Step(
                "Restore Debug Database",
                "Debug database restored",
                self.restore,
            )
        )

    def validate_arguments(self) -> None:
        """Validates the source and existing debug environment."""

        if (self.repository is None) == (self.s3_uri is None):
            raise ValueError("Exactly one of --repository or --s3-uri must be provided")

        if self.s3_uri is not None:
            self.s3_uri = self.s3_uri.strip()
            parsed_s3_uri = urlsplit(self.s3_uri)
            if (
                parsed_s3_uri.scheme != "s3"
                or parsed_s3_uri.netloc == ""
                or parsed_s3_uri.query != ""
                or parsed_s3_uri.fragment != ""
            ):
                raise ValueError(
                    f"Value {self.s3_uri} is not a valid S3 bucket or prefix"
                )
            if shutil.which("aws") is None:
                raise ValueError(
                    "The AWS CLI is required to download an S3 Restic repository"
                )

        if self.repository is not None:
            repository_path = Path(self.repository).expanduser().resolve()
            if not repository_path.is_dir():
                raise ValueError(
                    f"Path {repository_path} does not point to a valid Restic repository"
                )
            if not (repository_path / "config").is_file():
                raise ValueError(
                    f"Path {repository_path} does not contain a Restic repository"
                )
            self.repository = str(repository_path)

        if self.aws_profile is not None:
            self.aws_profile = self.aws_profile.strip()
            if self.aws_profile == "":
                raise ValueError("--aws-profile cannot be empty")

        configuration = get_debug_configuration()
        if not Path(configuration.get_environment_file_path()).is_file():
            raise ValueError(
                "Debug configuration is missing. Run './scripts/debug_scripts.py create' first."
            )
        if not Path(f"{configuration.path}/data").is_dir():
            raise ValueError(
                "Debug data directory is missing. Run './scripts/debug_scripts.py create' first."
            )

    def get_aws_profile_arguments(self) -> list[str]:
        """Returns the AWS CLI arguments selecting the requested profile."""

        if self.aws_profile is None:
            return []
        return ["--profile", self.aws_profile]

    def run_aws(
        self,
        arguments: list[str],
        check: bool = True,
        capture_output: bool = False,
    ) -> subprocess.CompletedProcess[str]:
        """Runs an AWS CLI command using the selected profile."""

        return subprocess.run(
            ["aws", *arguments, *self.get_aws_profile_arguments()],
            check=check,
            capture_output=capture_output,
            text=True,
        )

    def ensure_aws_login(self) -> None:
        """Ensures that the selected AWS CLI profile has usable credentials."""

        identity_check = self.run_aws(
            ["sts", "get-caller-identity"], check=False, capture_output=True
        )
        if identity_check.returncode == 0:
            return

        login_mode = self.run_aws(
            ["configure", "get", "sso_session"],
            check=False,
            capture_output=True,
        )
        if login_mode.stdout.strip() == "":
            login_mode = self.run_aws(
                ["configure", "get", "sso_start_url"],
                check=False,
                capture_output=True,
            )

        if login_mode.stdout.strip() != "":
            self.run_aws(["sso", "login"], check=True)
        else:
            self.run_aws(["login"], check=True)

        identity_check = self.run_aws(
            ["sts", "get-caller-identity"], check=False, capture_output=True
        )
        if identity_check.returncode != 0:
            raise RuntimeError(
                "AWS authentication completed but the selected profile could not access AWS"
            )

    def download_s3_repository(self, destination: Path) -> None:
        """Downloads the complete Restic repository into a local directory."""

        if self.s3_uri is None:
            raise RuntimeError("An S3 source was not configured")
        self.ensure_aws_login()
        self.run_aws(
            ["s3", "sync", self.s3_uri, str(destination)],
            check=True,
        )

        if not (destination / "config").is_file():
            raise RuntimeError(
                "The S3 source did not contain a complete Restic repository"
            )

    @staticmethod
    def get_restic_password() -> str:
        """Gets the Restic password from the environment or an interactive prompt."""

        password = os.environ.get("RESTIC_PASSWORD", "")
        if password != "":
            return password

        password = getpass.getpass("Restic password: ")
        if password == "":
            raise ValueError("A Restic password must be provided")
        return password

    @staticmethod
    def get_rollback_path(data_directory: Path) -> Path:
        """Creates a unique path for preserving the current debug database."""

        descriptor, path = tempfile.mkstemp(
            prefix="database.db.before-restore-",
            suffix=f"-{uuid4().hex}.bak",
            dir=data_directory,
        )
        os.close(descriptor)
        return Path(path)

    @staticmethod
    def get_migration_environment() -> dict[str, str]:
        """Returns the guarded development authentication settings for migration."""

        environment = get_debug_environment()
        return {
            "AUTH_MODE": "development",
            "DEVELOPMENT_AUTH_SUBJECT": environment.get(
                "DEVELOPMENT_AUTH_SUBJECT", "local-developer"
            ),
            "DEVELOPMENT_AUTH_ADDITIONAL_SUBJECTS": environment.get(
                "DEVELOPMENT_AUTH_ADDITIONAL_SUBJECTS", "local-standard,local-read-only"
            ),
            "DEVELOPMENT_AUTH_READ_ONLY_SUBJECTS": environment.get(
                "DEVELOPMENT_AUTH_READ_ONLY_SUBJECTS", "local-read-only"
            ),
        }

    def restore(self) -> None:
        """Restores, validates, migrates, and atomically installs the debug database."""

        configuration = get_debug_configuration()
        data_directory = Path(configuration.path) / "data"
        database_path = data_directory / "database.db"
        if database_path.exists() and not database_path.is_file():
            raise ValueError(f"Debug database path is not a file: {database_path}")

        password = self.get_restic_password()
        with tempfile.TemporaryDirectory(
            prefix=".financial-tracker-repository-", dir=data_directory
        ) as repository_directory:
            if self.repository is None:
                self.download_s3_repository(Path(repository_directory))
                repository_path = Path(repository_directory)
            else:
                repository_path = Path(self.repository)

            with tempfile.TemporaryDirectory(
                prefix=".financial-tracker-restore-", dir=data_directory
            ) as temporary_directory:
                restore_directory = Path(temporary_directory)
                run_restic(
                    ["check"],
                    repository=repository_path,
                    password=password,
                    pass_aws_credentials=False,
                )
                run_restic(
                    [
                        "restore",
                        "latest",
                        "--tag",
                        BACKUP_TAG,
                        "--target",
                        "/restore",
                        "--verify",
                    ],
                    repository=repository_path,
                    volumes=((restore_directory, "/restore", False),),
                    password=password,
                    pass_aws_credentials=False,
                )

                restored_database = restore_directory / "snapshot" / "database.db"
                if not restored_database.is_file():
                    raise RuntimeError(
                        "Restic did not restore the expected database file"
                    )

                staged_database = restore_directory / "database.db"
                shutil.copy2(restored_database, staged_database)
                os.chmod(restore_directory, 0o777)
                os.chmod(staged_database, 0o666)
                BackupCommand.validate_database(staged_database)

                migration_environment = self.get_migration_environment()
                migration_environment["DATABASE_PATH"] = str(staged_database)
                ApplyDebugMigrations().apply_migrations_to(
                    staged_database, migration_environment
                )
                BackupCommand.validate_database(staged_database)

                rollback_path: Path | None = None
                if database_path.is_file():
                    rollback_path = self.get_rollback_path(data_directory)
                    try:
                        shutil.copy2(database_path, rollback_path)
                    except Exception:
                        rollback_path.unlink(missing_ok=True)
                        raise

                os.replace(staged_database, database_path)
                os.chmod(database_path, 0o666)
                if rollback_path is not None:
                    print(f"Previous debug database preserved at {rollback_path}")


class ApplyDebugMigrations(Command):
    """Applies compiled EF migrations to the debug database."""

    def __init__(self):
        """Constructs a new instance of this class."""

        super().__init__(
            "apply-migrations", "Applies compiled EF migrations to the debug database"
        )
        self.steps.append(
            Step(
                "Apply Debug Migrations",
                "Debug database migrated",
                self.apply_migrations,
            )
        )

    def apply_migrations(self) -> None:
        """Runs the migrator project against the debug database."""

        environment = get_debug_environment()
        self.apply_migrations_to(
            Path(get_debug_configuration().get_database_file_path()), environment
        )

    def apply_migrations_to(
        self, database_path: Path, environment: dict[str, str] | None = None
    ) -> None:
        """Runs the migrator project against a selected native database file."""

        migration_environment = environment or get_debug_environment()
        migration_environment["DATABASE_PATH"] = str(database_path)
        os.chdir(PROJECT_ROOT)
        self.run_subprocess(
            "dotnet run --project backend/Migrator/Migrator.csproj",
            env=migration_environment,
        )


class DestroyDebugEnvironment(Command):
    """Command class that destroys the debug environment"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("destroy", "Destroys the debug environment")
        self.steps.append(
            Step(
                "Destroy Debug Environment",
                "Debug environment destroyed",
                lambda: shutil.rmtree(get_debug_configuration().path),
            )
        )


class StartDebugStack(Command):
    """Starts the complete local container stack."""

    def __init__(self):
        """Constructs a new instance of this class."""

        super().__init__(
            "stack-up", "Builds and starts the complete local container stack"
        )
        self.steps.append(
            Step(
                "Prepare Debug Configuration",
                "Debug configuration ready",
                prepare_debug_configuration,
            )
        )
        self.steps.append(
            Step("Start Debug Stack", "Debug stack running", self.start_stack)
        )

    def start_stack(self) -> None:
        """Builds and starts the local Docker Compose stack."""

        self.run_subprocess(
            f"docker compose -f {PROJECT_ROOT / 'compose.dev.yaml'} up --build --detach --wait"
        )


class StopDebugStack(Command):
    """Stops the complete local container stack while retaining its data."""

    def __init__(self):
        """Constructs a new instance of this class."""

        super().__init__(
            "stack-down", "Stops the local container stack and retains its data"
        )
        self.steps.append(
            Step("Stop Debug Stack", "Debug stack stopped", self.stop_stack)
        )

    def stop_stack(self) -> None:
        """Stops the local Docker Compose stack without deleting its volumes."""

        self.run_subprocess(
            f"docker compose -f {PROJECT_ROOT / 'compose.dev.yaml'} down"
        )


class DestroyDebugStack(Command):
    """Stops the complete local container stack and removes its data."""

    def __init__(self):
        """Constructs a new instance of this class."""

        super().__init__(
            "stack-destroy", "Stops the local container stack and removes its data"
        )
        self.steps.append(
            Step(
                "Destroy Debug Stack",
                "Debug stack and data removed",
                self.destroy_stack,
            )
        )

    def destroy_stack(self) -> None:
        """Stops the local Docker Compose stack and deletes its named volumes."""

        self.run_subprocess(
            f"docker compose -f {PROJECT_ROOT / 'compose.dev.yaml'} down --volumes"
        )


class RunDebugFrontend(Command):
    """Command class that runs the frontend for the debug environment"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("frontend", "Runs the frontend for the debug environment")
        self.steps.append(
            Step("Run Debug Frontend", "Debug frontend running", self.run_frontend)
        )

    def run_frontend(self):
        """Runs the frontend for the debug environment"""

        os.chdir(PROJECT_ROOT / "frontend")
        environment = get_debug_environment()
        environment["API_URL"] = f"http://localhost:{DEBUG_BACKEND_PORT}"
        self.run_subprocess(
            f"npx next dev --port {DEBUG_FRONTEND_PORT}", env=environment
        )


class RunDebugBackend(Command):
    """Command class that runs the backend for the debug environment"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("backend", "Runs the backend for the debug environment")
        self.steps.append(
            Step("Run Debug Backend", "Debug backend running", self.run_backend)
        )

    def run_backend(self):
        """Runs the backend for the debug environment"""

        configuration = get_debug_configuration()
        environment = get_debug_environment()
        environment["ASPNETCORE_ENVIRONMENT"] = configuration.environment.value
        environment["ASPNETCORE_HTTP_PORTS"] = str(DEBUG_BACKEND_PORT)
        environment["DATABASE_PATH"] = configuration.get_database_file_path()
        environment["LOG_DIRECTORY"] = f"{configuration.path}/logs"
        environment["FRONTEND_ORIGIN"] = f"http://localhost:{DEBUG_FRONTEND_PORT}"
        os.chdir(PROJECT_ROOT / "backend" / "Rest")
        self.run_subprocess("dotnet run", env=environment)


if __name__ == "__main__":
    main()
