"""Run the native debug frontend."""

from argparse import Namespace

from ..core.context import Context
from .debug_support import default_debug_frontend_port, environment


def run(context: Context, args: Namespace) -> int:
    values = environment(context)
    print("Run Debug Frontend", flush=True)
    command = [
        "npm",
        "exec",
        "--",
        "next",
        "dev",
        "--port",
        str(getattr(args, "port", default_debug_frontend_port())),
    ]
    if getattr(args, "inspect", False):
        command.append("--inspect")
    return context.runner.run(
        command,
        cwd=context.paths.frontend,
        env=values,
    ).returncode
