"""Tests for the container image scan."""

from argparse import Namespace
from pathlib import Path
from subprocess import CompletedProcess
from types import SimpleNamespace

import pytest

from orchestrator.commands import security_images


class FakeToolchain:
    def require_image(self, name: str) -> str:
        return name


class RecordingRunner:
    def __init__(self, returncode: int = 0):
        self.returncode = returncode
        self.calls: list[tuple[list[str], dict[str, object]]] = []

    def run(self, command, **kwargs):
        self.calls.append((command, kwargs))
        return CompletedProcess(command, self.returncode, "findings", "scanner error")


@pytest.fixture(autouse=True)
def scan_environment(monkeypatch):
    monkeypatch.delenv("IMAGE_SCAN_MODE", raising=False)
    monkeypatch.delenv("GITHUB_STEP_SUMMARY", raising=False)
    monkeypatch.setattr(security_images, "find_tool", lambda *_: "trivy")
    monkeypatch.setattr(security_images.Toolchain, "read", lambda *_: FakeToolchain())


def make_context(runner: RecordingRunner) -> SimpleNamespace:
    return SimpleNamespace(
        root=Path("."),
        runner=runner,
        paths=SimpleNamespace(toolchain=Path("toolchain.toml")),
    )


def exit_code(command: list[str]) -> str:
    return command[command.index("--exit-code") + 1]


def test_scan_fails_on_findings_by_default():
    runner = RecordingRunner()

    security_images.run(make_context(runner), Namespace())

    assert [exit_code(command) for command, _ in runner.calls] == ["1", "1", "1"]
    assert all("check" not in kwargs for _, kwargs in runner.calls)


def test_report_mode_writes_findings_to_summary(monkeypatch, tmp_path: Path):
    summary_path = tmp_path / "summary.md"
    monkeypatch.setenv("IMAGE_SCAN_MODE", "report")
    monkeypatch.setenv("GITHUB_STEP_SUMMARY", str(summary_path))
    runner = RecordingRunner()

    security_images.run(make_context(runner), Namespace())

    assert [exit_code(command) for command, _ in runner.calls] == ["0", "0", "0"]
    summary = summary_path.read_text(encoding="utf-8")
    assert "### backend" in summary
    assert "### migrator" in summary
    assert "findings" in summary


def test_report_mode_survives_scanner_errors(monkeypatch, tmp_path: Path):
    summary_path = tmp_path / "summary.md"
    monkeypatch.setenv("IMAGE_SCAN_MODE", "report")
    monkeypatch.setenv("GITHUB_STEP_SUMMARY", str(summary_path))

    result = security_images.run(make_context(RecordingRunner(1)), Namespace())

    assert result == 0
    assert "scanner error" in summary_path.read_text(encoding="utf-8")


def test_unknown_scan_mode_is_rejected(monkeypatch):
    monkeypatch.setenv("IMAGE_SCAN_MODE", "warn")

    with pytest.raises(ValueError, match="Unsupported image scan mode"):
        security_images.run(make_context(RecordingRunner()), Namespace())
