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
