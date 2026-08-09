"""Validate one environment profile."""

from argparse import Namespace
from pathlib import Path

from ..config.environment import merged_environment, read_dotenv
from ..core.context import Context
from .env_support import schema


def run(context: Context, args: Namespace) -> int:
    source = Path(args.file) if args.file else None
    file_values = read_dotenv(source) if source else {}
    all_values = merged_environment(source)
    loaded_schema = schema(context)
    values = {name: all_values.get(name, "") for name in loaded_schema.variables}
    errors = loaded_schema.validate(args.profile, values, allow_unknown=True)
    unknown = sorted(set(file_values) - set(loaded_schema.variables))
    if not args.allow_unknown:
        errors.extend(f"Unknown environment variable: {name}" for name in unknown)
    if errors:
        for error in errors:
            print(f"- {error}")
        return 1
    print(f"Environment profile '{args.profile}' is valid")
    return 0
