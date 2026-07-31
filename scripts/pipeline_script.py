#!/usr/bin/env python3
"""Runs the locally reproducible portions of the GitHub Actions pipelines."""

from collections.abc import Sequence
from pathlib import Path
import subprocess
import sys

from shared.command import Command
from shared.command_collection import CommandCollection
from shared.step import Step

SCRIPTS_DIRECTORY = Path(__file__).resolve().parent

QUALITY_COMMANDS = (
    ("backend_scripts.py", "restore"),
    ("backend_scripts.py", "format"),
    ("frontend_scripts.py", "install"),
    ("frontend_scripts.py", "format"),
    ("frontend_scripts.py", "lint"),
)

VERIFICATION_COMMANDS = (
    ("backend_scripts.py", "restore"),
    ("backend_scripts.py", "build"),
    ("backend_scripts.py", "test"),
    ("frontend_scripts.py", "install"),
    ("frontend_scripts.py", "build"),
    ("frontend_scripts.py", "verify-models"),
    ("security_scripts.py", "scan-dependencies"),
    ("container_scripts.py", "build"),
    ("security_scripts.py", "scan-images"),
    ("container_scripts.py", "smoke-test"),
)

RELEASE_CHECK_COMMANDS = (
    ("container_scripts.py", "build"),
    ("security_scripts.py", "scan-images"),
    ("container_scripts.py", "smoke-test"),
)

PULL_REQUEST_COMMANDS = (
    ("backend_scripts.py", "restore"),
    ("frontend_scripts.py", "install"),
    ("backend_scripts.py", "format"),
    ("frontend_scripts.py", "format"),
    ("frontend_scripts.py", "lint"),
    ("backend_scripts.py", "build"),
    ("backend_scripts.py", "test"),
    ("frontend_scripts.py", "build"),
    ("frontend_scripts.py", "verify-models"),
    ("security_scripts.py", "scan-dependencies"),
    ("container_scripts.py", "build"),
    ("security_scripts.py", "scan-images"),
    ("container_scripts.py", "smoke-test"),
)


def main() -> None:
    """Builds and runs the local pipeline command collection."""

    commands = CommandCollection("Runs the same repository checks used by GitHub Actions")
    commands.commands.append(PipelineCommand(
        "quality",
        "Runs the code-quality workflow locally",
        QUALITY_COMMANDS))
    commands.commands.append(PipelineCommand(
        "verify",
        "Runs the verification workflow locally",
        VERIFICATION_COMMANDS))
    commands.commands.append(PipelineCommand(
        "release-check",
        "Builds, scans, and smoke-tests release images without publishing them",
        RELEASE_CHECK_COMMANDS))
    commands.commands.append(PipelineCommand(
        "pr",
        "Runs all pull-request checks locally without repeating setup work",
        PULL_REQUEST_COMMANDS))
    commands.run()


class PipelineCommand(Command):
    """Runs an ordered collection of repository script commands."""

    def __init__(self, name: str, description: str, commands: Sequence[tuple[str, str]]) -> None:
        """Constructs a local pipeline command."""

        super().__init__(name, description)
        for script, command in commands:
            self.steps.append(Step(
                "",
                "",
                lambda script=script, command=command: self.run_script(script, command)))

    @staticmethod
    def run_script(script: str, command: str) -> None:
        """Runs a repository script from the scripts directory in an isolated process."""

        subprocess.run(
            [sys.executable, script, command],
            cwd=SCRIPTS_DIRECTORY,
            check=True)


if __name__ == "__main__":
    main()
