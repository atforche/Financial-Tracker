"""Scan deployable container images."""

from argparse import Namespace

from ..config.toolchain import Toolchain
from ..core.context import Context
from .common import find_tool


def run(context: Context, _args: Namespace) -> int:
    trivy = find_tool(context, "trivy")
    if trivy is None:
        raise RuntimeError("Trivy is not installed. Run 'ft deps install'.")
    toolchain = Toolchain.read(context.paths.toolchain)
    for image in (
        toolchain.require_image("backend"),
        toolchain.require_image("frontend"),
        toolchain.require_image("migrator"),
    ):
        context.runner.run(
            [
                trivy,
                "image",
                "--exit-code",
                "1",
                "--severity",
                "HIGH,CRITICAL",
                "--scanners",
                "vuln",
                image,
            ],
            cwd=context.root,
        )
    return 0
