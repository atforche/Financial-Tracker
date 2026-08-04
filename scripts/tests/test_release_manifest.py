from __future__ import annotations

import json

import pytest
from shared.release_manifest import ReleaseManifest


def valid_manifest() -> dict[str, str]:
    digest = "a" * 64
    return {
        "commit": "abc123",
        "backendImage": f"ghcr.io/example/backend@sha256:{digest}",
        "frontendImage": f"ghcr.io/example/frontend@sha256:{digest}",
        "migratorImage": f"ghcr.io/example/migrator@sha256:{digest}",
    }


def test_read_returns_validated_manifest(tmp_path):
    manifest_path = tmp_path / "release-manifest.json"
    manifest_path.write_text(json.dumps(valid_manifest()), encoding="utf-8")

    manifest = ReleaseManifest.read(manifest_path)

    assert manifest.commit == "abc123"
    assert manifest.backend_image.startswith("ghcr.io/example/backend@sha256:")


@pytest.mark.parametrize(
    ("update", "message"),
    [
        ({"commit": ""}, "commit must not be empty"),
        ({"backendImage": "ghcr.io/example/backend:latest"}, "immutable sha256"),
        ({"unexpected": "value"}, "only commit and the three image references"),
    ],
)
def test_read_rejects_invalid_manifests(tmp_path, update: dict[str, str], message: str):
    manifest_path = tmp_path / "release-manifest.json"
    content = valid_manifest()
    content.update(update)
    manifest_path.write_text(json.dumps(content), encoding="utf-8")

    with pytest.raises(ValueError, match=message):
        ReleaseManifest.read(manifest_path)
