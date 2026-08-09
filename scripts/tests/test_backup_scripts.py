import io
import json
from pathlib import Path
from types import SimpleNamespace
from urllib.request import Request

import backup_scripts
from shared import restic


def test_restored_backend_verification_uses_hardened_runtime_and_persists_data(
    monkeypatch, tmp_path
):
    commands: list[list[str]] = []
    requests: list[Request] = []

    def fake_run(command: list[str], **_kwargs):
        commands.append(command)
        return SimpleNamespace(stdout="127.0.0.1:49152\n")

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
                b'{"id":"account-id","name":"Restored backup smoke account","financialInstitution":"smoke-test"}'
            )
        return io.BytesIO(b"")

    monkeypatch.setattr(backup_scripts.subprocess, "run", fake_run)
    monkeypatch.setattr(backup_scripts, "urlopen", fake_urlopen)
    monkeypatch.setattr(
        backup_scripts.BackupCommand, "get_published_port", lambda _: 49152
    )
    monkeypatch.setattr(backup_scripts.BackupCommand, "wait_for_url", lambda _: None)

    backup_scripts.BackupCommand.verify_restored_backend(
        SimpleNamespace(backend_image="backend-image"), Path(tmp_path)
    )

    run_command = commands[0]
    assert "--read-only" in run_command
    assert "--cap-drop" in run_command
    assert "no-new-privileges:true" in run_command
    assert run_command[-1] == "backend-image"
    assert commands[-1][:4] == ["docker", "container", "rm", "--force"]
    assert [request.get_method() for request in requests] == ["GET", "GET", "POST"]
    assert requests[0].get_header("Authorization") == (
        "Bearer development:backup-restore-smoke-test"
    )


def test_restic_password_is_forwarded_in_environment_not_command_arguments(
    monkeypatch, tmp_path
):
    repository = tmp_path / "repository"
    repository.mkdir()
    captured: dict[str, object] = {}

    def fake_run(command, **kwargs):
        captured["command"] = command
        captured["environment"] = kwargs["env"]

    monkeypatch.setattr(restic.subprocess, "run", fake_run)

    restic.run_restic(
        ["check"],
        repository=repository,
        password="restic-secret",
    )

    assert "restic-secret" not in captured["command"]
    assert captured["environment"]["RESTIC_PASSWORD"] == "restic-secret"


def test_local_restic_operations_can_omit_aws_environment_variables(
    monkeypatch, tmp_path
):
    repository = tmp_path / "repository"
    repository.mkdir()
    captured: dict[str, object] = {}
    monkeypatch.setenv("AWS_ACCESS_KEY_ID", "should-not-be-forwarded")

    def fake_run(command, **kwargs):
        captured["command"] = command
        captured["environment"] = kwargs["env"]

    monkeypatch.setattr(restic.subprocess, "run", fake_run)
    restic.run_restic(
        ["check"],
        repository=repository,
        password="restic-secret",
        pass_aws_credentials=False,
    )

    assert "AWS_ACCESS_KEY_ID" not in captured["command"]
