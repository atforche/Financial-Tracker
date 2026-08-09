"""Generate or verify frontend API models."""

from argparse import Namespace

from ..core.context import Context
from .frontend_support import npm


def run(context: Context, args: Namespace) -> int:
    input_path = context.paths.openapi_contract
    command = [
        "exec",
        "--",
        "openapi-typescript",
        str(input_path),
        "--output",
        str(context.paths.frontend_api_models.relative_to(context.paths.frontend)),
        "--enum",
    ]
    if args.verify:
        command.append("--check")
    return npm(context, command)
