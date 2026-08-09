import io
import json
from pathlib import Path
from types import SimpleNamespace
from urllib.request import Request

import backup_scripts


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
