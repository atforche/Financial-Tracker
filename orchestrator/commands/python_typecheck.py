"""Type-check Python source."""

from argparse import Namespace

from ..core.context import Context
from .python_support import quality_python


def run(context: Context, _args: Namespace) -> int:
    return context.runner.run(
        [quality_python(context), "-m", "mypy", "orchestrator"], cwd=context.root
    ).returncode
