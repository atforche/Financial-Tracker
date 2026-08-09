"""Registration for backend commands."""

from argparse import ArgumentParser

from . import (
    backend_build,
    backend_coverage,
    backend_create_migration,
    backend_format,
    backend_restore,
    backend_test,
)
from .common import add_command


def register(commands: object) -> None:
    add_command(
        commands, "restore", "Restore the backend solution", backend_restore.run
    )
    add_command(commands, "format", "Verify backend formatting", backend_format.run)
    add_command(commands, "build", "Build the backend solution", backend_build.run)

    add_command(commands, "test", "Run backend tests", backend_test.run)
    add_command(
        commands, "coverage", "Run backend tests with coverage", backend_coverage.run
    )

    def configure_migration(parser: ArgumentParser) -> None:
        parser.add_argument("name")

    add_command(
        commands,
        "create-migration",
        "Create a database migration",
        backend_create_migration.run,
        configure_migration,
    )
