"""Release manifest model and validation."""

import json
import re
from dataclasses import dataclass
from pathlib import Path

IMAGE_DIGEST_PATTERN = re.compile(r"^.+@sha256:[0-9a-f]{64}$")


@dataclass(frozen=True)
class ReleaseManifest:
    """Immutable container image references belonging to one release."""

    commit: str
    backend_image: str
    frontend_image: str
    migrator_image: str

    @classmethod
    def read(cls, path: Path) -> ReleaseManifest:
        """Reads and validates a release manifest."""

        with path.open("r", encoding="utf-8") as file:
            value = json.load(file)

        if not isinstance(value, dict):
            raise ValueError("Release manifest must contain a JSON object")

        expected_fields = {"commit", "backendImage", "frontendImage", "migratorImage"}
        if set(value) != expected_fields:
            raise ValueError(
                "Release manifest must contain only commit and the three image references"
            )

        commit = value["commit"]
        image_references = (
            value["backendImage"],
            value["frontendImage"],
            value["migratorImage"],
        )
        if not isinstance(commit, str) or commit.strip() == "":
            raise ValueError("Release manifest commit must not be empty")
        if not all(
            isinstance(image, str) and IMAGE_DIGEST_PATTERN.fullmatch(image)
            for image in image_references
        ):
            raise ValueError(
                "Release manifest images must be immutable sha256 digest references"
            )

        return cls(commit, *image_references)
