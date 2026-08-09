from argparse import Namespace

from orchestrator.commands import deps_install
from orchestrator.core.context import Context
from orchestrator.core.paths import RepoPaths


class RecordingRunner:
    def __init__(self) -> None:
        self.commands: list[list[str]] = []

    def run(self, arguments, **kwargs):
        command = [str(argument) for argument in arguments]
        self.commands.append(command)
        return None


def test_install_does_not_require_docker_for_repository_dependencies(
    tmp_path, monkeypatch
):
    runner = RecordingRunner()
    context = Context(paths=RepoPaths(tmp_path), runner=runner)
    monkeypatch.setattr(deps_install.python_install, "run", lambda *_args: None)
    monkeypatch.setattr(deps_install.frontend_install, "run", lambda *_args: None)
    monkeypatch.setattr(deps_install.trivy_install, "run", lambda *_args: None)
    monkeypatch.setattr(deps_install.backend_restore, "run", lambda *_args: None)

    deps_install.run(context, Namespace())

    assert runner.commands == []
