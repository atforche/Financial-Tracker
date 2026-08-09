"""Install the browser used by frontend end-to-end tests."""

from argparse import Namespace

from ..core.context import Context
from .frontend_support import npm


def run(context: Context, _args: Namespace) -> int:
    return npm(
        context,
        ["exec", "--", "playwright", "install", "--with-deps", "chromium"],
    )
