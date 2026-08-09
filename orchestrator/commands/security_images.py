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
    print(
        "Caddy findings are reported without failing CI; review this exception when Caddy releases a new image."
    )
    caddy_image = toolchain.require_image("caddy")
    context.runner.run(["docker", "pull", caddy_image], cwd=context.root)
    return context.runner.run(
        [
            trivy,
            "image",
            "--exit-code",
            "0",
            "--severity",
            "HIGH,CRITICAL",
            "--scanners",
            "vuln",
            caddy_image,
        ],
        cwd=context.root,
    ).returncode
