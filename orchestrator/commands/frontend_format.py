"""Format frontend source."""

from argparse import Namespace

from ..core.context import Context
from .frontend_support import npm


def check(context: Context, _args: Namespace) -> int:
    return npm(context, ["exec", "--", "prettier", ".", "--check"])


def fix(context: Context, _args: Namespace) -> int:
    return npm(context, ["exec", "--", "prettier", ".", "--write"])
