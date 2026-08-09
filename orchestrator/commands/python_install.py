"""Install the repository Python environment."""

from __future__ import annotations

import tomllib
from argparse import Namespace

from ..config.toolchain import Toolchain
from ..core.context import Context
from .python_support import required_python


def run(context: Context, _args: Namespace) -> int:
    if not _quality_environment_matches(context):
        context.runner.run(
            [
                required_python(context),
                "-m",
                "venv",
                str(context.paths.python_environment),
            ],
            cwd=context.root,
        )
    _ensure_pip(context)
    with context.paths.pyproject.open("rb") as file:
        dependencies = tomllib.load(file).get("dependency-groups", {}).get("dev", [])
    if not dependencies:
        raise RuntimeError("No development dependencies are defined in pyproject.toml")
    context.runner.run(
        [str(context.paths.quality_python), "-m", "pip", "install", *dependencies],
        cwd=context.root,
    )
    context.runner.run(
        [
            str(context.paths.quality_python),
            "-m",
            "pip",
            "install",
            "--editable",
            ".",
            "--no-deps",
        ],
        cwd=context.root,
    )
    return 0


def _quality_environment_matches(context: Context) -> bool:
    if not context.paths.quality_python.is_file():
        return False
    expected = Toolchain.read(context.paths.toolchain).require_tool("python")
    result = context.runner.run(
        [str(context.paths.quality_python), "--version"],
        cwd=context.root,
        check=False,
        capture_output=True,
    )
    output = (result.stdout or result.stderr or "").strip()
    return result.returncode == 0 and output.removeprefix("Python ").startswith(
        expected
    )


def _ensure_pip(context: Context) -> None:
    """Repair an existing virtual environment that was created without pip."""

    pip_check = context.runner.run(
        [str(context.paths.quality_python), "-m", "pip", "--version"],
        cwd=context.root,
        check=False,
        capture_output=True,
    )
    if pip_check.returncode == 0:
        return
    context.runner.run(
        [
            str(context.paths.quality_python),
            "-m",
            "ensurepip",
            "--upgrade",
        ],
        cwd=context.root,
    )
