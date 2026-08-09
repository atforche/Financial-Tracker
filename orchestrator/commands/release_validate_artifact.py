"""Validate a downloaded release manifest."""

from argparse import Namespace
from pathlib import Path

from ..core.context import Context
from ..operations.release_manifest import ReleaseManifest


def run(_context: Context, args: Namespace) -> int:
    manifest = ReleaseManifest.read(Path(args.manifest))
    if manifest.commit != args.commit:
        raise RuntimeError(
            "Release artifact commit does not match the requested commit"
        )
    return 0
