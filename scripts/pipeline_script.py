#!/usr/bin/env python3
"""Runs the locally reproducible portions of the GitHub Actions pipelines."""

import subprocess
import sys
from collections.abc import Sequence
from pathlib import Path

from shared.command import Command
from shared.command_collection import CommandCollection
from shared.step import Step

SCRIPTS_DIRECTORY = Path(__file__).resolve().parent

PYTHON_COMMANDS = (
    ("python_scripts.py", "install"),
    ("python_scripts.py", "format"),
    ("python_scripts.py", "lint"),
    ("python_scripts.py", "typecheck"),
    ("python_scripts.py", "test"),
)

BACKEND_FORMAT_COMMANDS = (
    ("backend_scripts.py", "restore"),
    ("backend_scripts.py", "format"),
)

FRONTEND_FORMAT_COMMANDS = (
    ("frontend_scripts.py", "install"),
    ("frontend_scripts.py", "format"),
    ("frontend_scripts.py", "lint"),
)

BACKEND_TEST_COMMANDS = (
    ("backend_scripts.py", "restore"),
    ("backend_scripts.py", "build"),
    ("backend_scripts.py", "test"),
)

FRONTEND_BUILD_COMMANDS = (
    ("frontend_scripts.py", "install"),
    ("frontend_scripts.py", "build"),
)

API_CONTRACT_COMMANDS = (
    ("frontend_scripts.py", "install"),
    ("frontend_scripts.py", "verify-models"),
)

DEPENDENCY_COMMANDS = (
    ("backend_scripts.py", "restore"),
    ("security_scripts.py", "scan-dependencies"),
)

CONTAINER_IMAGE_COMMANDS = (
    ("container_scripts.py", "build"),
    ("security_scripts.py", "scan-images"),
    ("container_scripts.py", "smoke-test"),
)

RUN_COMMANDS = (
    *PYTHON_COMMANDS,
    *BACKEND_FORMAT_COMMANDS,
    *FRONTEND_FORMAT_COMMANDS,
    *BACKEND_TEST_COMMANDS,
    *FRONTEND_BUILD_COMMANDS,
    *API_CONTRACT_COMMANDS,
    *DEPENDENCY_COMMANDS,
    *CONTAINER_IMAGE_COMMANDS,
)


def main() -> None:
    """Builds and runs the local pipeline command collection."""

    commands = CommandCollection(
        "Runs the same repository checks used by GitHub Actions"
    )
    commands.commands = [
        PipelineCommand(
            "python",
            "Runs Python formatting, linting, types, and tests",
            PYTHON_COMMANDS,
        ),
        PipelineCommand(
            "backend-format",
            "Restores and formats the backend",
            BACKEND_FORMAT_COMMANDS,
        ),
        PipelineCommand(
            "frontend-format",
            "Installs, formats, and lints the frontend",
            FRONTEND_FORMAT_COMMANDS,
        ),
        PipelineCommand(
            "backend-test",
            "Restores, builds, and tests the backend",
            BACKEND_TEST_COMMANDS,
        ),
        PipelineCommand(
            "frontend-build",
            "Installs and builds the frontend",
            FRONTEND_BUILD_COMMANDS,
        ),
        PipelineCommand(
            "api-contract",
            "Verifies generated frontend API models",
            API_CONTRACT_COMMANDS,
        ),
        PipelineCommand(
            "dependencies",
            "Restores and scans application dependencies",
            DEPENDENCY_COMMANDS,
        ),
        PipelineCommand(
            "container-images",
            "Builds, scans, and smoke-tests container images",
            CONTAINER_IMAGE_COMMANDS,
        ),
        PipelineCommand("run", "Runs every local pipeline command", RUN_COMMANDS),
    ]
    commands.run()


class PipelineCommand(Command):
    """Runs an ordered collection of repository script commands."""

    def __init__(
        self, name: str, description: str, commands: Sequence[tuple[str, str]]
    ) -> None:
        """Constructs a local pipeline command."""

        super().__init__(name, description)
        for script, command in commands:

            def run_script_step(script: str = script, command: str = command) -> None:
                self.run_script(script, command)

            self.steps.append(Step("", "", run_script_step))

    @staticmethod
    def run_script(script: str, command: str) -> None:
        """Runs a repository script from the scripts directory in an isolated process."""

        subprocess.run(
            [sys.executable, script, command], cwd=SCRIPTS_DIRECTORY, check=True
        )


if __name__ == "__main__":
    main()
