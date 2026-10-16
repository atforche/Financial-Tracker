"""Type-check frontend source."""

from argparse import Namespace

from ..core.context import Context
from .frontend_support import npm


def run(context: Context, _args: Namespace) -> int:
    # Typed routes and next-env.d.ts are generated, so a clean checkout needs them
    # before tsc can resolve route and image types.
    npm(context, ["exec", "--", "next", "typegen"])
    return npm(context, ["exec", "--", "tsc"])
