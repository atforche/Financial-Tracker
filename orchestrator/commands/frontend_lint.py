"""Lint frontend source."""

from argparse import Namespace

from ..core.context import Context
from .frontend_support import npm


def run(context: Context, _args: Namespace) -> int:
    return npm(
        context,
        [
            "exec",
            "--",
            "eslint",
            ".",
            "--ext",
            "ts,tsx",
            "--report-unused-disable-directives",
            "--max-warnings",
            "0",
        ],
    )
