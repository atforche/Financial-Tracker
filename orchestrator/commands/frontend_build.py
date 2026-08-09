"""Build frontend source."""

from argparse import Namespace

from ..core.context import Context
from .frontend_support import npm


def run(context: Context, _args: Namespace) -> int:
    npm(context, ["exec", "--", "tsc"])
    return npm(context, ["exec", "--", "next", "build"])
