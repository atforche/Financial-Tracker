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
