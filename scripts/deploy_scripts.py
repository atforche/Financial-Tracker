#!/usr/bin/env python3
"""Helper scripts for deploying the Financial Tracker"""

import os
import shutil
from typing import Annotated
from instance_scripts import StopCommand
from shared.configuration import Configuration
from shared.command import Command
from shared.command_collection import CommandCollection
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

    configuration: Configuration

    def __init__(self) -> None:
        """Constructs a new instance of this class"""

        super().__init__("create", "Creates a new instance at the specified location")
        self.steps.append(Step("Create Configuration", "Configuration created", self.build_configuration))
        self.steps.append(Step("Create Instance Directory", "Instance directory created", self.create_instance_directory))
        self.steps.append(Step("Copy Docker Compose File", "Docker compose file copied", self.copy_compose_file))
        self.steps.append(Step("Create Environment File", "Environment file created", self.create_environment_file))
        self.steps.append(Step("", "", lambda: CopyScripts(self.configuration).run([])))
        self.steps.append(Step("Create Empty Database", "Empty database created", self.create_empty_database))
        self.steps.append(Step("", "", lambda: ApplyMigrations(self.configuration).run([])))
        self.steps.append(Step("", "", lambda: BuildContainerImages(self.configuration).run([])))
        
    def build_configuration(self) -> None:
        """Builds the configuration from user input"""
        
        self.configuration = Configuration.build_from_user_input({}, False)

        if self.run_subprocess(f"docker image inspect backend-{self.configuration.name}", throw_on_error=False, suppress_output=True) == 0:
            raise ValueError(f"Instance name {self.configuration.name} is already in use")
        
        if self.run_subprocess(f"docker image inspect frontend-{self.configuration.name}", throw_on_error=False, suppress_output=True) == 0:
            raise ValueError(f"Instance name {self.configuration.name} is already in use")
        
        if os.path.isdir(self.configuration.path):
            raise ValueError(f"Directory '{self.configuration.path}' already exists")

    def create_instance_directory(self) -> None:
        """Create the main directory for a new instance of the application"""

        print(f"Creating instance directory at {self.configuration.path}")
        os.mkdir(self.configuration.path)

    def copy_compose_file(self) -> None:
        """Copies the Docker compose file from the source code directory to the instance directory"""

        print(f"Copying compose.yaml file to {self.configuration.get_compose_file_path()}")
        shutil.copy("../compose.yaml", self.configuration.get_compose_file_path())

    def create_environment_file(self) -> None:
        """Creates the file with environment variables in the instance directory"""

        print(f"Writing the environment file to {self.configuration.get_environment_file_path()}")
        self.configuration.write_to_file()

    def create_empty_database(self) -> None:
        """Creates an empty database in the instance directory with all the needed migrations applied"""
    
        print(f"Creating database file at {self.configuration.get_database_file_path()}")
        with open(self.configuration.get_database_file_path(), 'w', encoding="utf-8") as _:
            pass

class DeployCommand(Command):
    """Command class that deploys a new version to an existing instance of the Financial Tracker"""

    configuration: Configuration
    path: Annotated[str, "Path to the instance directory"]
    change_configuration: Annotated[bool, "True to prompt to overwrite existing configuration values, false otherwise"]

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("deploy", "Deploys a new version to an existing instance of the Financial Tracker")
        self.steps.append(Step("Build Configuration", "Configuration built", self.build_configuration))
        self.steps.append(Step("Stop Instance", "Instance stopped", lambda: StopCommand().stop_instance(self.configuration.get_compose_file_path())))
        self.steps.append(Step("", "", lambda: CopyScripts(self.configuration).run([])))
        self.steps.append(Step("", "", lambda: ApplyMigrations(self.configuration).run([])))
        self.steps.append(Step("", "", lambda: BuildContainerImages(self.configuration).run([])))

    def validate_arguments(self):
        """Runs additional validation on the provided arguments"""

        if not os.path.isdir(self.path):
            raise ValueError(f"Path {self.path} does not point to a valid directory")

    def build_configuration(self) -> None:
        """Builds the configuration from the existing instance directory, prompting for new options as necessary"""

        self.configuration = Configuration.build_from_existing_instance(self.path, self.change_configuration)

class CopyScripts(Command):
    """Command class that copies scripts to the instance directory"""

    configuration: Configuration

    def __init__(self, configuration: Configuration) -> None:
        """Constructs a new instance of this class
        
        Args:
            configuration: The configuration for the instance
        """

        super().__init__("copy-scripts", "Copies the scripts to an instance directory")
        self.configuration = configuration
        self.steps.append(Step("Copy Scripts", "Scripts copied", self.copy_scripts))

    def copy_scripts(self) -> None:
        """Copies the scripts to the instance directory"""

        if os.path.isdir(self.configuration.get_scripts_directory_path()):
            shutil.rmtree(self.configuration.get_scripts_directory_path())

        print(f"Copying scripts to {self.configuration.get_scripts_directory_path()}")
        os.mkdir(self.configuration.get_scripts_directory_path())
        os.mkdir(f"{self.configuration.get_scripts_directory_path()}/shared")
        shutil.copy("instance_scripts.py", f"{self.configuration.get_scripts_directory_path()}/instance_scripts.py")
        shutil.copy("./shared/argument.py", f"{self.configuration.get_scripts_directory_path()}/shared/argument.py")
        shutil.copy("./shared/environment_variable.py", f"{self.configuration.get_scripts_directory_path()}/shared/environment_variable.py")
        shutil.copy("./shared/command.py", f"{self.configuration.get_scripts_directory_path()}/shared/command.py")
        shutil.copy("./shared/command_collection.py", f"{self.configuration.get_scripts_directory_path()}/shared/command_collection.py")
        shutil.copy("./shared/configuration.py", f"{self.configuration.get_scripts_directory_path()}/shared/configuration.py")
        shutil.copy("./shared/step.py", f"{self.configuration.get_scripts_directory_path()}/shared/step.py")
        self.run_subprocess(f"chmod +x {self.configuration.get_scripts_directory_path()}/instance_scripts.py")

class ApplyMigrations(Command):
    """Command class that applies all missing migrations to the database"""

    configuration: Configuration

    def __init__(self, configuration: Configuration) -> None:
        """Constructs a new instance of this class
        
        Args:
            configuration: The configuration for the instance
        """

        super().__init__("apply-migrations", "Applies all the missing migrations to the instance database")
        self.configuration = configuration
        self.steps.append(Step("Apply Missing Migrations", "Database up to date", self.apply_missing_migrations))

    def apply_missing_migrations(self) -> None:
        """Applies all the missing migrations to the instance database"""
    
        print(f"Applying all missing migrations to database {self.configuration.get_database_file_path()}")

        print(f"Last migration applied {self.configuration.database_revision}")

        migrations_to_run = [migration for migration in MigrationScript.get_all() if migration.id > self.configuration.database_revision]
        if len(migrations_to_run) == 0:
            print("No migrations to run")
            return

        upgrade_database_file_path = f"{self.configuration.path}/database-upgrade.db"
        print(f"Creating copy of database for migrations at {upgrade_database_file_path}")
        shutil.copy(self.configuration.get_database_file_path(), upgrade_database_file_path)

        for migration in migrations_to_run:
            print(f"Applying migration {migration.file_name}")

            with open(f"{MigrationScript.directory}/{migration.file_name}", "r", encoding="utf-8") as file:
                self.run_subprocess(f'sqlite3 {upgrade_database_file_path}', file.read())

        if not os.path.isdir(f"{self.configuration.path}/archive"):
            print(f"Creating the archive directory for old databases at {self.configuration.path}/archive")
            os.mkdir(f"{self.configuration.path}/archive")

        print(f"Archiving the old database to '{self.configuration.path}/archive/database - {self.configuration.database_revision}.db'")
        shutil.move(self.configuration.get_database_file_path(), f"{self.configuration.path}/archive/database - {self.configuration.database_revision}.db")

        print(f"Moving upgraded database from {upgrade_database_file_path} to {self.configuration.get_database_file_path()}")
        shutil.move(upgrade_database_file_path, self.configuration.get_database_file_path())
        self.configuration.database_revision = migrations_to_run[-1].id
        self.configuration.write_to_file()

class BuildContainerImages(Command):
    """Command class that builds a new set of container images for the Financial Tracker"""

    configuration: Configuration

    def __init__(self, configuration: Configuration) -> None:
        """Constructs a new instance of this class
        
        Args:
            configuration: The configuration for the instance
        """

        super().__init__("build-containers", "Builds new versions of the container images for the Financial Tracker")
        self.configuration = configuration
        self.steps.append(Step("Build Containers", "Containers built", self.build_containers))

    def build_containers(self) -> None:
        """Builds the containers for the Financial Tracker using the current source code"""

        print("Building the backend container image")
        self.run_subprocess(f"docker build ../backend -t backend-{self.configuration.name}")
        print("Building the frontend container image")
        self.run_subprocess(f"docker build ../frontend -t frontend-{self.configuration.name} --build-arg VITE_API_URL=http://localhost:{self.configuration.backend_port} --build-arg NGINX_PORT={self.configuration.frontend_port}")
if __name__ == "__main__":
    main()
