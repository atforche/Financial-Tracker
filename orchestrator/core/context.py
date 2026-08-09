"""Execution context shared by command modules."""

from __future__ import annotations

from dataclasses import dataclass, field
from pathlib import Path

from .paths import RepoPaths
from .runner import Runner


@dataclass
class Context:
    """Explicit dependencies available to every command."""

    paths: RepoPaths = field(default_factory=RepoPaths.discover)
    runner: Runner = field(default_factory=Runner)

    @property
    def root(self) -> Path:
        return self.paths.root
