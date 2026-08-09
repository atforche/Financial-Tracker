"""Create an encrypted database backup."""

from argparse import Namespace

from ..core.context import Context
from ..operations.backup import BackupOperations


def run(context: Context, args: Namespace) -> int:
    BackupOperations(args.path, context.runner).backup()
    return 0
