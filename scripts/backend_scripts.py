#!/usr/bin/env python3
"""Helper scripts for developing the Financial Tracker backend"""

import os
from typing import Annotated
from shared.command import Command
from shared.command_collection import CommandCollection
from shared.migration_script import MigrationScript
from shared.step import Step

def main():
    """Builds and runs the command collection for this script"""

    commands = CommandCollection("Helper scripts for developing the Financial Tracker backend")
    commands.commands.append(RestoreSolution())
    commands.commands.append(FormatSolution())
    commands.commands.append(BuildSolution())
    commands.commands.append(TestSolution())
    commands.commands.append(RunPipeline())
    commands.commands.append(RunApi())
    commands.commands.append(CreateMigration())
    commands.run()

class RestoreSolution(Command):
    """Command class that runs a Nuget restore on the solution"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("restore", "Runs a Nuget restore on the solution")
        self.steps.append(Step("Restore Solution", "Restore completed", lambda: self.run_subprocess("dotnet restore ../backend/Backend.sln")))

class FormatSolution(Command):
    """Command class that runs formatting on the solution"""

    def __init__(self) -> None:
        """Constructs a new instance of this class"""

        super().__init__("format", "Runs formatting on the solution")
        self.steps.append(Step("Format Solution", "Format completed",
                               lambda: self.run_subprocess("dotnet format ../backend/Backend.sln --verify-no-changes --no-restore --severity info")))

class BuildSolution(Command):
    """Command class that builds the solution"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("build", "Builds the backend solution")
        self.steps.append(Step("Build Solution", "Build completed", lambda: self.run_subprocess("dotnet build ../backend/Backend.sln --no-restore")))

class TestSolution(Command):
    """Command class that tests the solution"""

    use_database: Annotated[bool, "If provided, runs the unit test against an actual database"]

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("test", "Runs the unit tests on the backend solution")
        self.steps.append(Step("Test solution", "Tests completed", self.run_tests))

    def run_tests(self):
        """Runs the tests"""

        if self.use_database:
            print("Running database integration tests")
            self.run_subprocess("dotnet test ../backend/Backend.sln --no-build --verbosity normal --environment USE_DATABASE=TRUE")
        else:
            print("Running unit tests")
            self.run_subprocess("dotnet test ../backend/Backend.sln --no-build --verbosity normal")

class RunPipeline(Command):
    """Command class that runs the entire build pipeline for the backend solution"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("run-pipeline", "Runs the full build pipeline for the backend solution")
        self.steps.append(Step("", "", lambda: RestoreSolution().run([])))
        self.steps.append(Step("", "", lambda: FormatSolution().run([])))
        self.steps.append(Step("", "", lambda: BuildSolution().run([])))
        self.steps.append(Step("", "", lambda: TestSolution().run([])))
        self.steps.append(Step("", "", lambda: TestSolution().run(["--use-database"])))

class RunApi(Command):
    """Command class that runs the backend API"""

    port: Annotated[int, "Port that the backend REST API should listen on"]
    database_path: Annotated[str, "Path to the database file"]
    frontend_origin: Annotated[str, "Origin that the frontend will make requests from"]

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("run", "Runs the backend REST API")
        self.steps.append(Step("Run Backend API", "Backend exited", self.run_api))

    def run_api(self):
        """Runs the backend API"""

        print(f"Running the backend on port {self.port} with database at {self.database_path}")
        self.run_subprocess(f"""dotnet run --project ../backend/Rest.Controllers/Rest.Controllers.csproj
                                --environment ASPNETCORE_ENVIRONMENT=Development 
                                --environment ASPNETCORE_HTTP_PORTS={self.port}
                                --environment DATABASE_PATH={self.database_path}
                                --environment FRONTEND_ORIGIN={self.frontend_origin}""")

class CreateMigration(Command):
    """Command class that creates a new database migration"""

    name: Annotated[str, "Name for the new migration"]

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("create-migration", "Creates a new database migration")
        self.steps.append(Step("Create Migration", "New migration created", self.create_new_migration))

    def create_new_migration(self):
        """Creates a new database migration"""

        print(f"Creating new migration {self.name}")
        self.run_subprocess(f"dotnet ef migrations add {self.name} \
                            --project ../backend/Data/Data.csproj --msbuildprojectextensionspath ../backend/.artifacts/obj/Data")

        if not os.path.isdir(MigrationScript.directory):
            print(f"Creating migration scripts directory at {MigrationScript.directory}")
            os.makedirs(MigrationScript.directory)

        migrations = MigrationScript.get_all()
        if len(migrations) == 0:
            new_migration = MigrationScript(1, self.name)
            last_migration = None
        else:
            last_migration = max(migrations, key=lambda migration: migration.id)
            new_migration = MigrationScript(last_migration.id + 1, self.name)

        print(f"Creating new migration script {new_migration.file_name}")
        self.run_subprocess(f'dotnet ef migrations script \
                                {last_migration.name if last_migration is not None else ""} \
                                --project ../backend/Data/Data.csproj \
                                --output "{MigrationScript.directory}/{new_migration.file_name}"')

if __name__ == "__main__":
    main()
