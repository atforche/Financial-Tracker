"""Create the native debug environment."""

from argparse import Namespace

from ..core.context import Context
from .debug_support import apply_migrations, prepare


def run(context: Context, _args: Namespace) -> int:
    prepare(context)
    database = context.paths.debug_database
    database.touch(mode=0o666, exist_ok=True)
    database.chmod(0o666)
    return apply_migrations(context)
