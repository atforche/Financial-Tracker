"""Test Python source."""

from argparse import Namespace

from ..core.context import Context
from .python_support import quality_python


def run(context: Context, _args: Namespace) -> int:
    return context.runner.run(
        [
            quality_python(context),
            "-m",
            "pytest",
            "--cov=orchestrator",
            "--cov-report=term-missing",
        ],
        cwd=context.root,
    ).returncode
