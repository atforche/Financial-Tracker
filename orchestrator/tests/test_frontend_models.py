from argparse import Namespace
from pathlib import Path
from types import SimpleNamespace

from orchestrator.commands import frontend_models
from orchestrator.core.context import Context
from orchestrator.core.paths import RepoPaths


def test_frontend_models_reads_openapi_contract_from_backend_artifacts(
    tmp_path: Path,
):
    captured: dict[str, object] = {}

    class Runner:
        def run(self, arguments, **kwargs):
            captured["arguments"] = arguments
            captured["kwargs"] = kwargs
            return SimpleNamespace(returncode=0)

    frontend_models.run(
        Context(paths=RepoPaths(tmp_path), runner=Runner()), Namespace(verify=True)
    )

    assert captured["arguments"] == [
        "npm",
        "exec",
        "--",
        "openapi-typescript",
        str(
            tmp_path
            / "backend"
            / ".artifacts"
            / "obj"
            / "Rest"
            / "Financial-Tracker-API.json"
        ),
        "--output",
        "framework/data/api.ts",
        "--enum",
        "--check",
    ]
    assert captured["kwargs"] == {"cwd": tmp_path / "frontend"}
