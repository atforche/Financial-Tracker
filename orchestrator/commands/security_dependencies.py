"""Scan frontend and backend dependencies."""

from __future__ import annotations

import json
from argparse import Namespace
from typing import Any

from ..core.context import Context


def run(context: Context, _args: Namespace) -> int:
    context.runner.run(
        ["npm", "audit", "--audit-level=high"], cwd=context.paths.frontend
    )
    result = context.runner.run(
        [
            "dotnet",
            "package",
            "list",
            "--project",
            str(context.paths.backend_solution),
            "--vulnerable",
            "--include-transitive",
            "--format",
            "json",
            "--no-restore",
        ],
        cwd=context.root,
        check=False,
        capture_output=True,
    )
    if result.stdout:
        print(result.stdout)
    if result.stderr:
        print(result.stderr)
    if result.returncode != 0:
        raise RuntimeError("NuGet vulnerability scan could not complete")
    vulnerabilities = _get_vulnerabilities(json.loads(result.stdout or "{}"))
    if vulnerabilities:
        raise RuntimeError(
            "NuGet reported vulnerable packages: " + ", ".join(sorted(vulnerabilities))
        )
    return 0


def _get_vulnerabilities(value: Any) -> set[str]:
    if isinstance(value, list):
        return set().union(*(_get_vulnerabilities(item) for item in value))
    if not isinstance(value, dict):
        return set()
    vulnerabilities = value.get("vulnerabilities")
    package_id = value.get("id")
    package_version = value.get("resolvedVersion") or value.get("requestedVersion")
    found = set()
    if (
        isinstance(vulnerabilities, list)
        and vulnerabilities
        and isinstance(package_id, str)
    ):
        found.add(f"{package_id} {package_version or ''}".strip())
    return found.union(*(_get_vulnerabilities(item) for item in value.values()))
