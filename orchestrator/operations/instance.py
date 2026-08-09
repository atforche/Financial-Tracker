"""Validation shared by deployed-instance operations."""

from __future__ import annotations

import os
from pathlib import Path


def resolve_instance_path(value: str) -> Path:
    """Resolve an instance path and reject unsafe runner locations."""

    candidate = Path(value).expanduser()
    if not candidate.is_absolute():
        raise ValueError("INSTANCE_PATH must be an absolute path")
    path = candidate.resolve()
    if path == Path("/"):
        raise ValueError("The filesystem root cannot be used as an instance path")

    workspace_value = os.environ.get("GITHUB_WORKSPACE", "").strip()
    if workspace_value:
        workspace = Path(workspace_value).resolve()
        if path == workspace or workspace in path.parents:
            raise ValueError("INSTANCE_PATH must be outside the runner workspace")
    return path
