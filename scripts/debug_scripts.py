#!/usr/bin/env python3
"""Helper scripts for debugging the Financial Tracker"""

import os
import shutil
from deploy_scripts import CreateInstanceDirectory, CreateEmptyDatabase, ApplyMigrations
from shared.command import Command
from shared.command_collection import CommandCollection
from shared.configuration import Configuration, Environment
from shared.step import Step

def main():
    """Builds and runs the command collection for this script"""

    commands = CommandCollection("Helper scripts for debugging the Financial Tracker")
    commands.commands.append(CreateDebugEnvironment())
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
        backend_port=8081,
        frontend_port=3001,
        database_revision=0
    )

class CreateDebugEnvironment(Command):
    """Command class that creates the debug environment"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("create", "Creates the debug environment")
        self.steps.append(Step("", "", lambda: CreateInstanceDirectory(get_debug_configuration()).run([])))
        self.steps.append(Step("", "", lambda: CreateEmptyDatabase(get_debug_configuration()).run([])))
        self.steps.append(Step("", "", lambda: ApplyMigrations(get_debug_configuration()).run([])))

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
        environment = os.environ.copy()
        environment["VITE_API_URL"] = f"http://localhost:{get_debug_configuration().backend_port}"
        self.run_subprocess(f"npx vite --port {get_debug_configuration().frontend_port}", env=environment)

class RunDebugBackend(Command):
    """Command class that runs the backend for the debug environment"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("backend", "Runs the backend for the debug environment")
        self.steps.append(Step("Run Debug Backend", "Debug backend running", self.run_backend))

    def run_backend(self):
        """Runs the backend for the debug environment"""

        configuration = get_debug_configuration()
        os.chdir("../backend/Rest")
        self.run_subprocess((f"dotnet run --environment ASPNETCORE_ENVIRONMENT={configuration.environment.value}"
                             f" --environment ASPNETCORE_HTTP_PORTS={configuration.backend_port}"
                             f" --environment DATABASE_PATH={configuration.get_database_file_path()}"
                             f" --environment LOG_DIRECTORY={configuration.path}/logs"
                             f" --environment FRONTEND_ORIGIN=http://localhost:{configuration.frontend_port}"))

if __name__ == "__main__":
    main()
