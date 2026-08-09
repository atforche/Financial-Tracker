from __future__ import annotations

import io
import json
from pathlib import Path
from types import SimpleNamespace
from urllib.request import Request

import pytest

from orchestrator.operations import container_smoke, migrator
from orchestrator.operations.backup import BackupOperations
from orchestrator.operations.configuration import Configuration, Environment
from orchestrator.operations.deployment import apply_migrations, validate_deploy_request
from orchestrator.operations.instance import resolve_instance_path
from orchestrator.operations.release_manifest import ReleaseManifest


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
        "auth-secret",
        "backend-image",
        "frontend-image",
        "migrator-image",
    )


def test_configuration_writes_expected_environment_values(tmp_path: Path):
    environment_file = tmp_path / ".env"
    build_configuration("https://tracker.example.com:8443").write_to_file(
        str(environment_file)
    )

    assert environment_file.read_text(encoding="utf-8") == (
        'INSTANCE_NAME="tracker"\n'
        'INSTANCE_DIR="/srv/tracker"\n'
        'ENVIRONMENT="Production"\n'
        'PUBLIC_ORIGIN="https://tracker.example.com:8443"\n'
        'PUBLIC_HOST="tracker.example.com"\n'
        'PUBLIC_PORT="8443"\n'
        'BACKEND_IMAGE="backend-image"\n'
        'FRONTEND_IMAGE="frontend-image"\n'
        'MIGRATOR_IMAGE="migrator-image"\n'
        'CADDY_IMAGE="caddy:2.11.4-alpine@sha256:5f5c8640aae01df9654968d946d8f1a56c497f1dd5c5cda4cf95ab7c14d58648"\n'
        'GOOGLE_CLIENT_ID="client-id"\n'
        'GOOGLE_CLIENT_SECRET="client-secret"\n'
        'AUTH_SECRET="auth-secret"\n'
        'AUTH_MODE="google"\n'
    )


@pytest.mark.parametrize(
    "origin", ["http://tracker.example.com", "https:///missing-host"]
)
def test_configuration_requires_https_public_origin(origin: str):
    with pytest.raises(ValueError, match="PUBLIC_ORIGIN must be an HTTPS origin"):
        build_configuration(origin).get_public_host()


def test_configuration_builds_bootstrap_values_from_environment(monkeypatch):
    monkeypatch.setenv("INSTANCE_NAME", "tracker")
    monkeypatch.setenv("PUBLIC_ORIGIN", "https://tracker.example.com")
    monkeypatch.setenv("GOOGLE_CLIENT_ID", "client-id")
    monkeypatch.setenv("GOOGLE_CLIENT_SECRET", "client-secret")
    monkeypatch.delenv("ENVIRONMENT", raising=False)
    monkeypatch.delenv("AUTH_SECRET", raising=False)
    monkeypatch.setattr(
        "orchestrator.operations.configuration.secrets.token_urlsafe",
        lambda _length: "generated-auth-secret",
    )

    configuration = Configuration.build_from_environment(
        "/srv/tracker", "backend", "frontend", "migrator"
    )

    assert configuration.environment is Environment.PRODUCTION
    assert configuration.auth_secret == "generated-auth-secret"


def test_migrator_uses_hardened_container_contract(tmp_path: Path):
    captured: dict[str, object] = {}

    class RecordingRunner:
        def run(self, command: list[str], **kwargs):
            captured["command"] = command
            captured["kwargs"] = kwargs

    migrator_runner = RecordingRunner()

    migrator.run_migrator(
        "registry.example/migrator@sha256:abc", tmp_path, runner=migrator_runner
    )

    assert captured["command"] == [
        "docker",
        "run",
        "--rm",
        "--read-only",
        "--tmpfs",
        "/tmp",
        "--cap-drop",
        "ALL",
        "--security-opt",
        "no-new-privileges:true",
        "--volume",
        f"{tmp_path.resolve()}:/data",
        "--env",
        "DATABASE_PATH=/data/database.db",
        "registry.example/migrator@sha256:abc",
    ]
    assert captured["kwargs"] == {}


def test_container_smoke_database_provisions_users(monkeypatch, tmp_path: Path):
    calls: list[tuple[str, Path, dict[str, str]]] = []

    def fake_run_migrator(image, data_directory, environment, **_kwargs):
        calls.append((image, data_directory, environment))

    monkeypatch.setattr(container_smoke, "run_migrator", fake_run_migrator)
    container_smoke.prepare_smoke_test_database(tmp_path)

    assert len(calls) == 3
    assert calls[0][1] == tmp_path
    assert calls[0][2]["DEVELOPMENT_AUTH_SUBJECT"] == "container-smoke-test"
    assert calls[2][2]["DEVELOPMENT_AUTH_ROLE"] == "Standard"


def test_release_manifest_rejects_mutable_images(tmp_path: Path):
    manifest_path = tmp_path / "release-manifest.json"
    digest = "a" * 64
    manifest_path.write_text(
        json.dumps(
            {
                "commit": "abc123",
                "backendImage": f"ghcr.io/example/backend@sha256:{digest}",
                "frontendImage": "ghcr.io/example/frontend:latest",
                "migratorImage": f"ghcr.io/example/migrator@sha256:{digest}",
            }
        ),
        encoding="utf-8",
    )

    with pytest.raises(ValueError, match="immutable sha256"):
        ReleaseManifest.read(manifest_path)


def test_apply_migrations_passes_bootstrap_email(monkeypatch, tmp_path: Path):
    instance_path = tmp_path / "instance"
    database_path = instance_path / "data" / "database.db"
    database_path.parent.mkdir(parents=True)
    database_path.touch()
    captured: dict[str, object] = {}

    def fake_run_migrator(image, data_directory, environment):
        captured["image"] = image
        captured["data_directory"] = data_directory
        captured["environment"] = environment

    monkeypatch.setattr(
        "orchestrator.operations.deployment.run_migrator", fake_run_migrator
    )
    configuration = SimpleNamespace(
        path=str(instance_path),
        migrator_image="registry.example/migrator@sha256:abc",
        get_database_file_path=lambda: str(database_path),
    )

    apply_migrations(configuration, "owner@example.com")

    assert captured["image"] == "registry.example/migrator@sha256:abc"
    assert captured["environment"] == {"BOOTSTRAP_ADMIN_EMAIL": "owner@example.com"}


def test_deploy_validation_accepts_missing_instance_path(tmp_path: Path):
    release_directory = tmp_path / "release"
    release_directory.mkdir()
    manifest_path = release_directory / "release-manifest.json"
    manifest_path.touch()
    (release_directory / "compose.yaml").touch()
    (release_directory / "Caddyfile").touch()

    validate_deploy_request(str(tmp_path / "instance"), str(manifest_path))


def test_instance_path_validation_rejects_the_runner_workspace(
    monkeypatch, tmp_path: Path
):
    monkeypatch.setenv("GITHUB_WORKSPACE", str(tmp_path))

    with pytest.raises(ValueError, match="outside the runner workspace"):
        resolve_instance_path(str(tmp_path / "instance"))


def test_backup_restoration_uses_hardened_runtime_and_persists_data(
    monkeypatch, tmp_path: Path
):
    commands: list[list[str]] = []
    requests: list[Request] = []

    class RecordingRunner:
        def run(self, command: list[str], **_kwargs):
            commands.append(command)
            return SimpleNamespace(stdout="127.0.0.1:49152\n", returncode=0)

    def fake_urlopen(request, timeout: int):
        assert timeout in {3, 30}
        if isinstance(request, Request):
            requests.append(request)
            if request.full_url.endswith("/users/me"):
                return io.BytesIO(b'{"email":"backup-restore-smoke-test@example.test"}')
            if request.get_method() == "POST":
                body = json.loads(request.data)
                return io.BytesIO(json.dumps({"id": "account-id", **body}).encode())
            if request.full_url.endswith("/accounts"):
                return io.BytesIO(
                    b'[{"id":"account-id","name":"Restored backup smoke account"}]'
                )
            return io.BytesIO(
                b'{"id":"account-id","name":"Restored backup smoke account"}'
            )
        return io.BytesIO(b"")

    monkeypatch.setattr("orchestrator.operations.backup.urlopen", fake_urlopen)
    monkeypatch.setattr(BackupOperations, "get_published_port", lambda *_args: 49152)
    monkeypatch.setattr(BackupOperations, "wait_for_url", lambda *_args: None)
    operations = BackupOperations.__new__(BackupOperations)
    operations.runner = RecordingRunner()

    operations.verify_restored_backend(
        SimpleNamespace(backend_image="backend-image"), tmp_path
    )

    assert "--read-only" in commands[0]
    assert "--cap-drop" in commands[0]
    assert "no-new-privileges:true" in commands[0]
    assert commands[0][-1] == "backend-image"
    assert commands[-1][:4] == ["docker", "container", "rm", "--force"]
    assert [request.get_method() for request in requests] == ["GET", "GET", "POST"]
