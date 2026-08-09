"""Destroy the native debug environment."""

import shutil
from argparse import Namespace

from ..core.context import Context


def run(context: Context, _args: Namespace) -> int:
    if context.paths.debug.exists():
        shutil.rmtree(context.paths.debug)
    return 0
