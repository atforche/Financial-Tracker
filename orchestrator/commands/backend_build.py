"""Build the backend solution."""

from argparse import Namespace

from ..core.context import Context


def run(context: Context, _args: Namespace) -> int:
    return context.runner.run(
        ["dotnet", "build", str(context.paths.backend_solution), "--no-restore"],
        cwd=context.root,
    ).returncode
