"""Shared support for Python tooling commands."""

from __future__ import annotations

import platform
import shutil
import sys

from ..config.toolchain import Toolchain
from ..core.context import Context


def quality_python(context: Context) -> str:
    if not context.paths.quality_python.is_file():
        raise RuntimeError("Python tools are not installed. Run 'ft python install'.")
    return str(context.paths.quality_python)


def required_python(context: Context) -> str:
    version = Toolchain.read(context.paths.toolchain).require_tool("python")
    if platform.python_version().startswith(version):
        return sys.executable
    executable = shutil.which(f"python{version}")
    if executable is None:
        raise RuntimeError(
            f"Python {version} is required to create the repository virtual environment"
        )
    return executable
