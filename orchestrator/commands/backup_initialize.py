"""Initialize the encrypted backup repository."""

from argparse import Namespace

from ..core.context import Context
from ..operations.backup import BackupOperations


def run(context: Context, args: Namespace) -> int:
    BackupOperations(args.path, context.runner).initialize()
    return 0
