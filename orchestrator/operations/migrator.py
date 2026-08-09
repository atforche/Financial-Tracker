"""Run the hardened database migrator container."""

from __future__ import annotations

from pathlib import Path

from ..core.runner import Runner


def run_migrator(
    image: str,
    data_directory: Path,
    environment: dict[str, str] | None = None,
    runner: Runner | None = None,
) -> None:
    """Run a migrator image against ``database.db`` in ``data_directory``."""

    environment_arguments: list[str] = []
    for name, value in (environment or {}).items():
        environment_arguments.extend(["--env", f"{name}={value}"])

    (runner or Runner()).run(
        [
            "docker",
            "run",
            "--rm",
            "--read-only",
            "--tmpfs",
            "/tmp",
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
    )
