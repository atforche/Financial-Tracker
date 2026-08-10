"""Run the native debug backend."""

from argparse import Namespace

from ..core.context import Context
from .debug_support import environment


def run(context: Context, _args: Namespace) -> int:
    values = environment(context)
    print("Run Debug Backend", flush=True)
    return context.runner.run(
        ["dotnet", "run", "--no-build", "--no-restore"],
        cwd=context.paths.backend / "Rest",
        env=values,
    ).returncode
