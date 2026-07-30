#!/usr/bin/env python3
"""Helper scripts for debugging the Financial Tracker"""

import os
import re
import shutil
from deploy_scripts import CreateInstanceDirectory, CreateEmptyDatabase, ApplyMigrations
from shared.command import Command
from shared.command_collection import CommandCollection
from shared.configuration import Configuration, Environment
from shared.step import Step

DEBUG_BACKEND_PORT = 8081
DEBUG_FRONTEND_PORT = 3001

def main():
    """Builds and runs the command collection for this script"""

    commands = CommandCollection("Helper scripts for debugging the Financial Tracker")
    commands.commands.append(CreateDebugEnvironment())
    commands.commands.append(UpgradeDebugEnvironment())
    commands.commands.append(DestroyDebugEnvironment())
    commands.commands.append(RunDebugFrontend())
    commands.commands.append(RunDebugBackend())
    commands.run()

def get_debug_configuration() -> Configuration:
    """Gets the configuration for the debug environment"""

    return Configuration(
        name="Debug",
        path=os.path.join(os.path.dirname(__file__), "..", "debug"),
        environment=Environment.DEVELOPMENT,
        database_revision=0,
        public_origin="https://localhost:3001",
        google_client_id=os.environ.get("GOOGLE_CLIENT_ID", ""),
        google_client_secret=os.environ.get("GOOGLE_CLIENT_SECRET", ""),
        google_allowed_subjects=os.environ.get("GOOGLE_ALLOWED_SUBJECTS", ""),
        auth_secret=os.environ.get("AUTH_SECRET", "")
    )

def get_debug_environment() -> dict[str, str]:
    """Returns the current environment augmented with the debug instance settings."""

    environment = os.environ.copy()
    with open(get_debug_configuration().get_environment_file_path(), "r", encoding="utf-8") as file:
        for line in file:
            variable_match = re.fullmatch(r'([A-Z][A-Z0-9_]*)="(.*)"\n?', line)
            if variable_match is not None:
                environment[variable_match.group(1)] = variable_match.group(2)
    return environment

class CreateDebugEnvironment(Command):
    """Command class that creates the debug environment"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("create", "Creates the debug environment")
        if os.path.exists(get_debug_configuration().path):
            return
        self.steps.append(Step("", "", lambda: CreateInstanceDirectory(get_debug_configuration()).run([])))
        self.steps.append(Step("", "", lambda: CreateEmptyDatabase(get_debug_configuration()).run([])))
        self.steps.append(Step("", "", lambda: ApplyMigrations(get_debug_configuration()).run([])))

class UpgradeDebugEnvironment(Command):
    """Command class that upgrades the debug environment"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("upgrade", "Upgrades the debug environment")
        config_path = get_debug_configuration().path
        self.steps.append(Step("", "", lambda: ApplyMigrations(Configuration.build_from_existing_instance(config_path, False)).run([])))

class DestroyDebugEnvironment(Command):
    """Command class that destroys the debug environment"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("destroy", "Destroys the debug environment")
        self.steps.append(Step("Destroy Debug Environment", "Debug environment destroyed", lambda: shutil.rmtree(get_debug_configuration().path)))

class RunDebugFrontend(Command):
    """Command class that runs the frontend for the debug environment"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("frontend", "Runs the frontend for the debug environment")
        self.steps.append(Step("Run Debug Frontend", "Debug frontend running", self.run_frontend))

    def run_frontend(self):
        """Runs the frontend for the debug environment"""

        os.chdir("../frontend")
        environment = get_debug_environment()
        environment["API_URL"] = f"http://localhost:{DEBUG_BACKEND_PORT}"
        self.run_subprocess(f"npx next dev --port {DEBUG_FRONTEND_PORT}", env=environment)

class RunDebugBackend(Command):
    """Command class that runs the backend for the debug environment"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("backend", "Runs the backend for the debug environment")
        self.steps.append(Step("Run Debug Backend", "Debug backend running", self.run_backend))

    def run_backend(self):
        """Runs the backend for the debug environment"""

        configuration = get_debug_configuration()
        environment = get_debug_environment()
        environment["ASPNETCORE_ENVIRONMENT"] = configuration.environment.value
        environment["ASPNETCORE_HTTP_PORTS"] = str(DEBUG_BACKEND_PORT)
        environment["DATABASE_PATH"] = configuration.get_database_file_path()
        environment["LOG_DIRECTORY"] = f"{configuration.path}/logs"
        environment["FRONTEND_ORIGIN"] = f"http://localhost:{DEBUG_FRONTEND_PORT}"
        os.chdir("../backend/Rest")
        self.run_subprocess("dotnet run", env=environment)

if __name__ == "__main__":
    main()
