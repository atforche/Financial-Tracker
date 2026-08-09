"""Run backend tests."""

from argparse import Namespace

from ..core.context import Context


def run(context: Context, args: Namespace) -> int:
    command = [
        "dotnet",
        "test",
        str(context.paths.backend_solution),
        "--no-build",
        "--no-restore",
        "--verbosity",
        "minimal",
    ]
    return context.runner.run(command, cwd=context.root).returncode
