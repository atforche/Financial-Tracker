"""Registration for deployment commands."""

from . import deploy_deploy, deploy_rollback
from .common import add_command


def register(commands: object) -> None:
    def configure_deploy(parser):
        parser.add_argument("--path", required=True)
        parser.add_argument("--release-manifest", required=True)
        parser.add_argument("--change-configuration", action="store_true")

    add_command(
        commands,
        "deploy",
        "Deploy an immutable release",
        deploy_deploy.run,
        configure_deploy,
    )

    def configure_rollback(parser):
        parser.add_argument("--path", required=True)

    add_command(
        commands,
        "rollback",
        "Roll back to the previous healthy release",
        deploy_rollback.run,
        configure_rollback,
    )
