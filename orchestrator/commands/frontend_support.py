"""Shared frontend command helpers."""

from ..core.context import Context


def npm(context: Context, arguments: list[str]) -> int:
    return context.runner.run(
        ["npm", *arguments], cwd=context.paths.frontend
    ).returncode
