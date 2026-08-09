"""Roll back to the previous healthy release."""

from argparse import Namespace

from ..core.context import Context
from ..operations.deployment import rollback_release


def run(context: Context, args: Namespace) -> int:
    rollback_release(args.path, context.runner)
    return 0
