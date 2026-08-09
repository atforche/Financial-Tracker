"""Shared local debug environment helpers."""

from __future__ import annotations

from argparse import Namespace
from urllib.parse import urlsplit

from ..config.environment import EnvironmentSchema, read_dotenv
from ..config.toolchain import Toolchain
from ..core.context import Context
from ..core.paths import RepoPaths
from .common import ensure_directory
from .env_render import run as render


def prepare(context: Context) -> None:
    ensure_directory(context.paths.debug)
    ensure_directory(context.paths.debug_data, 0o777)
    ensure_directory(context.paths.debug_logs, 0o777)
    render(
        context,
        Namespace(
            profile="debug", output=str(context.paths.debug_environment), force=False
        ),
    )


def compose_environment(context: Context) -> dict[str, str]:
    """Provide Compose with the base images selected by the repository toolchain."""

    toolchain = Toolchain.read(context.paths.toolchain)
    return {
        "DEBUG_BACKEND_PORT": str(default_debug_backend_port()),
        "DEBUG_FRONTEND_PORT": str(default_debug_frontend_port()),
        "NODE_IMAGE": toolchain.require_base_image("node"),
        "DOTNET_SDK_IMAGE": toolchain.require_base_image("dotnet_sdk"),
        "DOTNET_RUNTIME_IMAGE": toolchain.require_base_image("dotnet_runtime"),
        "ALPINE_IMAGE": toolchain.require_base_image("alpine"),
    }


def default_debug_backend_port() -> int:
    """Return the debug backend port from the environment contract."""

    return int(_debug_default("ASPNETCORE_HTTP_PORTS"))


def default_debug_frontend_port() -> int:
    """Return the debug frontend port from the configured public origin."""

    origin = _debug_default("PUBLIC_ORIGIN")
    parsed = urlsplit(origin)
    if parsed.port is None:
        raise ValueError("The debug PUBLIC_ORIGIN must include a port")
    return parsed.port


def _debug_default(name: str) -> str:
    schema = EnvironmentSchema.read(RepoPaths.discover().environment_schema)
    value = schema.variables[name].default_for("debug")
    if not value:
        raise ValueError(f"The debug default for {name} is not configured")
    return value


def environment(context: Context) -> dict[str, str]:
    if not context.paths.debug_environment.is_file():
        raise RuntimeError("Debug configuration is missing. Run 'ft debug create'.")
    return read_dotenv(context.paths.debug_environment)


def apply_migrations(context: Context) -> int:
    values = environment(context)
    return context.runner.run(
        ["dotnet", "run", "--project", str(context.paths.migrator_project)],
        cwd=context.root,
        env=values,
    ).returncode
