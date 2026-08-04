#!/usr/bin/env python3
"""Validates release workflow runs and artifacts before deployment."""

import json
import os
from pathlib import Path
from typing import Annotated
from urllib.request import Request, urlopen

from shared.command import Command
from shared.command_collection import CommandCollection
from shared.release_manifest import ReleaseManifest
from shared.step import Step


def main() -> None:
    """Builds and runs the release command collection."""

    commands = CommandCollection(
        "Validates release inputs used by deployment workflows"
    )
    commands.commands.append(ValidateReleaseRun())
    commands.commands.append(ValidateReleaseArtifact())
    commands.run()


class ValidateReleaseRun(Command):
    """Validates that a GitHub Actions run produced the requested release."""

    commit: Annotated[str, "Expected release commit SHA"]
    run_id: Annotated[int, "GitHub Actions release workflow run ID"]

    def __init__(self) -> None:
        """Constructs a release-run validation command."""

        super().__init__(
            "validate-run", "Validates a successful main release workflow run"
        )
        self.steps.append(
            Step(
                "Validate Release Workflow Run",
                "Release workflow run validated",
                self.validate_run,
            )
        )

    def validate_run(self) -> None:
        """Loads the workflow run from GitHub and validates its release identity."""

        github_api_url = os.environ["GITHUB_API_URL"]
        github_repository = os.environ["GITHUB_REPOSITORY"]
        github_token = os.environ["GITHUB_TOKEN"]
        url = f"{github_api_url}/repos/{github_repository}/actions/runs/{self.run_id}"
        request = Request(
            url,
            headers={
                "Accept": "application/vnd.github+json",
                "Authorization": f"Bearer {github_token}",
                "X-GitHub-Api-Version": "2022-11-28",
            },
        )
        with urlopen(request, timeout=30) as response:
            workflow_run = json.load(response)

        expected_values = {
            "conclusion": "success",
            "event": "push",
            "head_branch": "main",
            "head_sha": self.commit,
            "path": ".github/workflows/release.yml",
        }
        mismatches = [
            name
            for name, expected in expected_values.items()
            if workflow_run.get(name) != expected
        ]
        if mismatches:
            raise RuntimeError(
                "Selected run is not the requested successful main release: "
                + ", ".join(mismatches)
            )


class ValidateReleaseArtifact(Command):
    """Validates that a release artifact belongs to the requested commit."""

    commit: Annotated[str, "Expected release commit SHA"]
    manifest: Annotated[str, "Path to the downloaded release manifest"]

    def __init__(self) -> None:
        """Constructs a release-artifact validation command."""

        super().__init__("validate-artifact", "Validates a downloaded release artifact")
        self.steps.append(
            Step(
                "Validate Release Artifact",
                "Release artifact validated",
                self.validate_artifact,
            )
        )

    def validate_artifact(self) -> None:
        """Validates the release manifest and expected commit."""

        release_manifest = ReleaseManifest.read(Path(self.manifest))
        if release_manifest.commit != self.commit:
            raise RuntimeError(
                "Release artifact commit does not match the requested commit"
            )


if __name__ == "__main__":
    main()
