"""Registration for dependency commands."""

from . import deps_check, deps_install
from .common import add_command


def register(commands: object) -> None:
    add_command(
        commands, "install", "Install all repository dependencies", deps_install.run
    )
    add_command(
        commands, "check", "Verify the configured toolchain versions", deps_check.run
    )
