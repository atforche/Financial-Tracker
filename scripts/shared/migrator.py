"""Shared command construction for hardened database migrator containers."""

import subprocess
from pathlib import Path


def run_migrator(image: str, data_directory: Path) -> None:
    """Runs a migrator image against ``database.db`` in the supplied directory."""

    subprocess.run(
        [
            "docker",
            "run",
            "--rm",
            "--read-only",
            "--cap-drop",
            "ALL",
            "--security-opt",
            "no-new-privileges:true",
            "--volume",
            f"{data_directory.resolve()}:/data",
            "--env",
            "DATABASE_PATH=/data/database.db",
            image,
        ],
        check=True,
    )
