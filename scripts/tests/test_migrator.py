from pathlib import Path

from shared import migrator


def test_run_migrator_uses_the_hardened_container_contract(monkeypatch, tmp_path):
    captured: dict[str, object] = {}

    def fake_run(command: list[str], **kwargs):
        captured["command"] = command
        captured["kwargs"] = kwargs

    monkeypatch.setattr(migrator.subprocess, "run", fake_run)

    migrator.run_migrator("registry.example/migrator@sha256:abc", tmp_path)

    assert captured == {
        "command": [
            "docker",
            "run",
            "--rm",
            "--read-only",
            "--cap-drop",
            "ALL",
            "--security-opt",
            "no-new-privileges:true",
            "--volume",
            f"{Path(tmp_path).resolve()}:/data",
            "--env",
            "DATABASE_PATH=/data/database.db",
            "registry.example/migrator@sha256:abc",
        ],
        "kwargs": {"check": True},
    }
