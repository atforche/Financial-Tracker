"""Deployment configuration read from and written to instance dotenv files."""

from __future__ import annotations

import os
import secrets
from dataclasses import dataclass
from enum import Enum
from pathlib import Path
from urllib.parse import urlsplit

from ..config.environment import EnvironmentSchema, read_dotenv, write_dotenv
from ..config.toolchain import Toolchain
from ..core.paths import RepoPaths


class Environment(Enum):
    """Supported deployment environments."""

    DEVELOPMENT = "Development"
    PRODUCTION = "Production"


@dataclass
class Configuration:
    """Complete configuration for one deployed instance."""

    name: str
    path: str
    environment: Environment
    public_origin: str
    google_client_id: str
    google_client_secret: str
    auth_secret: str
    backend_image: str
    frontend_image: str
    migrator_image: str

    def write_to_file(self, environment_file_path: str | None = None) -> None:
        """Write the deployment configuration as a dotenv file."""

        target = Path(environment_file_path or self.get_environment_file_path())
        write_dotenv(
            target,
            {
                "INSTANCE_NAME": self.name,
                "INSTANCE_DIR": self.path,
                "ENVIRONMENT": self.environment.value,
                "PUBLIC_ORIGIN": self.public_origin,
                "PUBLIC_HOST": self.get_public_host(),
                "PUBLIC_PORT": str(self.get_public_port()),
                "BACKEND_IMAGE": self.backend_image,
                "FRONTEND_IMAGE": self.frontend_image,
                "MIGRATOR_IMAGE": self.migrator_image,
                "CADDY_IMAGE": self._toolchain().require_image("caddy"),
                "GOOGLE_CLIENT_ID": self.google_client_id,
                "GOOGLE_CLIENT_SECRET": self.google_client_secret,
                "AUTH_SECRET": self.auth_secret,
                "AUTH_MODE": (
                    "development"
                    if self.environment is Environment.DEVELOPMENT
                    else "google"
                ),
            },
            self._schema(),
            "production",
        )

    def get_public_host(self) -> str:
        parsed_origin = urlsplit(self.public_origin)
        if parsed_origin.scheme != "https" or parsed_origin.hostname is None:
            raise ValueError("PUBLIC_ORIGIN must be an HTTPS origin with a hostname")
        return parsed_origin.hostname

    def get_public_port(self) -> int:
        parsed_origin = urlsplit(self.public_origin)
        if parsed_origin.scheme != "https" or parsed_origin.hostname is None:
            raise ValueError("PUBLIC_ORIGIN must be an HTTPS origin with a hostname")
        try:
            return parsed_origin.port or 443
        except ValueError as error:
            raise ValueError("PUBLIC_ORIGIN must use a valid HTTPS port") from error

    def get_database_file_path(self) -> str:
        return f"{self.path}/data/database.db"

    def get_environment_file_path(self) -> str:
        return f"{self.path}/.env"

    @staticmethod
    def _schema() -> EnvironmentSchema:
        paths = RepoPaths.discover()
        return EnvironmentSchema.read(paths.environment_schema)

    @staticmethod
    def _toolchain() -> Toolchain:
        paths = RepoPaths.discover()
        return Toolchain.read(paths.toolchain)

    @classmethod
    def build_from_existing_instance(
        cls, instance_path: str, change_configuration: bool
    ) -> Configuration:
        values = read_dotenv(Path(instance_path) / ".env")
        values["CADDY_IMAGE"] = Configuration._toolchain().require_image("caddy")
        values.setdefault("AUTH_MODE", "google")
        required = Configuration._schema().profiles["production"]
        missing = [
            name for name in sorted(required) if not values.get(name, "").strip()
        ]
        if missing:
            raise ValueError(
                f"Instance configuration is missing required values: {', '.join(missing)}"
            )

        def value(name: str, prompt: str, secret: bool = False) -> str:
            current = values[name]
            if not change_configuration:
                return current
            default = "configured" if secret else current
            return input(f"Enter the {prompt} [{default}]: ") or current

        environment_value = value(
            "ENVIRONMENT", "the environment (Development/Production)"
        )
        try:
            environment = Environment(environment_value)
        except ValueError as error:
            raise ValueError(
                f"Unsupported deployment environment: {environment_value}"
            ) from error
        result = cls(
            value("INSTANCE_NAME", "the instance name"),
            str(Path(instance_path).resolve()),
            environment,
            value("PUBLIC_ORIGIN", "public HTTPS origin"),
            value("GOOGLE_CLIENT_ID", "Google OAuth client ID"),
            value("GOOGLE_CLIENT_SECRET", "Google OAuth client secret", True),
            value("AUTH_SECRET", "Auth.js session secret", True),
            value("BACKEND_IMAGE", "backend image"),
            value("FRONTEND_IMAGE", "frontend image"),
            value("MIGRATOR_IMAGE", "migrator image"),
        )
        for name, item in (
            ("PUBLIC_ORIGIN", result.public_origin),
            ("GOOGLE_CLIENT_ID", result.google_client_id),
            ("GOOGLE_CLIENT_SECRET", result.google_client_secret),
            ("AUTH_SECRET", result.auth_secret),
        ):
            if not item.strip():
                raise ValueError(f"{name} must not be empty")
        return result

    @classmethod
    def build_from_environment(
        cls, path: str, backend_image: str, frontend_image: str, migrator_image: str
    ) -> Configuration:
        toolchain = cls._toolchain()
        schema = cls._schema()
        environment_value = (
            os.environ.get("ENVIRONMENT", "") or Environment.PRODUCTION.value
        )
        auth_secret = os.environ.get("AUTH_SECRET", "") or secrets.token_urlsafe(48)
        authentication_mode = (
            "development"
            if environment_value == Environment.DEVELOPMENT.value
            else "google"
        )
        bootstrap_values = dict(os.environ)
        bootstrap_values.update(
            {
                "ENVIRONMENT": environment_value,
                "AUTH_SECRET": auth_secret,
                "AUTH_MODE": authentication_mode,
                "BACKEND_IMAGE": backend_image,
                "FRONTEND_IMAGE": frontend_image,
                "MIGRATOR_IMAGE": migrator_image,
                "CADDY_IMAGE": toolchain.require_image("caddy"),
            }
        )
        required = {
            name
            for name in schema.profiles["production"]
            if not schema.variables[name].derived
        }
        missing = [
            name
            for name in sorted(required)
            if not bootstrap_values.get(name, "").strip()
        ]
        if missing:
            raise ValueError(
                "The following environment variables must be configured for bootstrap: "
                + ", ".join(missing)
            )
        try:
            environment = Environment(environment_value)
        except ValueError as error:
            raise ValueError(
                f"Unsupported deployment environment: {environment_value}"
            ) from error
        return cls(
            os.environ["INSTANCE_NAME"],
            path,
            environment,
            os.environ["PUBLIC_ORIGIN"],
            os.environ["GOOGLE_CLIENT_ID"],
            os.environ["GOOGLE_CLIENT_SECRET"],
            auth_secret,
            backend_image,
            frontend_image,
            migrator_image,
        )
