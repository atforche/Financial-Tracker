"""Explain one environment variable."""

from argparse import Namespace

from ..core.context import Context
from .env_support import schema


def run(context: Context, args: Namespace) -> int:
    loaded_schema = schema(context)
    try:
        variable = loaded_schema.variables[args.name]
    except KeyError as error:
        raise ValueError(f"Unknown environment variable: {args.name}") from error
    required = ", ".join(sorted(variable.required_in)) or "none"
    print(variable.name)
    print(f"Description: {variable.description}")
    print(f"Secret: {'yes' if variable.secret else 'no'}")
    print(f"Required in: {required}")
    print(f"Derived: {'yes' if variable.derived else 'no'}")
    return 0
