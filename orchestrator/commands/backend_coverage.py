"""Run backend tests and enforce coverage thresholds."""

from __future__ import annotations

import xml.etree.ElementTree as ElementTree
from argparse import Namespace

from ..core.context import Context
from .backend_support import MINIMUM_BRANCH_COVERAGE, MINIMUM_LINE_COVERAGE


def run(context: Context, args: Namespace) -> int:
    results_directory = context.paths.backend_test_results
    command = [
        "dotnet",
        "test",
        str(context.paths.backend_solution),
        "--no-build",
        "--no-restore",
        "--verbosity",
        "minimal",
        "--collect",
        "XPlat Code Coverage",
        "--results-directory",
        str(results_directory),
    ]
    context.runner.run(command, cwd=context.root)
    coverage_files = list(results_directory.glob("**/coverage.cobertura.xml"))
    if not coverage_files:
        raise RuntimeError(
            "Coverage collection completed without producing a Cobertura report"
        )
    coverage_file = max(coverage_files, key=lambda path: path.stat().st_mtime)
    root = ElementTree.parse(coverage_file).getroot()
    line_rate = float(root.attrib["line-rate"])
    branch_rate = float(root.attrib["branch-rate"])
    print(f"Line coverage: {line_rate * 100:.2f}%")
    print(f"Branch coverage: {branch_rate * 100:.2f}%")
    print(f"Coverage report: {coverage_file.resolve()}")
    if line_rate < MINIMUM_LINE_COVERAGE or branch_rate < MINIMUM_BRANCH_COVERAGE:
        raise RuntimeError("Backend coverage is below the configured threshold")
    return 0
