"""Build frontend source."""

from argparse import Namespace

from ..core.context import Context
from . import frontend_typecheck
from .frontend_support import npm


def run(context: Context, args: Namespace) -> int:
    frontend_typecheck.run(context, args)
    return npm(context, ["exec", "--", "next", "build"])
