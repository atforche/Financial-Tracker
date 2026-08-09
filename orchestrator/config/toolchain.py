"""Repository tool and image versions loaded from one configuration file."""

from __future__ import annotations

import tomllib
from dataclasses import dataclass
from pathlib import Path


@dataclass(frozen=True)
class Toolchain:
    tools: dict[str, str]
    images: dict[str, str]
    base_images: dict[str, str]

    @classmethod
    def read(cls, path: Path) -> Toolchain:
        with path.open("rb") as file:
            document = tomllib.load(file)
        return cls(
            tools={
                name: str(value) for name, value in document.get("tools", {}).items()
            },
            images={
                name: str(value) for name, value in document.get("images", {}).items()
            },
            base_images={
                name: str(value)
                for name, value in document.get("base_images", {}).items()
            },
        )

    def require_tool(self, name: str) -> str:
        try:
            return self.tools[name]
        except KeyError as error:
            raise ValueError(f"Toolchain tool is not configured: {name}") from error

    def require_image(self, name: str) -> str:
        try:
            return self.images[name]
        except KeyError as error:
            raise ValueError(f"Toolchain image is not configured: {name}") from error

    def require_base_image(self, name: str) -> str:
        try:
            return self.base_images[name]
        except KeyError as error:
            raise ValueError(
                f"Toolchain base image is not configured: {name}"
            ) from error
