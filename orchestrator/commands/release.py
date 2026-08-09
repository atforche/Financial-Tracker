"""Registration for release commands."""

from . import release_validate_artifact, release_validate_run
from .common import add_command


def register(commands: object) -> None:
    def configure_run(parser):
        parser.add_argument("--commit", required=True)
        parser.add_argument("--run-id", required=True, type=int)

    add_command(
        commands,
        "validate-run",
        "Validate a successful release workflow run",
        release_validate_run.run,
        configure_run,
    )

    def configure_artifact(parser):
        parser.add_argument("--commit", required=True)
        parser.add_argument("--manifest", required=True)

    add_command(
        commands,
        "validate-artifact",
        "Validate a downloaded release manifest",
        release_validate_artifact.run,
        configure_artifact,
    )
