import subprocess
from argparse import Namespace

from orchestrator.commands import python_install
from orchestrator.core.context import Context
from orchestrator.core.paths import RepoPaths


class RecordingRunner:
    def __init__(self) -> None:
        self.commands: list[list[str]] = []

    def run(self, arguments, **kwargs):
        command = [str(argument) for argument in arguments]
        self.commands.append(command)
        if command[-3:] == ["-m", "pip", "--version"]:
            return subprocess.CompletedProcess(command, 1, "", "No module named pip")
        if command[-1:] == ["--version"]:
            return subprocess.CompletedProcess(command, 0, "Python 3.14.4\n", "")
        return subprocess.CompletedProcess(command, 0, "", "")


def test_install_repairs_existing_environment_without_pip(tmp_path, monkeypatch):
    (tmp_path / "config").mkdir()
    (tmp_path / "config" / "toolchain.toml").write_text(
        '[tools]\npython = "3.14"\n', encoding="utf-8"
    )
    (tmp_path / "pyproject.toml").write_text(
        '[dependency-groups]\ndev = ["ruff==0.15.1"]\n', encoding="utf-8"
    )
    quality_python = tmp_path / ".venv" / "bin" / "python"
    quality_python.parent.mkdir(parents=True)
    quality_python.touch()
    runner = RecordingRunner()
    context = Context(paths=RepoPaths(tmp_path), runner=runner)

    monkeypatch.setattr(
        python_install, "required_python", lambda _context: "/usr/bin/python3"
    )
    python_install.run(context, Namespace())

    assert runner.commands == [
        [str(quality_python), "--version"],
        [str(quality_python), "-m", "pip", "--version"],
        [str(quality_python), "-m", "ensurepip", "--upgrade"],
        [str(quality_python), "-m", "pip", "install", "ruff==0.15.1"],
        [str(quality_python), "-m", "pip", "install", "--editable", ".", "--no-deps"],
    ]
