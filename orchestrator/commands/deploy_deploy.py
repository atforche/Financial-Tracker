"""Deploy an immutable release."""

from argparse import Namespace

from ..core.context import Context
from ..operations.deployment import deploy_release


def run(context: Context, args: Namespace) -> int:
    deploy_release(
        args.path, args.release_manifest, args.change_configuration, context.runner
    )
    return 0
