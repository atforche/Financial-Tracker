"""Registration for security commands."""

from . import security_dependencies, security_images
from .common import add_command


def register(commands: object) -> None:
    add_command(
        commands,
        "scan-dependencies",
        "Scan frontend and backend dependencies",
        security_dependencies.run,
    )
    add_command(
        commands, "scan-images", "Scan deployable container images", security_images.run
    )
