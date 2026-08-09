"""Registration for container commands."""

from . import container_build, container_smoke_test
from .common import add_command


def register(commands: object) -> None:
    add_command(
        commands, "build", "Build deployable application images", container_build.run
    )
    add_command(
        commands,
        "smoke-test",
        "Run the container integration smoke test",
        container_smoke_test.run,
    )
