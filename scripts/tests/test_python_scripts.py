from __future__ import annotations

import pytest
import python_scripts


def test_get_quality_python_returns_repository_virtual_environment(
    monkeypatch, tmp_path
):
    quality_python = tmp_path / "bin" / "python"
    quality_python.parent.mkdir()
    quality_python.touch()
    monkeypatch.setattr(python_scripts, "QUALITY_PYTHON", quality_python)

    assert python_scripts.get_quality_python() == quality_python


def test_get_quality_python_requires_installed_virtual_environment(
    monkeypatch, tmp_path
):
    monkeypatch.setattr(python_scripts, "QUALITY_PYTHON", tmp_path / "missing-python")

    with pytest.raises(RuntimeError, match="Python quality tools are not installed"):
        python_scripts.get_quality_python()


def test_ensure_quality_pip_bootstraps_missing_pip(monkeypatch, tmp_path):
    quality_python = tmp_path / "bin" / "python"
    quality_python.parent.mkdir()
    quality_python.touch()
    commands: list[list[str]] = []

    def fake_run(command: list[str], **kwargs):
        commands.append(command)
        returncode = 1 if command[2:4] == ["pip", "--version"] else 0
        return type("Result", (), {"returncode": returncode})()

    monkeypatch.setattr(python_scripts, "QUALITY_PYTHON", quality_python)
    monkeypatch.setattr(python_scripts.subprocess, "run", fake_run)

    python_scripts.ensure_quality_pip()

    assert commands == [
        [str(quality_python), "-m", "pip", "--version"],
        [str(quality_python), "-m", "ensurepip", "--upgrade"],
    ]


def test_install_tools_creates_virtual_environment_with_python_314(
    monkeypatch, tmp_path
):
    virtual_environment = tmp_path / ".venv"
    quality_python = virtual_environment / "bin" / "python"
    commands: list[list[str]] = []

    def fake_run(command: list[str], **kwargs):
        commands.append(command)

    monkeypatch.setattr(python_scripts, "VIRTUAL_ENVIRONMENT_PATH", virtual_environment)
    monkeypatch.setattr(python_scripts, "QUALITY_PYTHON", quality_python)
    monkeypatch.setattr(
        python_scripts, "get_required_python", lambda: "/usr/bin/python3.14"
    )
    monkeypatch.setattr(python_scripts, "ensure_quality_pip", lambda: None)
    monkeypatch.setattr(python_scripts.subprocess, "run", fake_run)

    python_scripts.InstallCommand().install_tools()

    assert commands == [
        ["/usr/bin/python3.14", "-m", "venv", str(virtual_environment)],
        [
            str(quality_python),
            "-m",
            "pip",
            "install",
            "--requirement",
            "requirements-dev.txt",
        ],
    ]
