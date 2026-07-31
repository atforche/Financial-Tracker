#!/usr/bin/env python3
"""Helper scripts for scanning Financial Tracker dependencies and container images."""

import json
import subprocess
from typing import Any
from shared.command import Command
from shared.command_collection import CommandCollection
from shared.step import Step

CADDY_IMAGE = "caddy:2.11.4-alpine"


def main():
    """Builds and runs the command collection for this script."""

    commands = CommandCollection("Helper scripts for scanning Financial Tracker dependencies and container images")
    commands.commands.append(ScanDependencies())
    commands.commands.append(ScanContainerImages())
    commands.run()


class ScanDependencies(Command):
    """Scans application dependencies for known vulnerabilities."""

    def __init__(self):
        """Constructs a new instance of this class."""

        super().__init__("scan-dependencies", "Scans dependencies for high and critical vulnerabilities")
        self.steps.append(Step("Scan Frontend Dependencies", "Frontend dependencies scanned", self.scan_frontend_dependencies))
        self.steps.append(Step("Scan Backend Dependencies", "Backend dependencies scanned", self.scan_backend_dependencies))

    def scan_frontend_dependencies(self) -> None:
        """Fails when npm reports a high or critical dependency vulnerability."""

        self.run_subprocess("npm audit --prefix ../frontend --audit-level=high")

    def scan_backend_dependencies(self) -> None:
        """Fails when NuGet reports a dependency vulnerability."""

        command = [
            "dotnet",
            "package",
            "list",
            "--project",
            "../backend/Backend.sln",
            "--vulnerable",
            "--include-transitive",
            "--format",
            "json",
            "--no-restore",
        ]
        result = subprocess.run(command, check=False, capture_output=True, text=True)
        if result.stdout:
            print(result.stdout)
        if result.stderr:
            print(result.stderr)
        if result.returncode != 0:
            raise RuntimeError("NuGet vulnerability scan could not complete")

        report: dict[str, Any] = json.loads(result.stdout)
        vulnerabilities = self.get_vulnerabilities(report)
        if vulnerabilities:
            raise RuntimeError(
                "NuGet reported vulnerable packages: " + ", ".join(sorted(vulnerabilities)))

    @staticmethod
    def get_vulnerabilities(value: Any) -> set[str]:
        """Extracts package identifiers from NuGet's vulnerability report."""

        if isinstance(value, list):
            return set().union(*(ScanDependencies.get_vulnerabilities(item) for item in value))
        if not isinstance(value, dict):
            return set()

        vulnerabilities = value.get("vulnerabilities")
        package_id = value.get("id")
        package_version = value.get("resolvedVersion") or value.get("requestedVersion")
        if isinstance(vulnerabilities, list) and vulnerabilities and isinstance(package_id, str):
            return {f"{package_id} {package_version or ''}".strip()}

        return set().union(*(ScanDependencies.get_vulnerabilities(item) for item in value.values()))


class ScanContainerImages(Command):
    """Scans already-built deployable container images."""

    def __init__(self):
        """Constructs a new instance of this class."""

        super().__init__("scan-images", "Scans deployable container images for high and critical vulnerabilities")
        self.steps.append(Step("Scan Backend Image", "Backend image scanned", self.scan_backend_image))
        self.steps.append(Step("Scan Frontend Image", "Frontend image scanned", self.scan_frontend_image))
        self.steps.append(Step("Scan Migrator Image", "Migrator image scanned", self.scan_migrator_image))
        self.steps.append(Step("Scan Proxy Image", "Proxy image scanned", self.scan_proxy_image))

    def scan_backend_image(self) -> None:
        """Fails when Trivy finds a high or critical backend image vulnerability."""

        self.scan_image("financial-tracker-backend:workflow")

    def scan_frontend_image(self) -> None:
        """Fails when Trivy finds a high or critical frontend image vulnerability."""

        self.scan_image("financial-tracker-frontend:workflow")

    def scan_migrator_image(self) -> None:
        """Fails when Trivy finds a high or critical migrator image vulnerability."""

        self.scan_image("financial-tracker-migrator:workflow")

    def scan_proxy_image(self) -> None:
        """Reports high and critical reverse-proxy image vulnerabilities without failing the pipeline."""

        print(
            "Caddy findings are reported without failing CI because no clean official Caddy image is currently available. "
            "Review this exception whenever Caddy releases a new image.")
        self.run_subprocess(f"docker pull {CADDY_IMAGE}")
        self.run_subprocess(
            f"trivy image --exit-code 0 --severity HIGH,CRITICAL --scanners vuln {CADDY_IMAGE}")

    def scan_image(self, image: str) -> None:
        """Scans an image for high and critical OS and library vulnerabilities."""

        self.run_subprocess(
            f"trivy image --exit-code 1 --severity HIGH,CRITICAL --scanners vuln {image}")


if __name__ == "__main__":
    main()
