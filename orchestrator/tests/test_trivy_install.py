import hashlib
import os
from argparse import Namespace

from orchestrator.commands import trivy_install
from orchestrator.core.context import Context
from orchestrator.core.paths import RepoPaths


def test_install_downloads_pinned_trivy_to_local_tools(tmp_path, monkeypatch):
    config = tmp_path / "config"
    config.mkdir()
    (config / "toolchain.toml").write_text(
        '[tools]\ntrivy = "0.73.0"\n', encoding="utf-8"
    )
    context = Context(paths=RepoPaths(tmp_path))
    archive_name = "trivy_0.73.0_Linux-64bit.tar.gz"
    archive = b"test archive"
    checksum = hashlib.sha256(archive).hexdigest()
    downloads: list[str] = []

    def fake_download(url: str) -> bytes:
        downloads.append(url)
        if url.endswith("checksums.txt"):
            return f"{checksum}  {archive_name}\n".encode()
        return archive

    monkeypatch.setattr(trivy_install, "find_tool", lambda *_args: None)
    monkeypatch.setattr(trivy_install, "_download", fake_download)
    monkeypatch.setattr(trivy_install, "_extract_binary", lambda _archive: b"trivy")
    monkeypatch.setattr(trivy_install.platform, "system", lambda: "Linux")
    monkeypatch.setattr(trivy_install.platform, "machine", lambda: "x86_64")

    trivy_install.run(context, Namespace())

    assert downloads == [
        f"https://github.com/aquasecurity/trivy/releases/download/v0.73.0/{archive_name}",
        "https://github.com/aquasecurity/trivy/releases/download/v0.73.0/trivy_0.73.0_checksums.txt",
    ]
    assert context.paths.trivy.read_bytes() == b"trivy"
    assert os.access(context.paths.trivy, os.X_OK)
