#!/usr/bin/env python3
"""Helper scripts for developing the Financial Tracker frontend"""

import os
from shared.command import Command
from shared.command_collection import CommandCollection
from shared.step import Step

def main():
    """Builds and runs the command collection for this script"""

    commands = CommandCollection("Helper scripts for developing the Financial Tracker frontend")
    commands.commands.append(InstallFrontendPackages())
    commands.commands.append(FormatFrontend())
    commands.commands.append(FixFrontendFormatting())
    commands.commands.append(LintFrontend())
    commands.commands.append(BuildFrontend())
    commands.commands.append(RunFrontend())
    commands.commands.append(RefreshFrontendModels())
    commands.commands.append(VerifyFrontendModels())
    commands.run()

class InstallFrontendPackages(Command):
    """Command class that installs the npm dependencies for the frontend"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("install", "Installs the npm dependencies for the frontend")
        self.steps.append(Step("Install Frontend Dependencies", "Dependencies installed", self.install_dependencies))

    def install_dependencies(self):
        """Installs the npm dependencies for the frontend"""

        os.chdir("../frontend")
        self.run_subprocess("npm ci")

class FormatFrontend(Command):
    """Command class that runs formatting for the frontend"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("format", "Runs formatting for the frontend")
        self.steps.append(Step("Format Frontend", "Formatting completed", self.run_formatting))

    def run_formatting(self):
        """Runs formatting for the frontend"""

        os.chdir("../frontend")
        self.run_subprocess("npx prettier . --check")

class FixFrontendFormatting(Command):
    """Command class that fixes formatting for the frontend"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("fix-formatting", "Fixes formatting for the frontend")
        self.steps.append(Step("Fix Frontend Formatting", "Formatting fixed", self.fix_formatting))

    def fix_formatting(self):
        """Fixes formatting for the frontend"""

        os.chdir("../frontend")
        self.run_subprocess("npx prettier . --write")

class LintFrontend(Command):
    """Command class that runs linting for the frontend"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("lint", "Runs linting for the frontend")
        self.steps.append(Step("Lint Frontend", "Linting completed", self.run_linting))

    def run_linting(self):
        """Runs linting for the frontend"""

        os.chdir("../frontend")
        self.run_subprocess("npx eslint . --ext ts,tsx --report-unused-disable-directives --max-warnings 0")

class BuildFrontend(Command):
    """Command class that builds the frontend"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("build", "Builds the frontend")
        self.steps.append(Step("Build Frontend", "Frontend build completed", self.build_frontend))

    def build_frontend(self):
        """Builds the frontend"""

        os.chdir("../frontend")
        self.run_subprocess("npx tsc")
        self.run_subprocess("npx vite build")

class RunFrontend(Command):
    """Command class that runs the frontend"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("run", "Runs the frontend")
        self.steps.append(Step("Run Frontend", "Frontend exited", self.run_frontend))

    def run_frontend(self):
        """Runs the frontend"""

        os.chdir("../frontend")
        self.run_subprocess("npx vite")

class RefreshFrontendModels(Command):
    """Command class that refreshes the API models used by the frontend"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("refresh-models", "Refreshes the models used by the frontend")
        self.steps.append(Step("Refresh Frontend Models", "Models refreshed", self.refresh_models))

    def refresh_models(self):
        """Refreshes the models used by the frontend"""

        os.chdir("../frontend")
        self.run_subprocess("npx openapi-typescript ../backend/.artifacts/obj/Rest/Financial-Tracker-API.json --output src/data/api.ts --enum")

class VerifyFrontendModels(Command):
    """Command class that verifies the API models used by the frontend are up to date"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("verify-models", "Verifies the models used by the frontend are up to date")
        self.steps.append(Step("Verify Frontend Models", "Models verified", self.verify_models))

    def verify_models(self):
        """Verifies the models used by the frontend are up to date"""

        os.chdir("../frontend")
        self.run_subprocess("npx openapi-typescript ../backend/.artifacts/obj/Rest/Financial-Tracker-API.json --output src/data/api.ts --check --enum")

if __name__ == "__main__":
    main()
