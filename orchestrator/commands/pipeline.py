"""Typed CI workflows composed from the individual command handlers."""

from __future__ import annotations

from argparse import Namespace
from collections.abc import Callable

from ..core.context import Context
from . import (
    backend_build,
    backend_coverage,
    backend_restore,
    container_build,
    container_smoke_test,
    frontend_install,
    frontend_lint,
    frontend_models,
    frontend_playwright_install,
    python_format,
    python_install,
    python_lint,
    python_test,
    python_typecheck,
    security_dependencies,
    security_images,
    trivy_install,
)
from . import (
    backend_format as backend_format_command,
)
from . import (
    frontend_build as frontend_build_command,
)
from . import (
    frontend_format as frontend_format_command,
)
from .common import add_command

Workflow = Callable[[Context], int]


def _run(
    context: Context,
    commands: list[Callable[[Context, Namespace], int]],
    arguments: Namespace | None = None,
) -> int:
    values = arguments or Namespace(verify=False)
    for command in commands:
        command(context, values)
    return 0


def python_workflow(context: Context, _args: Namespace) -> int:
    return _run(
        context,
        [
            python_install.run,
            python_format.check,
            python_lint.run,
            python_typecheck.run,
            python_test.run,
        ],
    )


def backend_format(context: Context, _args: Namespace) -> int:
    return _run(context, [backend_restore.run, backend_format_command.run])


def frontend_format(context: Context, _args: Namespace) -> int:
    return _run(
        context,
        [frontend_install.run, frontend_format_command.check, frontend_lint.run],
    )


def backend_test(context: Context, _args: Namespace) -> int:
    return _run(context, [backend_restore.run, backend_build.run, backend_coverage.run])


def frontend_build(context: Context, _args: Namespace) -> int:
    return _run(context, [frontend_install.run, frontend_build_command.run])


def api_contract(context: Context, _args: Namespace) -> int:
    frontend_install.run(context, Namespace())
    return frontend_models.run(context, Namespace(verify=True))


def dependencies(context: Context, _args: Namespace) -> int:
    return _run(context, [backend_restore.run, security_dependencies.run])


def container_images(context: Context, _args: Namespace) -> int:
    return _run(
        context,
        [
            frontend_install.run,
            frontend_playwright_install.run,
            container_build.run,
            trivy_install.run,
            security_images.run,
            container_smoke_test.run,
        ],
    )


def run_all(context: Context, _args: Namespace) -> int:
    for workflow in (
        python_workflow,
        backend_format,
        frontend_format,
        backend_test,
        frontend_build,
        api_contract,
        dependencies,
        container_images,
    ):
        workflow(context, Namespace())
    return 0


def register(commands: object) -> None:
    add_command(
        commands,
        "python",
        "Run Python formatting, linting, typing, and tests",
        python_workflow,
    )
    add_command(
        commands, "backend-format", "Restore and format the backend", backend_format
    )
    add_command(
        commands,
        "frontend-format",
        "Install, format, and lint the frontend",
        frontend_format,
    )
    add_command(
        commands, "backend-test", "Restore, build, and test the backend", backend_test
    )
    add_command(
        commands, "frontend-build", "Install and build the frontend", frontend_build
    )
    add_command(
        commands, "api-contract", "Verify generated frontend API models", api_contract
    )
    add_command(
        commands,
        "dependencies",
        "Restore and scan application dependencies",
        dependencies,
    )
    add_command(
        commands,
        "container-images",
        "Build, scan, and smoke-test images",
        container_images,
    )
    add_command(commands, "run", "Run every repository verification workflow", run_all)
