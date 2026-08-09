import subprocess
from typing import Annotated

import pytest
from shared.command import Command


class DatabaseCommand(Command):
    """Command with an optional database test flag."""

    use_database: Annotated[bool, "If provided, runs tests against an actual database"]


class OptionalValueCommand(Command):
    """Command with an optional string argument."""

    value: Annotated[str | None, "Optional value"]


def test_run_subprocess_splits_command_and_returns_exit_code(monkeypatch):
    command = Command("test", "test command")
    captured: dict[str, object] = {}

    def fake_run(*arguments, **kwargs):
        captured["arguments"] = arguments
        captured["kwargs"] = kwargs
        return subprocess.CompletedProcess(arguments[0], 0)

    monkeypatch.setattr("shared.command.subprocess.run", fake_run)

    assert (
        command.run_subprocess('tool --value "contains spaces"', suppress_output=True)
        == 0
    )
    assert captured["arguments"] == (["tool", "--value", "contains spaces"],)
    assert captured["kwargs"] == {
        "text": True,
        "input": "",
        "check": False,
        "stdout": subprocess.DEVNULL,
        "stderr": subprocess.DEVNULL,
        "env": None,
    }


def test_run_subprocess_raises_for_unsuccessful_command(monkeypatch):
    command = Command("test", "test command")
    monkeypatch.setattr(
        "shared.command.subprocess.run",
        lambda *arguments, **kwargs: subprocess.CompletedProcess(arguments[0], 1),
    )

    with pytest.raises(RuntimeError, match="Command ended with error"):
        command.run_subprocess("tool")


def test_command_rejects_unexpected_arguments():
    command = Command("test", "test command")

    with pytest.raises(ValueError, match="Unexpected arguments provided"):
        command.run(["--unexpected"])


def test_optional_boolean_command_argument_defaults_to_false():
    command = DatabaseCommand("test", "test command")

    command.run([])

    assert command.use_database is False


def test_optional_string_command_argument_defaults_to_none_and_parses_value():
    command = OptionalValueCommand("test", "test command")
    command.run([])
    assert command.value is None

    command = OptionalValueCommand("test", "test command")
    command.run(["--value", "provided"])
    assert command.value == "provided"
