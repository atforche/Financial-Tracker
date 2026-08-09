"""Command-line entrypoint and explicit command registry."""

from __future__ import annotations

import argparse
import sys

from .commands.common import Handler
from .core.context import Context


def _group(
    parser: argparse._SubParsersAction[argparse.ArgumentParser],
    name: str,
    description: str,
) -> argparse._SubParsersAction[argparse.ArgumentParser]:
    group = parser.add_parser(name, help=description, description=description)
    return group.add_subparsers(dest=f"{name}_command", required=True)


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="ft",
        description="Authoritative Financial Tracker repository orchestrator",
    )
    groups = parser.add_subparsers(dest="group", required=True)

    from .commands import (
        backend,
        backup,
        container,
        debug,
        deploy,
        deps,
        env,
        frontend,
        pipeline,
        python,
        release,
        security,
    )

    python.register(_group(groups, "python", "Maintain the repository Python tooling"))
    deps.register(_group(groups, "deps", "Install and verify repository dependencies"))
    backend.register(_group(groups, "backend", "Build and maintain the .NET backend"))
    frontend.register(_group(groups, "frontend", "Build and run the frontend"))
    debug.register(_group(groups, "debug", "Manage local development environments"))
    container.register(_group(groups, "container", "Build and smoke-test images"))
    security.register(_group(groups, "security", "Scan dependencies and images"))
    release.register(_group(groups, "release", "Validate release artifacts"))
    deploy.register(_group(groups, "deploy", "Deploy and roll back instances"))
    backup.register(_group(groups, "backup", "Create and verify encrypted backups"))
    env.register(_group(groups, "env", "Validate and inspect environment profiles"))
    pipeline.register(_group(groups, "ci", "Run repository verification workflows"))
    return parser


def main(argv: list[str] | None = None) -> int:
    """Parse and execute one command."""

    parser = build_parser()
    arguments = parser.parse_args(argv)
    handler: Handler = arguments.handler
    try:
        result = handler(Context(), arguments)
        return int(result or 0)
    except KeyboardInterrupt, BrokenPipeError:
        return 130
    except Exception as error:  # command boundary: provide one consistent CLI error
        print(f"error: {error}", file=sys.stderr)
        return 1
