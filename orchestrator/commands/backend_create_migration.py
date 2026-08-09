"""Create a database migration."""

from argparse import Namespace

from ..core.context import Context


def run(context: Context, args: Namespace) -> int:
    return context.runner.run(
        [
            "dotnet",
            "ef",
            "migrations",
            "add",
            args.name,
            "--project",
            str(context.paths.backend_data_project),
            "--msbuildprojectextensionspath",
            str(context.paths.backend_data_artifacts),
        ],
        cwd=context.root,
        env={"DATABASE_PATH": ""},
    ).returncode
