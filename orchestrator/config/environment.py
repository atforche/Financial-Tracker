"""Central environment schema and profile validation."""

from __future__ import annotations

import os
import shlex
import tomllib
from collections.abc import Mapping
from dataclasses import dataclass
from pathlib import Path
from urllib.parse import urlsplit


@dataclass(frozen=True)
class VariableSpec:
    """Metadata for one environment variable."""

    name: str
    description: str
    secret: bool
    required_in: frozenset[str]
    derived: bool = False
    render_in: frozenset[str] = frozenset()
    defaults: dict[str, str] | None = None

    def default_for(self, profile: str) -> str:
        return (self.defaults or {}).get(profile, "")


@dataclass(frozen=True)
class EnvironmentSchema:
    """Loaded schema for all application and orchestrator environment variables."""

    variables: dict[str, VariableSpec]
    profiles: dict[str, frozenset[str]]
    https_origins: dict[str, frozenset[str]]

    @classmethod
    def read(cls, path: Path) -> EnvironmentSchema:
        with path.open("rb") as file:
            document = tomllib.load(file)

        variables = {
            name: VariableSpec(
                name=name,
                description=str(values.get("description", "")),
                secret=bool(values.get("secret", False)),
                required_in=frozenset(values.get("required_in", [])),
                derived=bool(values.get("derived", False)),
                render_in=frozenset(values.get("render_in", [])),
                defaults={
                    profile: str(value)
                    for profile, value in values.get("defaults", {}).items()
                },
            )
            for name, values in document.get("variables", {}).items()
        }
        profile_document = document.get("profiles", {})
        profile_names = set(profile_document)
        unknown_profiles = sorted(
            profile
            for variable in variables.values()
            for profile in (
                variable.required_in | variable.render_in | set(variable.defaults or {})
            )
            if profile not in profile_names
        )
        if unknown_profiles:
            raise ValueError(
                "Environment variables reference unknown profiles: "
                + ", ".join(sorted(set(unknown_profiles)))
            )
        profiles = {
            profile: frozenset(
                name
                for name, variable in variables.items()
                if profile in variable.required_in
            )
            for profile in profile_names
        }
        https_origins = {
            profile: frozenset(values.get("https_origins", []))
            for profile, values in profile_document.items()
        }
        return cls(variables, profiles, https_origins)

    def rendered_variables(self, profile: str) -> tuple[VariableSpec, ...]:
        """Return variables that belong in a generated profile file."""

        if profile not in self.profiles:
            raise ValueError(
                f"Unknown environment profile {profile!r}; choose from "
                + ", ".join(sorted(self.profiles))
            )
        return tuple(
            variable
            for variable in self.variables.values()
            if profile in variable.required_in or profile in variable.render_in
        )

    def validate(
        self,
        profile: str,
        values: Mapping[str, str],
        *,
        allow_unknown: bool = False,
    ) -> list[str]:
        """Return all missing, unknown, and invalid values for a profile."""

        if profile not in self.profiles:
            raise ValueError(
                f"Unknown environment profile {profile!r}; choose from "
                + ", ".join(sorted(self.profiles))
            )

        errors: list[str] = []
        required = self.profiles[profile]
        for name in sorted(required):
            if not values.get(name, "").strip():
                errors.append(f"{name} must be configured for profile {profile}")

        if not allow_unknown:
            unknown = sorted(set(values) - set(self.variables))
            errors.extend(f"Unknown environment variable: {name}" for name in unknown)

        for name, value in values.items():
            if name not in self.variables or not value.strip():
                continue
            if name.endswith("_PORT"):
                try:
                    port = int(value)
                except ValueError:
                    errors.append(f"{name} must be an integer port")
                else:
                    if not 1 <= port <= 65535:
                        errors.append(f"{name} must be between 1 and 65535")

        for name in sorted(self.https_origins.get(profile, ())):
            value = values.get(name, "").strip()
            if not value:
                continue
            parsed = urlsplit(value)
            try:
                origin_port = parsed.port
            except ValueError:
                errors.append(f"{name} must be an HTTPS origin with a valid port")
                continue
            if parsed.scheme != "https" or parsed.hostname is None:
                errors.append(f"{name} must be an HTTPS origin with a hostname")
            elif origin_port is not None and not 1 <= origin_port <= 65535:
                errors.append(f"{name} must use a port between 1 and 65535")

        return errors


def read_dotenv(path: Path) -> dict[str, str]:
    """Read the supported dotenv syntax without silently dropping empty values."""

    values: dict[str, str] = {}
    if not path.is_file():
        return values
    for line_number, raw_line in enumerate(
        path.read_text(encoding="utf-8").splitlines(), 1
    ):
        line = raw_line.strip()
        if not line or line.startswith("#"):
            continue
        if "=" not in line:
            raise ValueError(f"Invalid dotenv line {path}:{line_number}")
        name, raw_value = line.split("=", 1)
        name = name.strip()
        if not name or not name.replace("_", "").isalnum() or not name[0].isupper():
            raise ValueError(
                f"Invalid environment variable name at {path}:{line_number}"
            )
        try:
            parsed = shlex.split(raw_value, comments=False, posix=True)
        except ValueError as error:
            raise ValueError(f"Invalid dotenv value at {path}:{line_number}") from error
        values[name] = parsed[0] if parsed else ""
    return values


def write_dotenv(
    path: Path,
    values: Mapping[str, str],
    schema: EnvironmentSchema,
    profile: str,
) -> None:
    """Write the schema-owned variables for one profile to a dotenv file."""

    lines = [
        f'{variable.name}="{_quote_dotenv(values.get(variable.name, ""))}"'
        for variable in schema.rendered_variables(profile)
    ]
    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def _quote_dotenv(value: str) -> str:
    """Escape a value for the double-quoted dotenv format we emit."""

    return value.replace("\\", "\\\\").replace('"', '\\"').replace("\n", "\\n")


def merged_environment(path: Path | None = None) -> dict[str, str]:
    """Return the process environment with an optional dotenv file layered on top."""

    values = dict(os.environ)
    if path is not None:
        values.update(read_dotenv(path))
    return values
