"""Verify configured toolchain versions."""

from __future__ import annotations

import json
import platform
import re
from argparse import Namespace
from pathlib import Path

from ..config.toolchain import Toolchain
from ..core.context import Context
from .common import find_tool


def run(context: Context, _args: Namespace) -> int:
    expected = Toolchain.read(context.paths.toolchain).tools
    actual = {
        "python": platform.python_version(),
        "node": _tool_version(context, "node", ["node", "--version"]),
        "dotnet": _tool_version(context, "dotnet", ["dotnet", "--version"]),
        "trivy": _tool_version(context, "trivy", ["trivy", "--version"]),
    }
    failures: list[str] = []
    global_sdk = _global_sdk_version(context.root)
    if "dotnet" in expected:
        print(f"global.json dotnet: {global_sdk} (required {expected['dotnet']})")
        if global_sdk != expected["dotnet"]:
            failures.append("global.json")
    for name, wanted in expected.items():
        value = actual.get(name, "missing")
        print(f"{name}: {value} (required {wanted})")
        if value == "missing" or not value.startswith(str(wanted)):
            failures.append(name)
    if failures:
        raise RuntimeError(
            "Toolchain requirements are not satisfied: " + ", ".join(failures)
        )
    return 0


def _global_sdk_version(root: Path) -> str:
    """Read the SDK pin consumed by the .NET CLI."""

    path = root / "global.json"
    try:
        with path.open(encoding="utf-8") as file:
            value = json.load(file)["sdk"]["version"]
    except (OSError, KeyError, TypeError, json.JSONDecodeError) as error:
        raise RuntimeError(
            f"global.json does not contain a valid SDK version: {path}"
        ) from error
    if not isinstance(value, str) or not value:
        raise RuntimeError(f"global.json does not contain a valid SDK version: {path}")
    return value


def _tool_version(context: Context, name: str, command: list[str]) -> str:
    executable = find_tool(context, name)
    if executable is None:
        return "missing"
    result = context.runner.run(
        [executable, *command[1:]], cwd=context.root, capture_output=True
    )
    output = (
        (result.stdout or result.stderr).splitlines()[0]
        if (result.stdout or result.stderr)
        else ""
    )
    match = re.search(r"\d+(?:\.\d+){1,2}", output)
    return match.group(0) if match else output
