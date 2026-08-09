"""Verify backend formatting."""

from argparse import Namespace

from ..core.context import Context


def run(context: Context, _args: Namespace) -> int:
    return context.runner.run(
        [
            "dotnet",
            "format",
            str(context.paths.backend_solution),
            "--verify-no-changes",
            "--no-restore",
            "--severity",
            "info",
        ],
        cwd=context.root,
    ).returncode
