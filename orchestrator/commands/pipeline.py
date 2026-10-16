"""Typed CI workflows composed from the individual command handlers."""

from __future__ import annotations

from argparse import Namespace
from collections.abc import Callable

from ..core.context import Context
from . import (
    backend_build,
    backend_coverage,
    backend_format,
    backend_restore,
    container_build,
    container_smoke_test,
    frontend_format,
    frontend_install,
    frontend_lint,
    frontend_models,
    frontend_playwright_install,
    frontend_typecheck,
    python_format,
    python_install,
    python_lint,
    python_test,
    python_typecheck,
    security_dependencies,
    security_images,
    trivy_install,
)
from .common import add_command


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


def backend(context: Context, _args: Namespace) -> int:
    return _run(
        context,
        [
            backend_restore.run,
            backend_format.run,
            backend_build.run,
            backend_coverage.run,
        ],
    )


def frontend(context: Context, _args: Namespace) -> int:
    return _run(
        context,
        [
            frontend_install.run,
            frontend_format.check,
            frontend_lint.run,
            frontend_typecheck.run,
        ],
    )


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
            container_smoke_test.run,
        ],
    )


def container_scan(context: Context, _args: Namespace) -> int:
    return _run(context, [trivy_install.run, security_images.run])


def container_release(context: Context, _args: Namespace) -> int:
    return _run(context, [container_build.run, trivy_install.run, security_images.run])


def run_all(context: Context, _args: Namespace) -> int:
    for workflow in (
        python_workflow,
        backend,
        frontend,
        api_contract,
        dependencies,
        container_images,
        container_scan,
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
        commands,
        "backend",
        "Restore, format-check, build, and test the backend",
        backend,
    )
    add_command(
        commands,
        "frontend",
        "Install, format-check, lint, and type-check the frontend",
        frontend,
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
        "Build and smoke-test images",
        container_images,
    )
    add_command(
        commands,
        "container-scan",
        "Scan previously built images",
        container_scan,
    )
    add_command(
        commands,
        "container-release",
        "Build and scan images without smoke-testing them",
        container_release,
    )
    add_command(commands, "run", "Run every repository verification workflow", run_all)
