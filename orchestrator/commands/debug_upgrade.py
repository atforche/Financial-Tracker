"""Upgrade the native debug database."""

from argparse import Namespace

from ..core.context import Context
from .debug_support import apply_migrations


def run(context: Context, _args: Namespace) -> int:
    return apply_migrations(context)
