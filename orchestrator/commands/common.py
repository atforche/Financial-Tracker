"""Helpers shared by individual command modules."""

from __future__ import annotations

import os
import shutil
from argparse import ArgumentParser, Namespace
from collections.abc import Callable
from pathlib import Path

from ..core.context import Context

Handler = Callable[[Context, Namespace], int | None]


def add_command(
    commands: object,
    name: str,
    description: str,
    handler: Handler,
    configure: Callable[[ArgumentParser], None] | None = None,
) -> ArgumentParser:
    """Register one command in a subparser collection."""

    parser = commands.add_parser(name, help=description, description=description)  # type: ignore[attr-defined]
    if configure is not None:
        configure(parser)
    parser.set_defaults(handler=handler)
    return parser


def add_path_argument(parser: ArgumentParser, name: str, help_text: str) -> None:
    parser.add_argument(f"--{name}", required=True, help=help_text)


def require_tool(name: str) -> str:
    executable = shutil.which(name)
    if executable is None:
        raise RuntimeError(f"Required tool is not installed or not on PATH: {name}")
    return executable


def find_tool(context: Context, name: str) -> str | None:
    """Find a repository-local tool before falling back to PATH."""

    local_tool = context.paths.local_tools / name
    if local_tool.is_file() and os.access(local_tool, os.X_OK):
        return str(local_tool)
    return shutil.which(name)


def command_environment(overrides: dict[str, str]) -> dict[str, str]:
    """Return a copy of the current environment with explicit overrides."""

    environment = os.environ.copy()
    environment.update(overrides)
    return environment


def ensure_directory(path: Path, mode: int = 0o755) -> None:
    path.mkdir(mode=mode, parents=True, exist_ok=True)
