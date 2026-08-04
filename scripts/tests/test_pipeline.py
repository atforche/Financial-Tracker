from __future__ import annotations

from pathlib import Path

import pipeline_script

PROJECT_ROOT = Path(__file__).resolve().parents[2]


def test_run_command_contains_each_leaf_pipeline_command():
    expected_commands = (
        *pipeline_script.PYTHON_COMMANDS,
        *pipeline_script.BACKEND_FORMAT_COMMANDS,
        *pipeline_script.FRONTEND_FORMAT_COMMANDS,
        *pipeline_script.BACKEND_TEST_COMMANDS,
        *pipeline_script.FRONTEND_BUILD_COMMANDS,
        *pipeline_script.API_CONTRACT_COMMANDS,
        *pipeline_script.DEPENDENCY_COMMANDS,
        *pipeline_script.CONTAINER_IMAGE_COMMANDS,
    )

    assert expected_commands == pipeline_script.RUN_COMMANDS


def test_ci_workflows_delegate_check_sequences_to_pipeline_script():
    workflow_commands = {
        "code-quality.yml": ("python", "backend-format", "frontend-format"),
        "verification.yml": (
            "backend-test",
            "frontend-build",
            "api-contract",
            "dependencies",
            "container-images",
        ),
        "release.yml": ("container-images",),
    }
    direct_script_names = (
        "backend_scripts.py",
        "container_scripts.py",
        "frontend_scripts.py",
        "python_scripts.py",
        "security_scripts.py",
    )

    for workflow_name, commands in workflow_commands.items():
        workflow = (PROJECT_ROOT / ".github" / "workflows" / workflow_name).read_text(
            encoding="utf-8"
        )
        for command in commands:
            assert f"pipeline_script.py {command}" in workflow
        for script_name in direct_script_names:
            assert script_name not in workflow
