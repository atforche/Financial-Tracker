#!/usr/bin/env python3
"""Helper scripts for debugging the Financial Tracker"""

import os
import re
import secrets
import shutil
from pathlib import Path

from shared.command import Command
from shared.command_collection import CommandCollection
from shared.configuration import Configuration, Environment
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
        google_allowed_subjects=os.environ.get("GOOGLE_ALLOWED_SUBJECTS", ""),
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

        environment = os.environ.copy()
        environment["DATABASE_PATH"] = (
            get_debug_configuration().get_database_file_path()
        )
        os.chdir(PROJECT_ROOT)
        self.run_subprocess(
            "dotnet run --project backend/Migrator/Migrator.csproj", env=environment
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
