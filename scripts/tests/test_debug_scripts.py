import sqlite3
from pathlib import Path
from types import SimpleNamespace

import debug_scripts


def test_debug_migrations_use_the_development_identity_from_debug_environment(
    monkeypatch,
):
    command = debug_scripts.ApplyDebugMigrations()
    environment = {
        "AUTH_MODE": "development",
        "DEVELOPMENT_AUTH_SUBJECT": "local-developer",
        "DEVELOPMENT_AUTH_EMAIL": "local-developer@example.test",
    }
    calls: list[tuple[str, dict[str, str]]] = []

    monkeypatch.setattr(debug_scripts, "get_debug_environment", lambda: environment)
    monkeypatch.setattr(
        debug_scripts,
        "get_debug_configuration",
        lambda: SimpleNamespace(
            get_database_file_path=lambda: "/tmp/debug/data/database.db"
        ),
    )
    monkeypatch.setattr(debug_scripts.os, "chdir", lambda _directory: None)
    monkeypatch.setattr(
        command,
        "run_subprocess",
        lambda command_line, env: calls.append((command_line, env)),
    )

    command.apply_migrations()

    assert calls == [
        (
            "dotnet run --project backend/Migrator/Migrator.csproj",
            {
                "AUTH_MODE": "development",
                "DEVELOPMENT_AUTH_SUBJECT": "local-developer",
                "DEVELOPMENT_AUTH_EMAIL": "local-developer@example.test",
                "DATABASE_PATH": "/tmp/debug/data/database.db",
            },
        )
    ]


def test_restore_debug_database_stages_migrates_and_replaces_native_database(
    monkeypatch, tmp_path
):
    debug_directory = tmp_path / "debug"
    data_directory = debug_directory / "data"
    data_directory.mkdir(parents=True)
    environment_file = debug_directory / ".env"
    environment_file.touch()
    repository = tmp_path / "restic-repository"
    repository.mkdir()
    (repository / "config").touch()
    database_path = data_directory / "database.db"
    database_path.write_text("previous debug database", encoding="utf-8")

    configuration = SimpleNamespace(
        path=str(debug_directory),
        get_environment_file_path=lambda: str(environment_file),
    )
    restic_calls: list[dict[str, object]] = []
    migration_calls: list[tuple[object, dict[str, str]]] = []

    def fake_run_restic(arguments, **kwargs):
        restic_calls.append({"arguments": arguments, **kwargs})
        if arguments[0] != "restore":
            return
        restore_directory = kwargs["volumes"][0][0]
        restored_database = restore_directory / "snapshot" / "database.db"
        restored_database.parent.mkdir()
        with sqlite3.connect(restored_database) as connection:
            connection.execute(
                "CREATE TABLE __EFMigrationsHistory (MigrationId TEXT NOT NULL)"
            )

    def fake_apply_migrations(_command, database, environment):
        migration_calls.append((database, environment.copy()))

    monkeypatch.setattr(
        debug_scripts,
        "get_debug_configuration",
        lambda: configuration,
    )
    monkeypatch.setattr(
        debug_scripts,
        "get_debug_environment",
        lambda: {
            "AUTH_MODE": "development",
            "DEVELOPMENT_AUTH_SUBJECT": "local-developer",
        },
    )
    monkeypatch.setattr(debug_scripts, "run_restic", fake_run_restic)
    monkeypatch.setattr(
        debug_scripts.ApplyDebugMigrations,
        "apply_migrations_to",
        fake_apply_migrations,
    )
    monkeypatch.setattr(debug_scripts.getpass, "getpass", lambda _: "restic-secret")

    command = debug_scripts.RestoreDebugDatabase()
    command.repository = str(repository)
    command.validate_arguments()
    command.restore()

    assert [call["arguments"][0] for call in restic_calls] == ["check", "restore"]
    assert restic_calls[0]["repository"] == repository.resolve()
    assert restic_calls[0]["password"] == "restic-secret"
    assert "restic-secret" not in restic_calls[1]["arguments"]
    assert migration_calls[0][1]["AUTH_MODE"] == "development"
    assert migration_calls[0][1]["DATABASE_PATH"] == str(migration_calls[0][0])
    assert database_path.is_file()
    assert list(data_directory.glob("database.db.before-restore-*.bak"))
    assert database_path.read_bytes() != b"previous debug database"


def test_s3_restore_downloads_with_aws_login_and_selected_profile(
    monkeypatch, tmp_path
):
    destination = tmp_path / "repository"
    destination.mkdir()
    calls: list[tuple[list[str], dict[str, object]]] = []
    identity_checks = 0

    def fake_run(command, **kwargs):
        nonlocal identity_checks
        calls.append((command, kwargs))
        if command[1:3] == ["sts", "get-caller-identity"]:
            identity_checks += 1
            return debug_scripts.subprocess.CompletedProcess(
                command, 0 if identity_checks == 2 else 1, stdout="", stderr=""
            )
        if command[1:3] == ["s3", "sync"]:
            Path(command[4], "config").touch()
        return debug_scripts.subprocess.CompletedProcess(
            command, 0, stdout="", stderr=""
        )

    monkeypatch.setattr(debug_scripts.shutil, "which", lambda _: "/usr/bin/aws")
    monkeypatch.setattr(debug_scripts.subprocess, "run", fake_run)

    command = debug_scripts.RestoreDebugDatabase()
    command.s3_uri = "s3://backup-bucket/financial-tracker"
    command.aws_profile = "production-backups"
    command.download_s3_repository(destination)

    command_lines = [call[0] for call in calls]
    assert ["aws", "login", "--profile", "production-backups"] in command_lines
    assert [
        "aws",
        "s3",
        "sync",
        "s3://backup-bucket/financial-tracker",
        str(destination),
        "--profile",
        "production-backups",
    ] in command_lines
    assert (destination / "config").is_file()
