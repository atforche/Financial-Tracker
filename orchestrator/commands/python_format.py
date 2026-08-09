"""Format Python source."""

from argparse import Namespace

from ..core.context import Context
from .python_support import quality_python


def check(context: Context, _args: Namespace) -> int:
    return context.runner.run(
        [quality_python(context), "-m", "ruff", "format", "--check", "orchestrator"],
        cwd=context.root,
    ).returncode


def fix(context: Context, _args: Namespace) -> int:
    return context.runner.run(
        [quality_python(context), "-m", "ruff", "format", "orchestrator"],
        cwd=context.root,
    ).returncode
