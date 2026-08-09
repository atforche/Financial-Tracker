"""Install the configured Trivy release when it is not already available."""

from __future__ import annotations

import hashlib
import io
import os
import platform
import tarfile
import tempfile
from argparse import Namespace
from pathlib import Path
from urllib.request import Request, urlopen

from ..config.toolchain import Toolchain
from ..core.context import Context
from .common import find_tool

TRIVY_RELEASES = "https://github.com/aquasecurity/trivy/releases/download"


def run(context: Context, _args: Namespace) -> int:
    """Make the configured Trivy release available to repository commands."""

    if find_tool(context, "trivy") is not None:
        return 0

    version = _configured_version(context)
    archive_name = _archive_name(version)
    release_url = f"{TRIVY_RELEASES}/v{version}"
    archive = _download(f"{release_url}/{archive_name}")
    checksums = _download(f"{release_url}/trivy_{version}_checksums.txt")
    expected = _expected_checksum(checksums, archive_name)
    actual = hashlib.sha256(archive).hexdigest()
    if actual != expected:
        raise RuntimeError(
            f"Trivy download checksum mismatch: expected {expected}, got {actual}"
        )

    binary = _extract_binary(archive)
    context.paths.local_tools.mkdir(mode=0o755, parents=True, exist_ok=True)
    _install_binary(binary, context.paths.trivy)
    print(f"Installed Trivy {version} at {context.paths.trivy}")
    return 0


def _configured_version(context: Context) -> str:
    return Toolchain.read(context.paths.toolchain).require_tool("trivy")


def _archive_name(version: str) -> str:
    operating_system = {"Linux": "Linux", "Darwin": "macOS"}.get(platform.system())
    architecture = {
        "x86_64": "64bit",
        "amd64": "64bit",
        "aarch64": "ARM64",
        "arm64": "ARM64",
    }.get(platform.machine())
    if operating_system is None or architecture is None:
        raise RuntimeError(
            "Automatic Trivy installation supports Linux and macOS on x64 or ARM64"
        )
    return f"trivy_{version}_{operating_system}-{architecture}.tar.gz"


def _download(url: str) -> bytes:
    request = Request(url, headers={"User-Agent": "financial-tracker-orchestrator"})
    with urlopen(request, timeout=60) as response:
        return response.read()


def _expected_checksum(checksums: bytes, archive_name: str) -> str:
    for line in checksums.decode("utf-8").splitlines():
        fields = line.split()
        if len(fields) >= 2 and fields[-1] == archive_name:
            return fields[0]
    raise RuntimeError(f"Trivy checksum is missing for {archive_name}")


def _extract_binary(archive: bytes) -> bytes:
    with tarfile.open(fileobj=io.BytesIO(archive), mode="r:gz") as tar:
        try:
            member = tar.getmember("trivy")
        except KeyError as error:
            raise RuntimeError(
                "Trivy archive does not contain its expected binary"
            ) from error
        if not member.isfile():
            raise RuntimeError("Trivy archive entry is not a regular file")
        binary = tar.extractfile(member)
        if binary is None:
            raise RuntimeError("Trivy binary could not be extracted")
        return binary.read()


def _install_binary(binary: bytes, destination: Path) -> None:
    temporary_path: Path | None = None
    try:
        with tempfile.NamedTemporaryFile(
            dir=destination.parent, prefix=".trivy-", delete=False
        ) as temporary:
            temporary.write(binary)
            temporary_path = Path(temporary.name)
        temporary_path.chmod(0o755)
        os.replace(temporary_path, destination)
    finally:
        if temporary_path is not None:
            temporary_path.unlink(missing_ok=True)
