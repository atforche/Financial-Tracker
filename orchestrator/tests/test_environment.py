from argparse import Namespace
from pathlib import Path

import pytest

from orchestrator.commands import env_render
from orchestrator.config.environment import EnvironmentSchema, read_dotenv
from orchestrator.core.context import Context
from orchestrator.core.paths import RepoPaths


def test_read_dotenv_preserves_empty_quoted_values(tmp_path: Path):
    path = tmp_path / ".env"
    path.write_text('EMPTY=""\nVALUE="hello world"\n', encoding="utf-8")

    assert read_dotenv(path) == {"EMPTY": "", "VALUE": "hello world"}


def test_debug_schema_reports_missing_required_values(tmp_path: Path):
    schema = EnvironmentSchema.read(Path("config/environment.toml"))

    errors = schema.validate("debug", {"AUTH_MODE": "development"})

    assert "PUBLIC_ORIGIN must be configured for profile debug" in errors
    assert "AUTH_SECRET must be configured for profile debug" in errors


def test_production_schema_derives_required_values_from_variable_metadata():
    schema = EnvironmentSchema.read(Path("config/environment.toml"))

    errors = schema.validate(
        "production",
        {
            "INSTANCE_NAME": "tracker",
            "ENVIRONMENT": "Production",
            "PUBLIC_ORIGIN": "https://tracker.example.com",
            "BACKEND_IMAGE": "backend@sha256:abc",
            "FRONTEND_IMAGE": "frontend@sha256:abc",
            "MIGRATOR_IMAGE": "migrator@sha256:abc",
            "GOOGLE_CLIENT_ID": "client-id",
            "GOOGLE_CLIENT_SECRET": "client-secret",
            "AUTH_SECRET": "auth-secret",
            "AUTH_MODE": "google",
        },
    )

    assert "CADDY_IMAGE must be configured for profile production" in errors
    assert "INSTANCE_DIR must be configured for profile production" in errors


def test_schema_rejects_non_https_public_origin():
    schema = EnvironmentSchema.read(Path("config/environment.toml"))

    errors = schema.validate("production", {"PUBLIC_ORIGIN": "http://localhost"})

    assert "PUBLIC_ORIGIN must be an HTTPS origin with a hostname" in errors


def test_schema_documents_transient_bootstrap_environment_variable():
    schema = EnvironmentSchema.read(Path("config/environment.toml"))

    variable = schema.variables["BOOTSTRAP_ADMIN_EMAIL"]

    assert variable.derived
    assert "administrator" in variable.description


def test_debug_environment_render_includes_native_runtime_paths(tmp_path: Path):
    root = tmp_path
    config = root / "config"
    config.mkdir(parents=True)
    (config / "environment.toml").write_text(
        "\n".join(
            (
                "[profiles.debug]",
                "",
                "[variables.AUTH_SECRET]",
                'required_in = ["debug"]',
                "",
                "[variables.DATABASE_PATH]",
                'required_in = ["debug"]',
                "",
                "[variables.LOG_DIRECTORY]",
                'required_in = ["debug"]',
                "",
            )
        ),
        encoding="utf-8",
    )
    output = root / "debug" / ".env"

    result = env_render.run(
        Context(paths=RepoPaths(root)),
        Namespace(profile="debug", output=str(output), force=False),
    )

    assert result == 0
    values = read_dotenv(output)
    assert values["DATABASE_PATH"] == str(root / "debug" / "data" / "database.db")
    assert values["LOG_DIRECTORY"] == str(root / "debug" / "logs")
    assert values["AUTH_SECRET"]


def test_dotenv_rejects_malformed_lines(tmp_path: Path):
    path = tmp_path / ".env"
    path.write_text("not-an-assignment\n", encoding="utf-8")

    with pytest.raises(ValueError, match="Invalid dotenv line"):
        read_dotenv(path)
