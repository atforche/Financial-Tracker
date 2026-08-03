#!/usr/bin/env python3
"""Helper scripts for developing the Financial Tracker frontend"""

import os
from pathlib import Path

from shared.command import Command
from shared.command_collection import CommandCollection
from shared.step import Step

FRONTEND_DIRECTORY = Path(__file__).resolve().parent.parent / "frontend"
OPENAPI_TYPESCRIPT_COMMAND = (
    "npx openapi-typescript ../backend/.artifacts/obj/Rest/Financial-Tracker-API.json "
    "--output framework/data/api.ts{check} --enum"
)


class FrontendCommand(Command):
    """Base command that executes a subprocess from the frontend directory."""

    def run_frontend_subprocess(self, command: str) -> None:
        """Runs a frontend command without leaving the process in another directory."""

        original_directory = Path.cwd()
        try:
            os.chdir(FRONTEND_DIRECTORY)
            self.run_subprocess(command)
        finally:
            os.chdir(original_directory)

    def update_frontend_models(self, verify: bool) -> None:
        """Generates frontend API models, optionally failing when they are stale."""

        self.run_frontend_subprocess(
            OPENAPI_TYPESCRIPT_COMMAND.format(check=" --check" if verify else "")
        )


def main():
    """Builds and runs the command collection for this script"""

    commands = CommandCollection(
        "Helper scripts for developing the Financial Tracker frontend"
    )
    commands.commands.append(InstallFrontendPackages())
    commands.commands.append(FormatFrontend())
    commands.commands.append(FixFrontendFormatting())
    commands.commands.append(LintFrontend())
    commands.commands.append(BuildFrontend())
    commands.commands.append(RunFrontend())
    commands.commands.append(RefreshFrontendModels())
    commands.commands.append(VerifyFrontendModels())
    commands.run()


class InstallFrontendPackages(FrontendCommand):
    """Command class that installs the npm dependencies for the frontend"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("install", "Installs the npm dependencies for the frontend")
        self.steps.append(
            Step(
                "Install Frontend Dependencies",
                "Dependencies installed",
                self.install_dependencies,
            )
        )

    def install_dependencies(self):
        """Installs the npm dependencies for the frontend"""

        self.run_frontend_subprocess("npm ci")


class FormatFrontend(FrontendCommand):
    """Command class that runs formatting for the frontend"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("format", "Runs formatting for the frontend")
        self.steps.append(
            Step("Format Frontend", "Formatting completed", self.run_formatting)
        )

    def run_formatting(self):
        """Runs formatting for the frontend"""

        self.run_frontend_subprocess("npx prettier . --check")


class FixFrontendFormatting(FrontendCommand):
    """Command class that fixes formatting for the frontend"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("fix-formatting", "Fixes formatting for the frontend")
        self.steps.append(
            Step("Fix Frontend Formatting", "Formatting fixed", self.fix_formatting)
        )

    def fix_formatting(self):
        """Fixes formatting for the frontend"""

        self.run_frontend_subprocess("npx prettier . --write")


class LintFrontend(FrontendCommand):
    """Command class that runs linting for the frontend"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("lint", "Runs linting for the frontend")
        self.steps.append(Step("Lint Frontend", "Linting completed", self.run_linting))

    def run_linting(self):
        """Runs linting for the frontend"""

        self.run_frontend_subprocess(
            "npx eslint . --ext ts,tsx --report-unused-disable-directives --max-warnings 0"
        )


class BuildFrontend(FrontendCommand):
    """Command class that builds the frontend"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("build", "Builds the frontend")
        self.steps.append(
            Step("Build Frontend", "Frontend build completed", self.build_frontend)
        )

    def build_frontend(self):
        """Builds the frontend"""

        self.run_frontend_subprocess("npx tsc")
        self.run_frontend_subprocess("npx next build")


class RunFrontend(FrontendCommand):
    """Command class that runs the frontend"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("run", "Runs the frontend")
        self.steps.append(Step("Run Frontend", "Frontend exited", self.run_frontend))

    def run_frontend(self):
        """Runs the frontend"""

        self.run_frontend_subprocess("npx next dev")


class RefreshFrontendModels(FrontendCommand):
    """Command class that refreshes the API models used by the frontend"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("refresh-models", "Refreshes the models used by the frontend")
        self.steps.append(
            Step("Refresh Frontend Models", "Models refreshed", self.refresh_models)
        )

    def refresh_models(self):
        """Refreshes the models used by the frontend"""

        self.update_frontend_models(verify=False)


class VerifyFrontendModels(FrontendCommand):
    """Command class that verifies the API models used by the frontend are up to date"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__(
            "verify-models", "Verifies the models used by the frontend are up to date"
        )
        self.steps.append(
            Step("Verify Frontend Models", "Models verified", self.verify_models)
        )

    def verify_models(self):
        """Verifies the models used by the frontend are up to date"""

        self.update_frontend_models(verify=True)


if __name__ == "__main__":
    main()
