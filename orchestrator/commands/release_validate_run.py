"""Validate a successful release workflow run."""

from __future__ import annotations

import json
import os
from argparse import Namespace
from urllib.request import Request, urlopen

from ..core.context import Context


def run(_context: Context, args: Namespace) -> int:
    api_url = os.environ.get("GITHUB_API_URL", "https://api.github.com")
    repository = os.environ.get("GITHUB_REPOSITORY", "")
    token = os.environ.get("GITHUB_TOKEN", "")
    if not repository or not token:
        raise ValueError("GITHUB_REPOSITORY and GITHUB_TOKEN must be configured")
    request = Request(
        f"{api_url}/repos/{repository}/actions/runs/{args.run_id}",
        headers={
            "Accept": "application/vnd.github+json",
            "Authorization": f"Bearer {token}",
            "X-GitHub-Api-Version": "2022-11-28",
        },
    )
    with urlopen(request, timeout=30) as response:
        workflow_run = json.load(response)
    expected = {
        "conclusion": "success",
        "event": "push",
        "head_branch": "main",
        "head_sha": args.commit,
        "path": ".github/workflows/release.yml",
    }
    mismatches = [
        name for name, value in expected.items() if workflow_run.get(name) != value
    ]
    if mismatches:
        raise RuntimeError(
            "Selected run is not the requested successful main release: "
            + ", ".join(mismatches)
        )
    return 0
