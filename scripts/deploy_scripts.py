#!/usr/bin/env python3
"""Helper scripts for deploying the Financial Tracker"""

import os
import shutil
from typing import Annotated, Any, List
from instance_scripts import StopCommand
from shared.command import Command
from shared.command_collection import CommandCollection
from shared.environment_variable import EnvironmentVariable
from shared.migration_script import MigrationScript
from shared.step import Step

def main():
    """Builds and runs the command collection for this script"""

    commands = CommandCollection("Helper scripts for deploying the Financial Tracker")
    commands.commands.append(CreateCommand())
    commands.commands.append(DeployCommand())
    commands.run()

class CreateCommand(Command):
    """Command class that creates a new instance of the Financial Tracker"""

    name: Annotated[str, "Name for this instance of the Financial Tracker"]
    path: Annotated[str, "Path to the directory where this instance should be created. This directory should not already exist."]
    environment: Annotated[str, "Type of environment for this instance. Must be one of Development or Production"]
    backend_port: Annotated[int, "Port for the backend REST API to be exposed on"]
    frontend_origin: Annotated[str, "Origin where requests from the frontend will originate from"]

    def __init__(self) -> None:
        """Constructs a new instance of this class"""

        super().__init__("create", "Creates a new instance at the specified location")
        self.steps.append(Step("Create Instance Directory", "Instance directory created", self.create_instance_directory))
        self.steps.append(Step("Copy Docker Compose File", "Docker compose file copied", self.copy_compose_file))
        self.steps.append(Step("Create Environment File", "Environment file created", self.create_environment_file))
        self.steps.append(Step("", "", lambda: CopyScripts().run(["--path", self.path])))
        self.steps.append(Step("Create Empty Database", "Empty database created", self.create_empty_database))
        self.steps.append(Step("", "", lambda: ApplyMigrations().run(["--path", self.path])))
        self.steps.append(Step("", "", lambda: BuildContainerImages().run(["--name", self.name])))

    def validate_arguments(self) -> None:
        """Runs additional validation on the provided arguments"""

        if self.environment.lower() != "development" and self.environment.lower() != "production":
            raise ValueError(f"Invalid environment provided: '{self.environment}'")

        if self.run_subprocess(f"docker image inspect backend-{self.name}", throw_on_error=False, suppress_output=True) == 0:
            raise ValueError(f"Instance name {self.name} is already in use")

    def create_instance_directory(self) -> None:
        """Create the main directory for a new instance of the application"""

        if os.path.isdir(self.path):
            raise ValueError(f"Directory '{self.path}' already exists")
        print(f"Creating instance directory at {self.path}")
        os.mkdir(self.path)

    def copy_compose_file(self) -> None:
        """Copies the Docker compose file from the source code directory to the instance directory"""

        compose_file_path = f"{self.path}/compose.yaml"
        print(f"Copying compose.yaml file to {compose_file_path}")
        shutil.copy("../compose.yaml", compose_file_path)

    def create_environment_file(self) -> None:
        """Creates the file with environment variables in the instance directory"""

        environment_file_path = f"{self.path}/.env"

        print(f"Writing the environment file to {environment_file_path}")
        with open(environment_file_path, "w", encoding="utf-8"):
            pass

        environment_variables: List[EnvironmentVariable[Any]] = [
            EnvironmentVariable[str](environment_file_path, "INSTANCE_NAME", self.name),
            EnvironmentVariable[str](environment_file_path, "INSTANCE_DIR", self.path),
            EnvironmentVariable[str](environment_file_path, "ASPNETCORE_ENVIRONMENT", self.environment),
            EnvironmentVariable[int](environment_file_path, "ASPNETCORE_HTTP_PORTS", 8080),
            EnvironmentVariable[int](environment_file_path, "BACKEND_PORT", self.backend_port),
            EnvironmentVariable[str](environment_file_path, "DATABASE_PATH", "/data/database.db"),
            EnvironmentVariable[str](environment_file_path, "LOG_DIRECTORY", "/logs"),
            EnvironmentVariable[str](environment_file_path, "FRONTEND_ORIGIN", self.frontend_origin),
            EnvironmentVariable[int](environment_file_path, "DATABASE_REVISION", 0)
        ]
        for environment_variable in environment_variables:
            environment_variable.write_to_file()

    def create_empty_database(self) -> None:
        """Creates an empty database in the instance directory with all the needed migrations applied"""

        database_file_path = f"{self.path}/database.db"
        print(f"Creating database file at {database_file_path}")
        with open(database_file_path, 'w', encoding="utf-8") as _:
            pass

class DeployCommand(Command):
    """Command class that deploys a new version to an existing instance of the Financial Tracker"""

    path: Annotated[str, "Path to the instance directory"]

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("deploy", "Deploys a new version to an existing instance of the Financial Tracker")
        self.steps.append(Step("Stop Instance", "Instance stopped", lambda: StopCommand().stop_instance(f"{self.path}/compose.yaml")))
        self.steps.append(Step("", "", lambda: CopyScripts().run(["--path", self.path])))
        self.steps.append(Step("", "", lambda: ApplyMigrations().run(["--path", self.path])))
        self.steps.append(Step("", "", lambda: BuildContainerImages().run(
            [
                "--name", 
                EnvironmentVariable.read_from_file(f"{self.path}/.env", "INSTANCE_NAME", str).value
            ])))

    def validate_arguments(self):
        """Runs additional validation on the provided arguments"""

        if not os.path.isdir(self.path):
            raise ValueError(f"Path {self.path} does not point to a valid directory")

class CopyScripts(Command):
    """Command class that copies scripts to the instance directory"""

    path: Annotated[str, "Path to the instance directory to copy the scripts to"]

    def __init__(self) -> None:
        """Constructs a new instance of this class"""

        super().__init__("copy-scripts", "Copies the scripts to an instance directory")
        self.steps.append(Step("Copy Scripts", "Scripts copied", self.copy_scripts))

    def copy_scripts(self) -> None:
        """Copies the scripts to the instance directory"""

        if os.path.isdir(f"{self.path}/scripts"):
            shutil.rmtree(f"{self.path}/scripts")

        print(f"Copying scripts to {self.path}")
        os.mkdir(f"{self.path}/scripts")
        os.mkdir(f"{self.path}/scripts/shared")
        shutil.copy("instance_scripts.py", f"{self.path}/scripts/instance_scripts.py")
        shutil.copy("./shared/argument.py", f"{self.path}/scripts/shared/argument.py")
        shutil.copy("./shared/command.py", f"{self.path}/scripts/shared/command.py")
        shutil.copy("./shared/command_collection.py", f"{self.path}/scripts/shared/command_collection.py")
        shutil.copy("./shared/step.py", f"{self.path}/scripts/shared/step.py")

        self.run_subprocess(f"chmod +x {self.path}/scripts/instance_scripts.py")

class ApplyMigrations(Command):
    """Command class that applies all missing migrations to the database"""

    path: Annotated[str, "Path to the instance directory to apply the missing migrations to"]

    def __init__(self) -> None:
        """Constructs a new instance of this class"""

        super().__init__("apply-migrations", "Applies all the missing migrations to the instance database")
        self.steps.append(Step("Apply Missing Migrations", "Database up to date", self.apply_missing_migrations))

    def apply_missing_migrations(self) -> None:
        """Applies all the missing migrations to the instance database"""

        database_file_path = f"{self.path}/database.db"
        print(f"Applying all missing migrations to database {database_file_path}")

        current_revision: EnvironmentVariable[int] = EnvironmentVariable.read_from_file(f"{self.path}/.env", "DATABASE_REVISION", int)
        print(f"Last migration applied {current_revision.value}")

        migrations_to_run = [migration for migration in MigrationScript.get_all() if migration.id > current_revision.value]
        if len(migrations_to_run) == 0:
            print("No migrations to run")
            return

        upgrade_database_file_path = f"{self.path}/database-upgrade.db"
        print(f"Creating copy of database for migrations at {upgrade_database_file_path}")
        shutil.copy(database_file_path, upgrade_database_file_path)

        for migration in migrations_to_run:
            print(f"Applying migration {migration.file_name}")

            with open(f"{MigrationScript.directory}/{migration.file_name}", "r", encoding="utf-8") as file:
                self.run_subprocess(f'sqlite3 {upgrade_database_file_path}', file.read())

        if not os.path.isdir(f"{self.path}/archive"):
            print(f"Creating the archive directory for old databases at {self.path}/archive")
            os.mkdir(f"{self.path}/archive")

        print(f"Archiving the old database to '{self.path}/archive/database - {current_revision.value}.db'")
        shutil.move(database_file_path, f"{self.path}/archive/database - {current_revision.value}.db")

        print(f"Moving upgraded database from {upgrade_database_file_path} to {database_file_path}")
        shutil.move(upgrade_database_file_path, database_file_path)
        current_revision.value = migrations_to_run[-1].id
        current_revision.write_to_file()

class BuildContainerImages(Command):
    """Command class that builds a new set of container images for the Financial Tracker"""

    name: Annotated[str, "Name for this instance of the Financial Tracker"]

    def __init__(self) -> None:
        """Constructs a new instance of this class"""

        super().__init__("build-containers", "Builds new versions of the container images for the Financial Tracker")
        self.steps.append(Step("Build Containers", "Containers built", self.build_containers))

    def build_containers(self) -> None:
        """Builds the containers for the Financial Tracker using the current source code"""

        print("Building the backend container image")
        self.run_subprocess(f"docker build ../backend -t backend-{self.name}")

if __name__ == "__main__":
    main()
