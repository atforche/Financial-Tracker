"""Install repository dependencies."""

from argparse import Namespace

from ..core.context import Context
from . import backend_restore, frontend_install, python_install, trivy_install


def run(context: Context, args: Namespace) -> int:
    python_install.run(context, args)
    frontend_install.run(context, args)
    trivy_install.run(context, args)
    if context.paths.dotnet_tool_manifest.is_file():
        context.runner.run(
            [
                "dotnet",
                "tool",
                "restore",
                "--tool-manifest",
                str(context.paths.dotnet_tool_manifest.relative_to(context.root)),
            ],
            cwd=context.root,
        )
    backend_restore.run(context, args)
    print("Python, frontend, backend, .NET tool, and Trivy dependencies are installed")
    return 0
