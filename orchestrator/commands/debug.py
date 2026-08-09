"""Registration for local debug commands."""

from argparse import ArgumentParser

from . import (
    debug_backend,
    debug_create,
    debug_destroy,
    debug_frontend,
    debug_stack_destroy,
    debug_stack_down,
    debug_stack_up,
    debug_upgrade,
)
from .common import add_command
from .debug_support import default_debug_frontend_port


def register(commands: object) -> None:
    add_command(
        commands, "create", "Create the local debug environment", debug_create.run
    )
    add_command(
        commands, "upgrade", "Apply debug database migrations", debug_upgrade.run
    )
    add_command(
        commands, "destroy", "Remove the native debug environment", debug_destroy.run
    )
    add_command(
        commands,
        "stack-up",
        "Build and start the local container stack",
        debug_stack_up.run,
    )
    add_command(
        commands, "stack-down", "Stop the local container stack", debug_stack_down.run
    )
    add_command(
        commands,
        "stack-destroy",
        "Stop the stack and remove its volumes",
        debug_stack_destroy.run,
    )

    def configure_frontend(parser: ArgumentParser) -> None:
        parser.add_argument("--port", type=int, default=default_debug_frontend_port())
        parser.add_argument(
            "--inspect", action="store_true", help="Enable the Node.js inspector"
        )

    add_command(
        commands,
        "frontend",
        "Run the native debug frontend",
        debug_frontend.run,
        configure_frontend,
    )
    add_command(commands, "backend", "Run the native debug backend", debug_backend.run)
