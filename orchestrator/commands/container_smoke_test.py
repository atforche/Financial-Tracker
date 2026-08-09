"""Run the container integration smoke test."""

from argparse import Namespace

from ..core.context import Context
from ..operations.container_smoke import ContainerSmokeTest


def run(context: Context, _args: Namespace) -> int:
    ContainerSmokeTest(context.paths, context.runner).run()
    return 0
