"""Shared environment command helpers."""

from ..config.environment import EnvironmentSchema
from ..core.context import Context


def schema(context: Context) -> EnvironmentSchema:
    return EnvironmentSchema.read(context.paths.environment_schema)
