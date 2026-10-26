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
    toolchain = Toolchain.read(context.paths.toolchain)
    expected = toolchain.tools
    actual = {
        "python": platform.python_version(),
        "node": _tool_version(context, "node", ["node", "--version"]),
        "trivy": _tool_version(context, "trivy", ["trivy", "--version"]),
    }
    failures = _dotnet_failures(context, toolchain) if "dotnet" in expected else []
    for name, wanted in expected.items():
        if name == "dotnet":
            continue
        value = actual.get(name, "missing")
        print(f"{name}: {value} (required {wanted})")
        if value == "missing" or not value.startswith(str(wanted)):
            failures.append(name)
    if failures:
        raise RuntimeError(
            "Toolchain requirements are not satisfied: " + ", ".join(failures)
        )
    return 0


def check_dotnet(context: Context, _args: Namespace) -> int:
    """Fail unless the active .NET SDK and every SDK pin are the same exact version.

    Analyzer rules change between SDK releases, so a build on any other SDK can
    pass or fail differently than CI and the container images.
    """

    return _raise_dotnet_failures(
        _dotnet_failures(context, Toolchain.read(context.paths.toolchain))
    )


def check_dotnet_pins(context: Context, _args: Namespace) -> int:
    """Fail unless every .NET SDK pin, including the container build image, is the same exact version."""

    toolchain = Toolchain.read(context.paths.toolchain)
    return _raise_dotnet_failures(
        _dotnet_failures(context, toolchain, include_active=False)
    )


def _raise_dotnet_failures(failures: list[str]) -> int:
    if failures:
        raise RuntimeError(
            ".NET SDK versions do not match the toolchain: " + ", ".join(failures)
        )
    return 0


def _dotnet_failures(
    context: Context, toolchain: Toolchain, include_active: bool = True
) -> list[str]:
    wanted = toolchain.require_tool("dotnet")
    global_json = _global_json_sdk(context.root)
    image = toolchain.require_base_image("dotnet_sdk")
    checks = {
        "global.json": global_json.get("version"),
        "global.json rollForward": global_json.get("rollForward"),
        "dotnet_sdk image": image.split("@", 1)[0].rsplit(":", 1)[-1],
    }
    if include_active:
        checks["dotnet"] = _active_dotnet_sdk(context)
    required = {"global.json rollForward": "disable"}
    failures: list[str] = []
    for name, value in checks.items():
        expected = required.get(name, wanted)
        print(f"{name}: {value} (required {expected})")
        if value != expected:
            failures.append(name)
    return failures


def _global_json_sdk(root: Path) -> dict[str, str]:
    """Read the SDK pin consumed by the .NET CLI."""

    path = root / "global.json"
    try:
        with path.open(encoding="utf-8") as file:
            value = json.load(file)["sdk"]
    except (OSError, KeyError, TypeError, json.JSONDecodeError) as error:
        raise RuntimeError(
            f"global.json does not contain a valid SDK version: {path}"
        ) from error
    if not isinstance(value, dict) or not isinstance(value.get("version"), str):
        raise RuntimeError(f"global.json does not contain a valid SDK version: {path}")
    return {key: str(item) for key, item in value.items()}


def _active_dotnet_sdk(context: Context) -> str:
    """Return the SDK the .NET CLI resolves from the repository root, or why it failed."""

    executable = find_tool(context, "dotnet")
    if executable is None:
        return "missing"
    result = context.runner.run(
        [executable, "--version"], cwd=context.root, capture_output=True, check=False
    )
    if result.returncode != 0:
        return "not installed"
    return (result.stdout or "").strip()


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
