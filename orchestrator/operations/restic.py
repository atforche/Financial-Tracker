"""Shared hardened execution for Restic operations."""

from __future__ import annotations

import os
import subprocess
from pathlib import Path

from ..core.paths import RepoPaths
from ..core.runner import Runner

RESTIC_IMAGE = "restic/restic:0.19.1@sha256:136600b6ff6843d61d355f7f71f460a166429f35de6fd11b568fece3c9a4d510"
RESTIC_ENVIRONMENT_VARIABLES = (
    "RESTIC_REPOSITORY",
    "RESTIC_PASSWORD",
    "AWS_ACCESS_KEY_ID",
    "AWS_SECRET_ACCESS_KEY",
    "AWS_SESSION_TOKEN",
    "AWS_DEFAULT_REGION",
)


def run_restic(
    arguments: list[str],
    repository: str | Path | None = None,
    volumes: tuple[tuple[Path, str, bool], ...] = (),
    password: str | None = None,
    pass_aws_credentials: bool = True,
    *,
    capture_output: bool = False,
    image: str | None = None,
    runner: Runner | None = None,
) -> subprocess.CompletedProcess[str]:
    """Runs Restic in a restricted container with explicitly mounted paths.

    A supplied password is placed in the Docker process environment rather than
    in the command arguments, keeping it out of shell history and process argv.
    """

    repository_value = (
        str(repository)
        if repository is not None
        else os.environ.get("RESTIC_REPOSITORY", "")
    )
    if repository_value == "":
        raise ValueError("A Restic repository must be configured")

    command = [
        "docker",
        "run",
        "--rm",
        "--read-only",
        "--user",
        f"{os.getuid()}:{os.getgid()}",
        "--cap-drop",
        "ALL",
        "--security-opt",
        "no-new-privileges:true",
        "--tmpfs",
        "/tmp",
        "--env",
        "HOME=/tmp",
    ]
    if repository_value.startswith("/"):
        command.extend(["--volume", f"{Path(repository_value).resolve()}:/repository"])
        command.extend(["--env", "RESTIC_REPOSITORY=/repository"])
    else:
        command.extend(["--env", "RESTIC_REPOSITORY"])

    docker_environment: dict[str, str] = {}
    for name in RESTIC_ENVIRONMENT_VARIABLES:
        if name == "RESTIC_REPOSITORY":
            continue
        if name.startswith("AWS_") and not pass_aws_credentials:
            continue
        if name == "RESTIC_PASSWORD" and password is not None:
            docker_environment[name] = password
            command.extend(["--env", name])
        elif os.environ.get(name, "") != "":
            command.extend(["--env", name])
    for source, destination, read_only in volumes:
        mode = ":ro" if read_only else ""
        command.extend(["--volume", f"{source.resolve()}:{destination}{mode}"])

    selected_image = image or _default_restic_image()
    return (runner or Runner()).run(
        [*command, selected_image, *arguments],
        env=docker_environment,
        capture_output=capture_output,
    )


def _default_restic_image() -> str:
    """Read the pinned Restic image from the repository toolchain."""

    from ..config.toolchain import Toolchain

    return Toolchain.read(RepoPaths.discover().toolchain).require_image("restic")
