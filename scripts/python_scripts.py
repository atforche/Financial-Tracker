#!/usr/bin/env python3
"""Commands for maintaining the quality of repository Python scripts."""

import os
import subprocess
import sys
from pathlib import Path

from shared.command import Command
from shared.command_collection import CommandCollection
from shared.step import Step

PROJECT_ROOT = Path(__file__).resolve().parent.parent
VIRTUAL_ENVIRONMENT_PATH = PROJECT_ROOT / ".venv"
QUALITY_PYTHON = VIRTUAL_ENVIRONMENT_PATH / "bin" / "python"


def get_quality_python() -> Path:
    """Returns the Python interpreter in the repository-local virtual environment."""

    if not QUALITY_PYTHON.is_file():
        raise RuntimeError(
            "Python quality tools are not installed. Run 'python3 scripts/python_scripts.py install'."
        )
    return QUALITY_PYTHON


def ensure_quality_pip() -> None:
    """Bootstraps pip in the repository virtual environment when needed."""

    pip_check = subprocess.run(
        [str(get_quality_python()), "-m", "pip", "--version"], check=False
    )
    if pip_check.returncode == 0:
        return

    print("Bootstrapping pip in the Python virtual environment")
    ensure_pip = subprocess.run(
        [str(get_quality_python()), "-m", "ensurepip", "--upgrade"], check=False
    )
    if ensure_pip.returncode != 0:
        python_version = f"{sys.version_info.major}.{sys.version_info.minor}"
        raise RuntimeError(
            "The Python virtual environment does not include pip. Install the Python "
            f"venv package (for example, 'sudo apt install python{python_version}-venv'), "
            f"delete {VIRTUAL_ENVIRONMENT_PATH}, and run this command again."
        )


class InstallCommand(Command):
    """Installs the Python quality tool dependencies."""

    def __init__(self) -> None:
        super().__init__("install", "Installs Python quality tools")
        self.steps.append(
            Step(
                "Install Python Quality Tools",
                "Python quality tools installed",
                self.install_tools,
            )
        )

    def install_tools(self) -> None:
        """Creates the virtual environment and installs pinned development dependencies."""

        if not QUALITY_PYTHON.is_file():
            print(f"Creating Python virtual environment: {VIRTUAL_ENVIRONMENT_PATH}")
            subprocess.run(
                [sys.executable, "-m", "venv", str(VIRTUAL_ENVIRONMENT_PATH)],
                check=True,
            )

        ensure_quality_pip()
        subprocess.run(
            [
                str(QUALITY_PYTHON),
                "-m",
                "pip",
                "install",
                "--requirement",
                "requirements-dev.txt",
            ],
            check=True,
        )


class FormatCommand(Command):
    """Verifies that Python scripts have the configured formatting."""

    def __init__(self) -> None:
        super().__init__("format", "Verifies Python script formatting")
        self.steps.append(
            Step(
                "Verify Python Formatting",
                "Python formatting verified",
                self.verify_formatting,
            )
        )

    def verify_formatting(self) -> None:
        """Runs the Ruff formatter in verification mode."""

        self.run_subprocess(f"{get_quality_python()} -m ruff format --check scripts")


class FormatFixCommand(Command):
    """Applies the configured Python formatting."""

    def __init__(self) -> None:
        super().__init__("format-fix", "Applies Python script formatting")
        self.steps.append(
            Step(
                "Format Python Scripts", "Python scripts formatted", self.format_scripts
            )
        )

    def format_scripts(self) -> None:
        """Runs the Ruff formatter in write mode."""

        self.run_subprocess(f"{get_quality_python()} -m ruff format scripts")


class LintCommand(Command):
    """Runs static lint checks for Python scripts."""

    def __init__(self) -> None:
        super().__init__("lint", "Lints Python scripts")
        self.steps.append(
            Step("Lint Python Scripts", "Python scripts linted", self.lint_scripts)
        )

    def lint_scripts(self) -> None:
        """Runs Ruff linting."""

        self.run_subprocess(f"{get_quality_python()} -m ruff check scripts")


class TypeCheckCommand(Command):
    """Runs static type checks for Python scripts."""

    def __init__(self) -> None:
        super().__init__("typecheck", "Type checks Python scripts")
        self.steps.append(
            Step(
                "Type Check Python Scripts",
                "Python scripts type checked",
                self.type_check,
            )
        )

    def type_check(self) -> None:
        """Runs Mypy type checking."""

        self.run_subprocess(f"{get_quality_python()} -m mypy scripts")


class TestCommand(Command):
    """Runs Python script unit tests with coverage."""

    def __init__(self) -> None:
        super().__init__("test", "Tests Python scripts with coverage")
        self.steps.append(
            Step("Test Python Scripts", "Python scripts tested", self.test_scripts)
        )

    def test_scripts(self) -> None:
        """Runs pytest and reports coverage for production scripts."""

        self.run_subprocess(
            f"{get_quality_python()} -m pytest --cov=scripts --cov-report=term-missing"
        )


def main() -> None:
    """Runs the requested Python quality command from the repository root."""

    os.chdir(PROJECT_ROOT)
    commands = CommandCollection("Quality commands for the repository Python scripts")
    commands.commands = [
        InstallCommand(),
        FormatCommand(),
        FormatFixCommand(),
        LintCommand(),
        TypeCheckCommand(),
        TestCommand(),
    ]
    commands.run()


if __name__ == "__main__":
    main()
