"""Registration for environment commands."""

from ..config.environment import EnvironmentSchema
from ..core.paths import RepoPaths
from . import env_explain, env_render, env_validate
from .common import add_command


def register(commands: object) -> None:
    def configure_validate(parser):
        parser.add_argument(
            "--profile",
            required=True,
            choices=sorted(
                EnvironmentSchema.read(RepoPaths.discover().environment_schema).profiles
            ),
        )
        parser.add_argument(
            "--file", help="Optional dotenv file to layer over the process environment"
        )
        parser.add_argument("--allow-unknown", action="store_true")

    add_command(
        commands,
        "validate",
        "Validate an environment profile",
        env_validate.run,
        configure_validate,
    )

    def configure_explain(parser):
        parser.add_argument("name")

    add_command(
        commands,
        "explain",
        "Explain one environment variable",
        env_explain.run,
        configure_explain,
    )

    def configure_render(parser):
        parser.add_argument("--profile", required=True, choices=["debug", "production"])
        parser.add_argument("--output")
        parser.add_argument("--force", action="store_true")

    add_command(
        commands,
        "render",
        "Render a profile environment file",
        env_render.run,
        configure_render,
    )
