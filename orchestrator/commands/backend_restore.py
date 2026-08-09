"""Restore the backend solution."""

from argparse import Namespace

from ..core.context import Context


def run(context: Context, _args: Namespace) -> int:
    return context.runner.run(
        ["dotnet", "restore", str(context.paths.backend_solution)], cwd=context.root
    ).returncode
