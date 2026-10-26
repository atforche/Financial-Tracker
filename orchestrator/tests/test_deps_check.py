import json
from argparse import Namespace
from subprocess import CompletedProcess

import pytest

from orchestrator.commands import deps_check
from orchestrator.core.context import Context
from orchestrator.core.paths import RepoPaths


class DotnetRunner:
    def __init__(self, version: str | None) -> None:
        self.version = version

    def run(self, arguments, **kwargs):
        if self.version is None:
            return CompletedProcess(arguments, 1, stdout="", stderr="SDK not found")
        return CompletedProcess(arguments, 0, stdout=f"{self.version}\n", stderr="")


def write_pins(
    root,
    *,
    toolchain: str = "10.0.403",
    global_json: str = "10.0.403",
    roll_forward: str = "disable",
    image_tag: str = "10.0.403",
) -> None:
    (root / "config").mkdir()
    (root / "config" / "toolchain.toml").write_text(
        f'[tools]\ndotnet = "{toolchain}"\n\n'
        f'[base_images]\ndotnet_sdk = "mcr.microsoft.com/dotnet/sdk:{image_tag}@sha256:abc"\n',
        encoding="utf-8",
    )
    (root / "global.json").write_text(
        json.dumps({"sdk": {"version": global_json, "rollForward": roll_forward}}),
        encoding="utf-8",
    )


def make_context(root, monkeypatch, active: str | None = "10.0.403") -> Context:
    monkeypatch.setattr(deps_check, "find_tool", lambda _context, _name: "dotnet")
    return Context(paths=RepoPaths(root), runner=DotnetRunner(active))


def test_check_dotnet_accepts_matching_pins_and_active_sdk(tmp_path, monkeypatch):
    write_pins(tmp_path)

    assert (
        deps_check.check_dotnet(make_context(tmp_path, monkeypatch), Namespace()) == 0
    )


@pytest.mark.parametrize(
    ("pins", "active", "failure"),
    [
        ({"global_json": "10.0.111"}, "10.0.403", "global.json"),
        ({"roll_forward": "latestFeature"}, "10.0.403", "global.json rollForward"),
        ({"image_tag": "10.0"}, "10.0.403", "dotnet_sdk image"),
        ({}, "10.0.402", "dotnet"),
        ({}, None, "dotnet"),
    ],
)
def test_check_dotnet_rejects_any_mismatch(
    tmp_path, monkeypatch, pins, active, failure
):
    write_pins(tmp_path, **pins)

    with pytest.raises(RuntimeError, match=f": {failure}$"):
        deps_check.check_dotnet(
            make_context(tmp_path, monkeypatch, active), Namespace()
        )


def test_check_dotnet_pins_ignores_the_active_sdk(tmp_path, monkeypatch):
    write_pins(tmp_path)

    context = make_context(tmp_path, monkeypatch, active=None)

    assert deps_check.check_dotnet_pins(context, Namespace()) == 0
