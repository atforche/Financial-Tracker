import subprocess

from orchestrator.core.runner import Runner


def test_runner_merges_environment_overrides(monkeypatch, tmp_path):
    captured: dict[str, object] = {}

    def fake_run(*args, **kwargs):
        captured["args"] = args
        captured["kwargs"] = kwargs
        return subprocess.CompletedProcess(args[0], 0, "", "")

    monkeypatch.setattr("orchestrator.core.runner.subprocess.run", fake_run)
    Runner(verbose=False).run(
        ["tool", "--value", "x"], cwd=tmp_path, env={"FT_TEST": "yes"}
    )

    assert captured["args"] == (["tool", "--value", "x"],)
    assert captured["kwargs"]["cwd"] == tmp_path  # type: ignore[index]
    assert captured["kwargs"]["env"]["FT_TEST"] == "yes"  # type: ignore[index]
