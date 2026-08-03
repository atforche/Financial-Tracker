#!/usr/bin/env python3
"""Helper scripts for verifying Financial Tracker container images."""

import os
import subprocess
import tempfile
import time
from pathlib import Path
from urllib.error import URLError
from urllib.request import urlopen
from uuid import uuid4

from shared.command import Command
from shared.command_collection import CommandCollection
from shared.migrator import run_migrator
from shared.step import Step


def main():
    """Builds and runs the command collection for this script."""

    commands = CommandCollection(
        "Helper scripts for verifying Financial Tracker container images"
    )
    commands.commands.append(BuildContainerImages())
    commands.commands.append(SmokeTestContainerImages())
    commands.run()


class BuildContainerImages(Command):
    """Builds the deployable backend and frontend container images."""

    def __init__(self):
        """Constructs a new instance of this class."""

        super().__init__(
            "build", "Builds the deployable application and migrator container images"
        )
        self.steps.append(
            Step("Build Backend Image", "Backend image built", self.build_backend_image)
        )
        self.steps.append(
            Step(
                "Build Frontend Image",
                "Frontend image built",
                self.build_frontend_image,
            )
        )
        self.steps.append(
            Step(
                "Build Migrator Image",
                "Migrator image built",
                self.build_migrator_image,
            )
        )

    def build_backend_image(self) -> None:
        """Builds the backend image used by verification and release workflows."""

        self.run_subprocess(
            "docker build ../backend --tag financial-tracker-backend:workflow"
        )

    def build_frontend_image(self) -> None:
        """Builds the frontend image used by verification and release workflows."""

        self.run_subprocess(
            "docker build ../frontend --tag financial-tracker-frontend:workflow"
        )

    def build_migrator_image(self) -> None:
        """Builds the database migrator image used by verification and release workflows."""

        self.run_subprocess(
            "docker build ../backend --file ../backend/Migrator.Dockerfile --tag financial-tracker-migrator:workflow"
        )


class SmokeTestContainerImages(Command):
    """Starts the deployable images and verifies their operational endpoints."""

    def __init__(self):
        """Constructs a new instance of this class."""

        super().__init__(
            "smoke-test",
            "Starts the built container images and verifies they become healthy",
        )
        self.steps.append(
            Step(
                "Smoke Test Container Images",
                "Container images started successfully",
                self.smoke_test_images,
            )
        )

    def smoke_test_images(self) -> None:
        """Starts the built images and verifies their operational endpoints."""

        identifier = uuid4().hex
        network = f"financial-tracker-smoke-{identifier}"
        backend = f"financial-tracker-backend-smoke-{identifier}"
        frontend = f"financial-tracker-frontend-smoke-{identifier}"

        with tempfile.TemporaryDirectory(
            prefix="financial-tracker-smoke-"
        ) as temporary_directory:
            directory = Path(temporary_directory)
            database = directory / "database.db"
            logs = directory / "logs"
            database.touch(mode=0o666)
            logs.mkdir(mode=0o777)
            os.chmod(directory, 0o777)
            os.chmod(database, 0o666)

            run_migrator("financial-tracker-migrator:workflow", database.parent)
            run_migrator("financial-tracker-migrator:workflow", database.parent)

            self.run_docker(["network", "create", network])
            try:
                self.run_docker(
                    [
                        "run",
                        "--detach",
                        "--name",
                        backend,
                        "--network",
                        network,
                        "--publish",
                        "127.0.0.1::8080",
                        "--read-only",
                        "--tmpfs",
                        "/tmp",
                        "--cap-drop",
                        "ALL",
                        "--security-opt",
                        "no-new-privileges:true",
                        "--volume",
                        f"{directory}:/data",
                        "--volume",
                        f"{logs}:/logs",
                        "--env",
                        "ASPNETCORE_ENVIRONMENT=Production",
                        "--env",
                        "ASPNETCORE_HTTP_PORTS=8080",
                        "--env",
                        "DATABASE_PATH=/data/database.db",
                        "--env",
                        "LOG_DIRECTORY=/logs",
                        "--env",
                        "FRONTEND_ORIGIN=https://localhost",
                        "--env",
                        "GOOGLE_CLIENT_ID=container-smoke-test",
                        "--env",
                        "GOOGLE_ALLOWED_SUBJECTS=container-smoke-test",
                        "financial-tracker-backend:workflow",
                    ]
                )
                backend_port = self.get_published_port(backend, 8080)
                self.wait_for_url(f"http://127.0.0.1:{backend_port}/health/ready")
                self.wait_for_container_health(backend)

                self.run_docker(
                    [
                        "run",
                        "--detach",
                        "--name",
                        frontend,
                        "--network",
                        network,
                        "--publish",
                        "127.0.0.1::3000",
                        "--read-only",
                        "--tmpfs",
                        "/tmp",
                        "--cap-drop",
                        "ALL",
                        "--security-opt",
                        "no-new-privileges:true",
                        "--env",
                        f"API_URL=http://{backend}:8080",
                        "--env",
                        "PUBLIC_ORIGIN=https://localhost",
                        "--env",
                        "GOOGLE_CLIENT_ID=container-smoke-test",
                        "--env",
                        "GOOGLE_CLIENT_SECRET=container-smoke-test",
                        "--env",
                        "GOOGLE_ALLOWED_SUBJECTS=container-smoke-test",
                        "--env",
                        "AUTH_URL=https://localhost",
                        "--env",
                        "AUTH_TRUST_HOST=true",
                        "--env",
                        f"AUTH_SECRET={identifier}{identifier}",
                        "financial-tracker-frontend:workflow",
                    ]
                )
                frontend_port = self.get_published_port(frontend, 3000)
                self.wait_for_url(f"http://127.0.0.1:{frontend_port}/login")
                self.wait_for_container_health(frontend)
            except Exception:
                self.print_container_logs(backend)
                self.print_container_logs(frontend)
                raise
            finally:
                subprocess.run(
                    ["docker", "container", "rm", "--force", frontend],
                    check=False,
                    capture_output=True,
                )
                subprocess.run(
                    ["docker", "container", "rm", "--force", backend],
                    check=False,
                    capture_output=True,
                )
                subprocess.run(
                    ["docker", "network", "rm", network],
                    check=False,
                    capture_output=True,
                )

    @staticmethod
    def run_docker(arguments: list[str]) -> None:
        """Runs a Docker command and fails when it does not succeed."""

        subprocess.run(["docker", *arguments], check=True)

    @staticmethod
    def get_published_port(container: str, container_port: int) -> int:
        """Returns the host port Docker assigned to a container port."""

        result = subprocess.run(
            ["docker", "port", container, f"{container_port}/tcp"],
            check=True,
            capture_output=True,
            text=True,
        )
        return int(result.stdout.strip().rsplit(":", maxsplit=1)[1])

    @staticmethod
    def wait_for_url(url: str) -> None:
        """Waits for an HTTP endpoint to return a successful response."""

        deadline = time.monotonic() + 60
        while time.monotonic() < deadline:
            try:
                with urlopen(url, timeout=3) as response:
                    if 200 <= response.status < 300:
                        return
            except OSError, URLError:
                time.sleep(1)

        raise RuntimeError(f"Container endpoint did not become ready: {url}")

    @staticmethod
    def wait_for_container_health(container: str) -> None:
        """Waits for Docker to report that a container is healthy."""

        deadline = time.monotonic() + 60
        while time.monotonic() < deadline:
            result = subprocess.run(
                [
                    "docker",
                    "inspect",
                    "--format",
                    "{{.State.Health.Status}}",
                    container,
                ],
                check=True,
                capture_output=True,
                text=True,
            )
            status = result.stdout.strip()
            if status == "healthy":
                return
            if status == "unhealthy":
                raise RuntimeError(f"Container became unhealthy: {container}")
            time.sleep(1)

        raise RuntimeError(f"Container did not become healthy: {container}")

    @staticmethod
    def print_container_logs(container: str) -> None:
        """Prints available container logs after a smoke-test failure."""

        result = subprocess.run(
            ["docker", "logs", container], check=False, capture_output=True, text=True
        )
        if result.returncode == 0:
            print(result.stdout)
            print(result.stderr)


if __name__ == "__main__":
    main()
