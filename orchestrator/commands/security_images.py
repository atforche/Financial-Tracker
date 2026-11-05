"""Scan deployable container images."""

import os
from argparse import Namespace
from pathlib import Path

from ..config.toolchain import Toolchain
from ..core.context import Context
from .common import find_tool


def _report(context: Context, command: list[str], image: str) -> None:
    """Record a scan's result without failing the build."""

    result = context.runner.run(
        command, cwd=context.root, check=False, capture_output=True
    )
    report = (
        result.stdout
        if result.returncode == 0
        else f"Image scan could not complete:\n{result.stderr}"
    )
    print(report)
    summary_path = os.environ.get("GITHUB_STEP_SUMMARY")
    if summary_path:
        with Path(summary_path).open("a", encoding="utf-8") as summary:
            summary.write(f"### {image}\n\n```text\n{report}\n```\n\n")


def run(context: Context, _args: Namespace) -> int:
    scan_mode = os.environ.get("IMAGE_SCAN_MODE", "").strip().lower()
    # "report" records findings for review, while the default fails on them.
    if scan_mode not in ("", "report"):
        raise ValueError(f"Unsupported image scan mode: {scan_mode}")
    report_only = scan_mode == "report"
    trivy = find_tool(context, "trivy")
    if trivy is None:
        raise RuntimeError("Trivy is not installed. Run 'ft deps install'.")
    toolchain = Toolchain.read(context.paths.toolchain)
    for image in (
        toolchain.require_image("backend"),
        toolchain.require_image("frontend"),
        toolchain.require_image("migrator"),
    ):
        command = [
            trivy,
            "image",
            "--exit-code",
            "0" if report_only else "1",
            "--severity",
            "HIGH,CRITICAL",
            "--scanners",
            "vuln",
            image,
        ]
        if report_only:
            _report(context, command, image)
        else:
            context.runner.run(command, cwd=context.root)
    return 0
