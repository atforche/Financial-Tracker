"""Registration for backup commands."""

from argparse import ArgumentParser

from . import backup_create, backup_initialize, backup_verify
from .common import add_command


def register(commands: object) -> None:
    def configure(parser: ArgumentParser) -> None:
        parser.add_argument(
            "--path", required=True, help="Path to the Financial Tracker instance"
        )

    add_command(
        commands,
        "initialize",
        "Initialize the encrypted backup repository",
        backup_initialize.run,
        configure,
    )
    add_command(
        commands,
        "backup",
        "Create an encrypted database backup",
        backup_create.run,
        configure,
    )
    add_command(
        commands,
        "verify",
        "Restore and verify the latest backup",
        backup_verify.run,
        configure,
    )
