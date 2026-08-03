#!/usr/bin/env python3
"""Creates encrypted database backups and verifies that they can be restored."""

import os
import shutil
import sqlite3
import subprocess
import tempfile
import time
from json import load as load_json
from pathlib import Path
from typing import Annotated
from urllib.error import URLError
from urllib.request import Request, urlopen
from uuid import uuid4

from shared.command import Command
from shared.command_collection import CommandCollection
from shared.configuration import Configuration
from shared.migrator import run_migrator
from shared.step import Step

RESTIC_IMAGE = "restic/restic:0.19.1@sha256:136600b6ff6843d61d355f7f71f460a166429f35de6fd11b568fece3c9a4d510"
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


def main() -> None:
    """Builds and runs the backup command collection."""

    commands = CommandCollection(
        "Creates and verifies encrypted Financial Tracker backups"
    )
    commands.commands.append(InitializeBackupRepository())
    commands.commands.append(BackupDatabase())
    commands.commands.append(VerifyDatabaseRestoration())
    commands.run()


class BackupCommand(Command):
    """Base command for operations against an instance backup repository."""

    path: str

    def validate_arguments(self) -> None:
        """Validates the instance and required backup environment."""

        instance_path = Path(self.path).resolve()
        if not instance_path.is_dir():
            raise ValueError(
                f"Path {instance_path} does not point to a valid instance directory"
            )
        if (
            not (instance_path / ".env").is_file()
            or not (instance_path / "data" / "database.db").is_file()
        ):
            raise ValueError(
                f"Path {instance_path} does not contain a Financial Tracker instance"
            )
        for name in ("RESTIC_REPOSITORY", "RESTIC_PASSWORD"):
            if os.environ.get(name, "").strip() == "":
                raise ValueError(f"Environment variable {name} must be configured")

        self.path = str(instance_path)

    def get_configuration(self) -> Configuration:
        """Reads the deployed instance configuration."""

        return Configuration.build_from_existing_instance(self.path, False)

    def run_restic(
        self, arguments: list[str], volumes: tuple[tuple[Path, str, bool], ...] = ()
    ) -> None:
        """Runs restic with only its required credentials and explicitly mounted paths."""

        command = [
            "docker",
            "run",
            "--rm",
            "--read-only",
            "--user",
            f"{os.getuid()}:{os.getgid()}",
            "--cap-drop",
            "ALL",
            "--security-opt",
            "no-new-privileges:true",
            "--tmpfs",
            "/tmp",
            "--env",
            "HOME=/tmp",
        ]
        repository = os.environ["RESTIC_REPOSITORY"]
        if repository.startswith("/"):
            command.extend(["--volume", f"{Path(repository).resolve()}:/repository"])
            command.extend(["--env", "RESTIC_REPOSITORY=/repository"])
        else:
            command.extend(["--env", "RESTIC_REPOSITORY"])

        for name in RESTIC_ENVIRONMENT_VARIABLES:
            if name != "RESTIC_REPOSITORY" and os.environ.get(name, "") != "":
                command.extend(["--env", name])
        for source, destination, read_only in volumes:
            mode = ":ro" if read_only else ""
            command.extend(["--volume", f"{source.resolve()}:{destination}{mode}"])

        subprocess.run([*command, RESTIC_IMAGE, *arguments], check=True)

    @staticmethod
    def create_database_snapshot(source: Path, destination: Path) -> None:
        """Creates a transactionally consistent SQLite snapshot using the online backup API."""

        source_connection = sqlite3.connect(f"file:{source}?mode=ro", uri=True)
        destination_connection = sqlite3.connect(destination)
        try:
            source_connection.backup(destination_connection)
        finally:
            destination_connection.close()
            source_connection.close()

    @staticmethod
    def validate_database(database_path: Path) -> None:
        """Validates SQLite integrity, foreign keys, and EF migration metadata."""

        connection = sqlite3.connect(f"file:{database_path}?mode=ro", uri=True)
        try:
            integrity_results = connection.execute("PRAGMA integrity_check").fetchall()
            if integrity_results != [("ok",)]:
                raise RuntimeError(
                    f"SQLite integrity check failed: {integrity_results}"
                )
            foreign_key_results = connection.execute(
                "PRAGMA foreign_key_check"
            ).fetchall()
            if foreign_key_results:
                raise RuntimeError(
                    f"SQLite foreign-key check failed: {foreign_key_results}"
                )
            migration_table = connection.execute(
                "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = '__EFMigrationsHistory'"
            ).fetchone()
            if migration_table is None:
                raise RuntimeError(
                    "Restored database does not contain EF migration history"
                )
        finally:
            connection.close()

    @staticmethod
    def get_published_port(container: str) -> int:
        """Returns the host port Docker assigned to a restored backend container."""

        result = subprocess.run(
            ["docker", "port", container, "8080/tcp"],
            check=True,
            capture_output=True,
            text=True,
        )
        return int(result.stdout.strip().rsplit(":", maxsplit=1)[1])

    @staticmethod
    def wait_for_url(url: str) -> None:
        """Waits for a restored backend operational endpoint to become healthy."""

        deadline = time.monotonic() + 60
        while time.monotonic() < deadline:
            try:
                with urlopen(url, timeout=3) as response:
                    if 200 <= response.status < 300:
                        return
            except OSError, URLError:
                time.sleep(1)

        raise RuntimeError(f"Restored backend endpoint did not become ready: {url}")

    @classmethod
    def verify_restored_backend(
        cls, configuration: Configuration, data_directory: Path
    ) -> None:
        """Runs the upgraded backup in the backend image and verifies a protected write."""

        identifier = uuid4().hex
        container = f"financial-tracker-restore-smoke-{identifier}"
        logs_directory = data_directory / "logs"
        logs_directory.mkdir(mode=0o777)
        os.chmod(data_directory, 0o777)
        try:
            subprocess.run(
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
                check=True,
            )
            port = cls.get_published_port(container)
            base_url = f"http://127.0.0.1:{port}"
            cls.wait_for_url(f"{base_url}/health/ready")
            headers = {
                "Authorization": f"Bearer development:{RESTORE_SMOKE_SUBJECT}",
                "Content-Type": "application/json",
            }
            request = Request(
                f"{base_url}/accounts/onboard",
                data=(
                    f'{{"name":"Restored backup smoke {identifier}",'
                    '"type":"Standard","onboardedBalance":1}'
                ).encode(),
                headers=headers,
                method="POST",
            )
            with urlopen(request, timeout=30) as response:
                account = load_json(response)
            with urlopen(
                Request(f"{base_url}/accounts/{account['id']}", headers=headers),
                timeout=30,
            ) as response:
                restored_account = load_json(response)
            if restored_account["name"] != account["name"]:
                raise RuntimeError(
                    "Restored backend did not return its persisted account"
                )
        finally:
            subprocess.run(
                ["docker", "container", "rm", "--force", container],
                check=False,
                capture_output=True,
            )


class InitializeBackupRepository(BackupCommand):
    """Initializes the encrypted restic repository."""

    path: Annotated[str, "Path to the Financial Tracker instance"]

    def __init__(self) -> None:
        """Constructs a backup-repository initialization command."""

        super().__init__("initialize", "Initializes the encrypted backup repository")
        self.steps.append(
            Step(
                "Initialize Backup Repository",
                "Backup repository initialized",
                self.initialize,
            )
        )

    def initialize(self) -> None:
        """Initializes a new restic repository with the configured password."""

        self.run_restic(["init"])


class BackupDatabase(BackupCommand):
    """Creates and retains an encrypted off-host database backup."""

    path: Annotated[str, "Path to the Financial Tracker instance"]

    def __init__(self) -> None:
        """Constructs a database backup command."""

        super().__init__(
            "backup", "Creates an encrypted database backup and applies retention"
        )
        self.steps.append(
            Step("Back Up Database", "Encrypted database backup completed", self.backup)
        )

    def backup(self) -> None:
        """Snapshots the live database, validates it, and sends it to restic."""

        configuration = self.get_configuration()
        with tempfile.TemporaryDirectory(
            prefix="financial-tracker-backup-"
        ) as temporary_directory:
            snapshot_directory = Path(temporary_directory)
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


class VerifyDatabaseRestoration(BackupCommand):
    """Restores and validates the latest encrypted database backup."""

    path: Annotated[str, "Path to the Financial Tracker instance"]

    def __init__(self) -> None:
        """Constructs a restoration-verification command."""

        super().__init__(
            "verify",
            "Restores the latest backup and verifies database integrity and migration",
        )
        self.steps.append(
            Step(
                "Verify Backup Restoration", "Backup restoration verified", self.verify
            )
        )

    def verify(self) -> None:
        """Checks the repository and exercises restoration into an isolated directory."""

        configuration = self.get_configuration()
        self.run_restic(["check"])
        with tempfile.TemporaryDirectory(
            prefix="financial-tracker-restore-"
        ) as temporary_directory:
            restore_directory = Path(temporary_directory)
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

            restored_database = restore_directory / "snapshot/database.db"
            if not restored_database.is_file():
                raise RuntimeError("Restic did not restore the expected database file")
            self.validate_database(restored_database)

            migration_directory = restore_directory / "migration"
            migration_directory.mkdir(mode=0o777)
            migration_database = migration_directory / "database.db"
            shutil.copy2(restored_database, migration_database)
            os.chmod(migration_database, 0o666)
            run_migrator(configuration.migrator_image, migration_directory)
            self.validate_database(migration_database)
            self.verify_restored_backend(configuration, migration_directory)


if __name__ == "__main__":
    main()
