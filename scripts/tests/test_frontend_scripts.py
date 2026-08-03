from pathlib import Path

import frontend_scripts


def test_frontend_subprocess_uses_frontend_directory_and_restores_cwd(
    monkeypatch, tmp_path
):
    command = frontend_scripts.FrontendCommand("test", "test command")
    working_directories: list[Path] = []
    monkeypatch.chdir(tmp_path)
    monkeypatch.setattr(
        command,
        "run_subprocess",
        lambda _: working_directories.append(Path.cwd()),
    )

    command.run_frontend_subprocess("npm ci")

    assert working_directories == [frontend_scripts.FRONTEND_DIRECTORY]
    assert Path.cwd() == tmp_path


def test_model_commands_share_openapi_command_construction(monkeypatch):
    refresh_command = frontend_scripts.RefreshFrontendModels()
    verify_command = frontend_scripts.VerifyFrontendModels()
    commands: list[str] = []
    monkeypatch.setattr(refresh_command, "run_frontend_subprocess", commands.append)
    monkeypatch.setattr(verify_command, "run_frontend_subprocess", commands.append)

    refresh_command.refresh_models()
    verify_command.verify_models()

    assert commands == [
        frontend_scripts.OPENAPI_TYPESCRIPT_COMMAND.format(check=""),
        frontend_scripts.OPENAPI_TYPESCRIPT_COMMAND.format(check=" --check"),
    ]
