"""Build deployable container images."""

from __future__ import annotations

import os
from argparse import Namespace

from ..config.toolchain import Toolchain
from ..core.context import Context


def run(context: Context, _args: Namespace) -> int:
    cache_backend = os.environ.get("DOCKER_BUILD_CACHE", "").strip().lower()
    if cache_backend not in ("", "gha"):
        raise ValueError(f"Unsupported Docker build cache backend: {cache_backend}")
    toolchain = Toolchain.read(context.paths.toolchain)
    dotnet_build_args = {
        "DOTNET_SDK_IMAGE": toolchain.require_base_image("dotnet_sdk"),
        "DOTNET_RUNTIME_IMAGE": toolchain.require_base_image("dotnet_runtime"),
    }
    node_build_args = {"NODE_IMAGE": toolchain.require_base_image("node")}
    images = (
        (
            context.paths.backend,
            "backend",
            context.paths.backend_dockerfile,
            dotnet_build_args,
        ),
        (
            context.paths.frontend,
            "frontend",
            context.paths.frontend_dockerfile,
            node_build_args,
        ),
        (
            context.paths.backend,
            "migrator",
            context.paths.migrator_dockerfile,
            dotnet_build_args,
        ),
    )
    for build_context, image_name, dockerfile, build_args in images:
        tag = toolchain.require_image(image_name)
        scope = f"financial-tracker-{image_name}"
        command = (
            ["docker", "buildx", "build"]
            if cache_backend == "gha"
            else ["docker", "build"]
        )
        command.extend(["--tag", tag])
        for name, value in build_args.items():
            command.extend(["--build-arg", f"{name}={value}"])
        if dockerfile is not None:
            command.extend(["--file", str(dockerfile)])
        if cache_backend == "gha":
            command.extend(
                [
                    "--load",
                    "--cache-from",
                    f"type=gha,scope={scope}",
                    "--cache-to",
                    f"type=gha,mode=max,ignore-error=true,scope={scope}",
                ]
            )
        command.append(str(build_context))
        context.runner.run(command, cwd=context.root)
    return 0
