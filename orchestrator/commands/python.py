"""Registration for Python tooling commands."""

from . import python_format, python_install, python_lint, python_test, python_typecheck
from .common import add_command


def register(commands: object) -> None:
    add_command(
        commands,
        "install",
        "Install pinned Python quality dependencies",
        python_install.run,
    )
    add_command(commands, "format", "Verify Python formatting", python_format.check)
    add_command(commands, "format-fix", "Apply Python formatting", python_format.fix)
    add_command(commands, "lint", "Run Python lint checks", python_lint.run)
    add_command(commands, "typecheck", "Run Python type checks", python_typecheck.run)
    add_command(commands, "test", "Run Python tests with coverage", python_test.run)
