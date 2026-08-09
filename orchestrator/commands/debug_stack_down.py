"""Stop the local Compose stack."""

from argparse import Namespace

from ..core.context import Context
from .debug_support import compose_environment


def run(context: Context, _args: Namespace) -> int:
    return context.runner.run(
        [
            "docker",
            "compose",
            "--file",
            str(context.paths.compose_dev),
            "down",
        ],
        cwd=context.root,
        env=compose_environment(context),
    ).returncode
