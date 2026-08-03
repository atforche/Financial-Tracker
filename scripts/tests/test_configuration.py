from __future__ import annotations

import pytest
from shared.configuration import Configuration, Environment


def build_configuration(
    public_origin: str = "https://tracker.example.com",
) -> Configuration:
    return Configuration(
        "tracker",
        "/srv/tracker",
        Environment.PRODUCTION,
        public_origin,
        "client-id",
        "client-secret",
        "subject-a,subject-b",
        "auth-secret",
        "backend-image",
        "frontend-image",
        "migrator-image",
    )


def test_write_to_file_writes_expected_environment_values(tmp_path):
    configuration = build_configuration("https://tracker.example.com:8443")
    environment_file = tmp_path / ".env"

    configuration.write_to_file(str(environment_file))

    assert environment_file.read_text(encoding="utf-8") == (
        'INSTANCE_NAME="tracker"\n'
        'INSTANCE_DIR="/srv/tracker"\n'
        'ENVIRONMENT="Production"\n'
        'PUBLIC_ORIGIN="https://tracker.example.com:8443"\n'
        'PUBLIC_HOST="tracker.example.com"\n'
        'PUBLIC_PORT="8443"\n'
        "\n"
        'BACKEND_IMAGE="backend-image"\n'
        'FRONTEND_IMAGE="frontend-image"\n'
        'MIGRATOR_IMAGE="migrator-image"\n'
        "\n"
        'GOOGLE_CLIENT_ID="client-id"\n'
        'GOOGLE_CLIENT_SECRET="client-secret"\n'
        'GOOGLE_ALLOWED_SUBJECTS="subject-a,subject-b"\n'
        "\n"
        'AUTH_SECRET="auth-secret"\n'
    )


@pytest.mark.parametrize(
    "origin", ["http://tracker.example.com", "https:///missing-host"]
)
def test_public_origin_must_be_an_https_origin_with_a_host(origin: str):
    configuration = build_configuration(origin)

    with pytest.raises(
        ValueError, match="PUBLIC_ORIGIN must be an HTTPS origin with a hostname"
    ):
        configuration.get_public_host()


def test_public_origin_defaults_to_https_port():
    assert build_configuration().get_public_port() == 443


def test_build_from_environment_reports_all_missing_required_values(monkeypatch):
    for variable in (
        "INSTANCE_NAME",
        "PUBLIC_ORIGIN",
        "GOOGLE_CLIENT_ID",
        "GOOGLE_CLIENT_SECRET",
        "GOOGLE_ALLOWED_SUBJECTS",
    ):
        monkeypatch.delenv(variable, raising=False)

    with pytest.raises(
        ValueError, match="INSTANCE_NAME, PUBLIC_ORIGIN, GOOGLE_CLIENT_ID"
    ):
        Configuration.build_from_environment(
            "/srv/tracker", "backend", "frontend", "migrator"
        )


def test_build_from_environment_uses_defaults_and_generates_auth_secret(monkeypatch):
    monkeypatch.setenv("INSTANCE_NAME", "tracker")
    monkeypatch.setenv("PUBLIC_ORIGIN", "https://tracker.example.com")
    monkeypatch.setenv("GOOGLE_CLIENT_ID", "client-id")
    monkeypatch.setenv("GOOGLE_CLIENT_SECRET", "client-secret")
    monkeypatch.setenv("GOOGLE_ALLOWED_SUBJECTS", "subject")
    monkeypatch.delenv("ENVIRONMENT", raising=False)
    monkeypatch.delenv("AUTH_SECRET", raising=False)

    configuration = Configuration.build_from_environment(
        "/srv/tracker", "backend", "frontend", "migrator"
    )

    assert configuration.environment is Environment.PRODUCTION
    assert configuration.auth_secret
