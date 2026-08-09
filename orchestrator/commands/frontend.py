"""Registration for frontend commands."""

from . import (
    frontend_build,
    frontend_format,
    frontend_install,
    frontend_lint,
    frontend_models,
    frontend_playwright_install,
)
from .common import add_command


def register(commands: object) -> None:
    add_command(
        commands, "install", "Install frontend dependencies", frontend_install.run
    )
    add_command(commands, "format", "Verify frontend formatting", frontend_format.check)
    add_command(
        commands, "format-fix", "Apply frontend formatting", frontend_format.fix
    )
    add_command(commands, "lint", "Run frontend lint checks", frontend_lint.run)
    add_command(
        commands,
        "install-browser",
        "Install the Chromium browser for frontend end-to-end tests",
        frontend_playwright_install.run,
    )
    add_command(commands, "build", "Build the frontend", frontend_build.run)

    def configure_models(parser):
        parser.add_argument(
            "--verify", action="store_true", help="Fail if generated models are stale"
        )

    add_command(
        commands,
        "models",
        "Generate or verify frontend API models",
        frontend_models.run,
        configure_models,
    )
