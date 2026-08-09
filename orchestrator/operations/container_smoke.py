"""Container image smoke test operations."""

from __future__ import annotations

import json
import os
import socket
import tempfile
import time
from http.cookiejar import CookieJar
from pathlib import Path
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode, urljoin
from urllib.request import (
    HTTPCookieProcessor,
    HTTPRedirectHandler,
    Request,
    build_opener,
    urlopen,
)
from uuid import uuid4

from ..config.toolchain import Toolchain
from ..core.paths import RepoPaths
from ..core.runner import Runner
from .migrator import run_migrator

SMOKE_TEST_SUBJECT = "container-smoke-test"
SMOKE_TEST_EMAIL = "container-smoke-test@example.test"
SMOKE_TEST_STANDARD_SUBJECT = "container-smoke-standard"


def prepare_smoke_test_database(
    data_directory: Path,
    migrator_image: str | None = None,
    runner: Runner | None = None,
) -> None:
    """Migrate the smoke database and provision deterministic test users."""

    environment = {
        "AUTH_MODE": "development",
        "DEVELOPMENT_AUTH_SUBJECT": SMOKE_TEST_SUBJECT,
        "DEVELOPMENT_AUTH_EMAIL": SMOKE_TEST_EMAIL,
    }
    image = migrator_image or Toolchain.read(
        RepoPaths.discover().toolchain
    ).require_image("migrator")
    run_migrator(image, data_directory, environment, runner=runner)
    run_migrator(image, data_directory, environment, runner=runner)
    run_migrator(
        image,
        data_directory,
        {
            "AUTH_MODE": "development",
            "DEVELOPMENT_AUTH_SUBJECT": SMOKE_TEST_STANDARD_SUBJECT,
            "DEVELOPMENT_AUTH_EMAIL": "container-smoke-standard@example.test",
            "DEVELOPMENT_AUTH_ROLE": "Standard",
        },
        runner=runner,
    )


class DoNotFollowRedirects(HTTPRedirectHandler):
    """Keep Auth.js callback redirects on the ephemeral smoke-test port."""

    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None


class ContainerSmokeTest:
    """Start the built images and verify their operational endpoints."""

    def __init__(
        self, paths: RepoPaths | None = None, runner: Runner | None = None
    ) -> None:
        self.paths = paths or RepoPaths.discover()
        self.runner = runner or Runner()
        toolchain = Toolchain.read(self.paths.toolchain)
        self.backend_image = toolchain.require_image("backend")
        self.frontend_image = toolchain.require_image("frontend")
        self.migrator_image = toolchain.require_image("migrator")

    def run(self) -> None:
        identifier = uuid4().hex
        network = f"financial-tracker-smoke-{identifier}"
        backend = f"financial-tracker-backend-smoke-{identifier}"
        frontend = f"financial-tracker-frontend-smoke-{identifier}"

        with tempfile.TemporaryDirectory(
            prefix="financial-tracker-smoke-"
        ) as directory_value:
            directory = Path(directory_value)
            database = directory / "database.db"
            logs = directory / "logs"
            database.touch(mode=0o666)
            logs.mkdir(mode=0o777)
            os.chmod(directory, 0o777)
            os.chmod(database, 0o666)
            prepare_smoke_test_database(directory, self.migrator_image, self.runner)

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
                        "--network-alias",
                        "backend",
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
                        "ASPNETCORE_ENVIRONMENT=Development",
                        "--env",
                        "ASPNETCORE_HTTP_PORTS=8080",
                        "--env",
                        "DATABASE_PATH=/data/database.db",
                        "--env",
                        "LOG_DIRECTORY=/logs",
                        "--env",
                        "FRONTEND_ORIGIN=http://localhost",
                        "--env",
                        "AUTH_MODE=development",
                        "--env",
                        f"DEVELOPMENT_AUTH_SUBJECT={SMOKE_TEST_SUBJECT}",
                        "--env",
                        f"DEVELOPMENT_AUTH_ADDITIONAL_SUBJECTS={SMOKE_TEST_STANDARD_SUBJECT}",
                        "--env",
                        "GOOGLE_CLIENT_ID=container-smoke-test",
                        self.backend_image,
                    ]
                )
                backend_port = self.get_published_port(backend, 8080)
                self.wait_for_url(f"http://127.0.0.1:{backend_port}/health/ready")
                self.wait_for_container_health(backend)
                self.create_and_read_account(backend_port, identifier)

                frontend_port = self.get_available_loopback_port()
                frontend_origin = f"http://localhost:{frontend_port}"
                self.run_docker(
                    [
                        "run",
                        "--detach",
                        "--name",
                        frontend,
                        "--network",
                        network,
                        "--publish",
                        f"127.0.0.1:{frontend_port}:3000",
                        "--read-only",
                        "--tmpfs",
                        "/tmp",
                        "--cap-drop",
                        "ALL",
                        "--security-opt",
                        "no-new-privileges:true",
                        "--env",
                        "API_URL=http://backend:8080",
                        "--env",
                        f"PUBLIC_ORIGIN={frontend_origin}",
                        "--env",
                        "AUTH_MODE=development",
                        "--env",
                        f"DEVELOPMENT_AUTH_SUBJECT={SMOKE_TEST_SUBJECT}",
                        "--env",
                        f"DEVELOPMENT_AUTH_ADDITIONAL_SUBJECTS={SMOKE_TEST_STANDARD_SUBJECT}",
                        "--env",
                        "GOOGLE_CLIENT_ID=container-smoke-test",
                        "--env",
                        "GOOGLE_CLIENT_SECRET=container-smoke-test",
                        "--env",
                        f"AUTH_URL={frontend_origin}",
                        "--env",
                        "AUTH_TRUST_HOST=true",
                        "--env",
                        f"AUTH_SECRET={identifier}{identifier}",
                        self.frontend_image,
                    ]
                )
                self.wait_for_url(f"{frontend_origin}/login")
                self.wait_for_container_health(frontend)
                self.verify_frontend_session_and_api_flow(frontend_port)
                self.verify_frontend_sign_in_outage(frontend_port, backend)
                self.run_playwright_e2e(frontend_port, f"{identifier}{identifier}")
            except Exception:
                self.print_container_logs(backend)
                self.print_container_logs(frontend)
                raise
            finally:
                for container in (frontend, backend):
                    self.runner.run(
                        ["docker", "container", "rm", "--force", container],
                        check=False,
                        capture_output=True,
                    )
                self.runner.run(
                    ["docker", "network", "rm", network],
                    check=False,
                    capture_output=True,
                )

    @staticmethod
    def create_and_read_account(backend_port: int, identifier: str) -> None:
        headers = {
            "Authorization": f"Bearer development:{SMOKE_TEST_SUBJECT}",
            "Content-Type": "application/json",
        }
        create_request = Request(
            f"http://127.0.0.1:{backend_port}/accounts/onboard",
            data=json.dumps(
                {
                    "name": f"Smoke account {identifier}",
                    "type": "Standard",
                    "onboardedBalance": 1,
                }
            ).encode(),
            headers=headers,
            method="POST",
        )
        with urlopen(create_request, timeout=30) as response:
            account = json.load(response)
        with urlopen(
            Request(
                f"http://127.0.0.1:{backend_port}/accounts/{account['id']}",
                headers=headers,
            ),
            timeout=30,
        ) as response:
            persisted_account = json.load(response)
        if persisted_account["name"] != account["name"]:
            raise RuntimeError("The backend did not return the account it persisted.")

    @staticmethod
    def verify_frontend_session_and_api_flow(frontend_port: int) -> None:
        base_url = f"http://127.0.0.1:{frontend_port}"
        cookies = CookieJar()
        browser = build_opener(HTTPCookieProcessor(cookies))
        with browser.open(f"{base_url}/api/auth/csrf", timeout=30) as response:
            csrf_token = json.load(response)["csrfToken"]
        sign_in_request = Request(
            f"{base_url}/api/auth/callback/development",
            data=urlencode(
                {
                    "csrfToken": csrf_token,
                    "callbackUrl": f"{base_url}/accounts/workspace",
                    "json": "true",
                    "subject": SMOKE_TEST_SUBJECT,
                }
            ).encode(),
            headers={"Content-Type": "application/x-www-form-urlencoded"},
            method="POST",
        )
        callback_browser = build_opener(
            HTTPCookieProcessor(cookies), DoNotFollowRedirects()
        )
        try:
            with callback_browser.open(sign_in_request, timeout=30):
                pass
        except HTTPError as error:
            if error.code != 302:
                raise
        with browser.open(f"{base_url}/api/auth/session", timeout=30) as response:
            session = json.load(response)
        if (
            session.get("user", {}).get("name") != "Administrator"
            or "idToken" in session
        ):
            raise RuntimeError(
                "The frontend did not create a safe authenticated session."
            )
        with browser.open(f"{base_url}/accounts/workspace", timeout=30) as response:
            if not 200 <= response.status < 300:
                raise RuntimeError(
                    "The authenticated frontend could not load account data."
                )

    def verify_frontend_sign_in_outage(self, frontend_port: int, backend: str) -> None:
        base_url = f"http://127.0.0.1:{frontend_port}"
        cookies = CookieJar()
        browser = build_opener(HTTPCookieProcessor(cookies))
        with browser.open(f"{base_url}/api/auth/csrf", timeout=30) as response:
            csrf_token = json.load(response)["csrfToken"]
        sign_in_request = Request(
            f"{base_url}/api/auth/callback/development",
            data=urlencode(
                {
                    "csrfToken": csrf_token,
                    "callbackUrl": f"{base_url}/accounts/workspace",
                    "json": "true",
                    "subject": SMOKE_TEST_SUBJECT,
                }
            ).encode(),
            headers={"Content-Type": "application/x-www-form-urlencoded"},
            method="POST",
        )
        callback_browser = build_opener(
            HTTPCookieProcessor(cookies), DoNotFollowRedirects()
        )
        self.run_docker(["stop", backend])
        try:
            try:
                callback_browser.open(sign_in_request, timeout=30)
            except HTTPError as error:
                if error.code != 302:
                    raise
                redirect_location = error.headers.get("Location")
                if (
                    redirect_location is None
                    or "/login?error=AccessDenied" not in redirect_location
                ):
                    raise RuntimeError(
                        "A resolver outage did not fail closed to the generic login error."
                    ) from error
                with browser.open(
                    urljoin(base_url, redirect_location), timeout=30
                ) as response:
                    login_page = response.read().decode()
                if (
                    "We could not confirm that this account has access."
                    not in login_page
                ):
                    raise RuntimeError(
                        "The login page did not show the generic resolver outage message."
                    ) from error
            else:
                raise RuntimeError("A resolver outage unexpectedly created a session.")
        finally:
            self.run_docker(["start", backend])
            restarted_backend_port = self.get_published_port(backend, 8080)
            self.wait_for_url(f"http://127.0.0.1:{restarted_backend_port}/health/ready")
            self.wait_for_container_health(backend)

    def run_playwright_e2e(self, frontend_port: int, auth_secret: str) -> None:
        environment = os.environ.copy()
        environment["E2E_BASE_URL"] = f"http://localhost:{frontend_port}"
        environment["E2E_AUTH_SECRET"] = auth_secret
        self.runner.run(
            ["npm", "run", "test:e2e", "--", "--config", "e2e/playwright.config.ts"],
            cwd=self.paths.frontend,
            env=environment,
        )

    def run_docker(self, arguments: list[str]) -> None:
        self.runner.run(["docker", *arguments])

    def get_published_port(self, container: str, container_port: int) -> int:
        result = self.runner.run(
            ["docker", "port", container, f"{container_port}/tcp"],
            capture_output=True,
        )
        return int(result.stdout.strip().rsplit(":", maxsplit=1)[1])

    @staticmethod
    def get_available_loopback_port() -> int:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as listener:
            listener.bind(("127.0.0.1", 0))
            return int(listener.getsockname()[1])

    @staticmethod
    def wait_for_url(url: str) -> None:
        deadline = time.monotonic() + 60
        while time.monotonic() < deadline:
            try:
                with urlopen(url, timeout=3) as response:
                    if 200 <= response.status < 300:
                        return
            except OSError, URLError:
                time.sleep(1)
        raise RuntimeError(f"Container endpoint did not become ready: {url}")

    def wait_for_container_health(self, container: str) -> None:
        deadline = time.monotonic() + 60
        while time.monotonic() < deadline:
            result = self.runner.run(
                [
                    "docker",
                    "inspect",
                    "--format",
                    "{{.State.Health.Status}}",
                    container,
                ],
                capture_output=True,
            )
            status = result.stdout.strip()
            if status == "healthy":
                return
            if status == "unhealthy":
                raise RuntimeError(f"Container became unhealthy: {container}")
            time.sleep(1)
        raise RuntimeError(f"Container {container} did not become healthy")

    def print_container_logs(self, container: str) -> None:
        result = self.runner.run(
            ["docker", "logs", container], check=False, capture_output=True
        )
        if result.returncode == 0:
            print(result.stdout)
            print(result.stderr)
