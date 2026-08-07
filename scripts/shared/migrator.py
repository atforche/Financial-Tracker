"""Shared command construction for hardened database migrator containers."""

import subprocess
from pathlib import Path


def run_migrator(
    image: str, data_directory: Path, environment: dict[str, str] | None = None
) -> None:
    """Runs a migrator image against ``database.db`` in the supplied directory."""

    environment_arguments: list[str] = []
    for name, value in (environment or {}).items():
        environment_arguments.extend(["--env", f"{name}={value}"])

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
            *environment_arguments,
            image,
        ],
        check=True,
    )
