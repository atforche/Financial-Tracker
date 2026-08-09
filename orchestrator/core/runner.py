"""Safe, testable subprocess execution."""

from __future__ import annotations

import os
import shlex
import subprocess
from collections.abc import Mapping, Sequence
from dataclasses import dataclass
from pathlib import Path


@dataclass
class Runner:
    """Runs external tools with explicit arguments and working directories."""

    verbose: bool = True

    def run(
        self,
        arguments: Sequence[str],
        *,
        cwd: Path | None = None,
        env: Mapping[str, str] | None = None,
        check: bool = True,
        capture_output: bool = False,
        input_text: str | None = None,
    ) -> subprocess.CompletedProcess[str]:
        """Run a command while merging environment overrides with the parent environment."""

        command = [str(argument) for argument in arguments]
        if self.verbose:
            print(f"Running subprocess: {shlex.join(command)}")

        process_environment = os.environ.copy()
        if env is not None:
            process_environment.update(env)

        return subprocess.run(
            command,
            cwd=cwd,
            env=process_environment,
            check=check,
            capture_output=capture_output,
            text=True,
            input=input_text,
        )
