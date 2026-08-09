"""Encrypted backup, restore, and verification operations."""

from __future__ import annotations

import os
import shutil
import sqlite3
import tempfile
import time
from json import load as load_json
from pathlib import Path
from urllib.error import URLError
from urllib.request import Request, urlopen
from uuid import uuid4

from ..config.toolchain import Toolchain
from ..core.paths import RepoPaths
from ..core.runner import Runner
from .configuration import Configuration
from .instance import resolve_instance_path
from .migrator import run_migrator
from .restic import run_restic as execute_restic

RESTIC_ENVIRONMENT_VARIABLES = (
    "RESTIC_REPOSITORY",
    "RESTIC_PASSWORD",
    "AWS_ACCESS_KEY_ID",
    "AWS_SECRET_ACCESS_KEY",
    "AWS_SESSION_TOKEN",
    "AWS_DEFAULT_REGION",
)
BACKUP_TAG = "financial-tracker"
RESTORE_SMOKE_SUBJECT = "backup-restore-smoke-test"
RESTORE_SMOKE_EMAIL = "backup-restore-smoke-test@example.test"


class BackupOperations:
    """Operations against one deployed instance backup repository."""

    def __init__(self, path_value: str, runner: Runner | None = None) -> None:
        self.path = resolve_instance_path(path_value)
        self.runner = runner or Runner()
        self.restic_image = Toolchain.read(
            RepoPaths.discover().toolchain
        ).require_image("restic")
        if not self.path.is_dir():
            raise ValueError(
                f"Path {self.path} does not point to a valid instance directory"
            )
        if not (
            (self.path / ".env").is_file()
            and (self.path / "data" / "database.db").is_file()
        ):
            raise ValueError(
                f"Path {self.path} does not contain a Financial Tracker instance"
            )
        for name in ("RESTIC_REPOSITORY", "RESTIC_PASSWORD"):
            if not os.environ.get(name, "").strip():
                raise ValueError(f"Environment variable {name} must be configured")

    def configuration(self) -> Configuration:
        return Configuration.build_from_existing_instance(str(self.path), False)

    def run_restic(
        self,
        arguments: list[str],
        volumes: tuple[tuple[Path, str, bool], ...] = (),
    ) -> None:
        """Run Restic with only its required credentials and explicit mounts."""

        execute_restic(
            arguments,
            volumes=volumes,
            image=self.restic_image,
            runner=self.runner,
        )

    @staticmethod
    def create_database_snapshot(source: Path, destination: Path) -> None:
        source_connection = sqlite3.connect(f"file:{source}?mode=ro", uri=True)
        destination_connection = sqlite3.connect(destination)
        try:
            source_connection.backup(destination_connection)
        finally:
            destination_connection.close()
            source_connection.close()

    @staticmethod
    def validate_database(database_path: Path) -> None:
        connection = sqlite3.connect(f"file:{database_path}?mode=ro", uri=True)
        try:
            if connection.execute("PRAGMA integrity_check").fetchall() != [("ok",)]:
                raise RuntimeError("SQLite integrity check failed")
            if connection.execute("PRAGMA foreign_key_check").fetchall():
                raise RuntimeError("SQLite foreign-key check failed")
            migration_table = connection.execute(
                "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = '__EFMigrationsHistory'"
            ).fetchone()
            if migration_table is None:
                raise RuntimeError(
                    "Restored database does not contain EF migration history"
                )
        finally:
            connection.close()

    def get_published_port(self, container: str) -> int:
        result = self.runner.run(
            ["docker", "port", container, "8080/tcp"],
            capture_output=True,
        )
        return int(result.stdout.strip().rsplit(":", maxsplit=1)[1])

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
        raise RuntimeError(f"Restored backend endpoint did not become ready: {url}")

    def verify_restored_backend(
        self, configuration: Configuration, data_directory: Path
    ) -> None:
        identifier = uuid4().hex
        container = f"financial-tracker-restore-smoke-{identifier}"
        logs_directory = data_directory / "logs"
        logs_directory.mkdir(mode=0o777)
        os.chmod(data_directory, 0o777)
        try:
            self.runner.run(
                [
                    "docker",
                    "run",
                    "--detach",
                    "--name",
                    container,
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
                    f"{data_directory.resolve()}:/data",
                    "--volume",
                    f"{logs_directory.resolve()}:/logs",
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
                    f"DEVELOPMENT_AUTH_SUBJECT={RESTORE_SMOKE_SUBJECT}",
                    configuration.backend_image,
                ],
            )
            port = self.get_published_port(container)
            base_url = f"http://127.0.0.1:{port}"
            self.wait_for_url(f"{base_url}/health/ready")
            headers = {
                "Authorization": f"Bearer development:{RESTORE_SMOKE_SUBJECT}",
                "Content-Type": "application/json",
            }
            with urlopen(
                Request(f"{base_url}/users/me", headers=headers), timeout=30
            ) as response:
                application_user = load_json(response)
            if application_user["email"] != RESTORE_SMOKE_EMAIL:
                raise RuntimeError(
                    "Restored backend did not resolve the seeded application user"
                )
            with urlopen(
                Request(f"{base_url}/accounts", headers=headers), timeout=30
            ) as response:
                accounts = load_json(response)
            if isinstance(accounts, list):
                account_items = accounts
            elif isinstance(accounts, dict) and "items" in accounts:
                account_items = accounts["items"]
            else:
                raise RuntimeError(
                    "Restored backend returned an unexpected accounts response"
                )
            if not account_items:
                raise RuntimeError(
                    "Restored backend returned no accounts from the restored database"
                )
            account = account_items[0]
            request = Request(
                f"{base_url}/accounts/{account['id']}",
                data=(
                    f'{{"name":"{account["name"]}","financialInstitution":"smoke-{identifier}"}}'
                ).encode(),
                headers=headers,
                method="POST",
            )
            with urlopen(request, timeout=30) as response:
                updated_account = load_json(response)
            if updated_account["financialInstitution"] != f"smoke-{identifier}":
                raise RuntimeError(
                    "Restored backend did not persist the account update"
                )
        finally:
            self.runner.run(
                ["docker", "container", "rm", "--force", container],
                check=False,
                capture_output=True,
            )

    def initialize(self) -> None:
        self.run_restic(["init"])

    def backup(self) -> None:
        configuration = self.configuration()
        with tempfile.TemporaryDirectory(
            prefix="financial-tracker-backup-"
        ) as directory:
            snapshot_directory = Path(directory)
            snapshot_path = snapshot_directory / "database.db"
            self.create_database_snapshot(
                Path(configuration.get_database_file_path()), snapshot_path
            )
            self.validate_database(snapshot_path)
            self.run_restic(
                [
                    "backup",
                    "/snapshot/database.db",
                    "--host",
                    configuration.name,
                    "--tag",
                    BACKUP_TAG,
                ],
                ((snapshot_directory, "/snapshot", True),),
            )
        self.run_restic(
            [
                "forget",
                "--host",
                configuration.name,
                "--tag",
                BACKUP_TAG,
                "--keep-daily",
                "7",
                "--keep-weekly",
                "5",
                "--keep-monthly",
                "12",
                "--prune",
            ]
        )

    def verify(self) -> None:
        configuration = self.configuration()
        self.run_restic(["check"])
        with tempfile.TemporaryDirectory(
            prefix="financial-tracker-restore-"
        ) as directory:
            restore_directory = Path(directory)
            self.run_restic(
                [
                    "restore",
                    "latest",
                    "--host",
                    configuration.name,
                    "--tag",
                    BACKUP_TAG,
                    "--target",
                    "/restore",
                    "--verify",
                ],
                ((restore_directory, "/restore", False),),
            )
            restored_database = restore_directory / "snapshot" / "database.db"
            if not restored_database.is_file():
                raise RuntimeError("Restic did not restore the expected database file")
            self.validate_database(restored_database)

            migration_directory = restore_directory / "migration"
            migration_directory.mkdir(mode=0o777)
            os.chmod(migration_directory, 0o777)
            migration_database = migration_directory / "database.db"
            shutil.copy2(restored_database, migration_database)
            os.chmod(migration_database, 0o666)
            run_migrator(
                configuration.migrator_image,
                migration_directory,
                {
                    "AUTH_MODE": "development",
                    "DEVELOPMENT_AUTH_SUBJECT": RESTORE_SMOKE_SUBJECT,
                    "DEVELOPMENT_AUTH_EMAIL": RESTORE_SMOKE_EMAIL,
                },
                runner=self.runner,
            )
            self.validate_database(migration_database)
            self.verify_restored_backend(configuration, migration_directory)
