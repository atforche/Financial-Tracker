"""Restore an encrypted production backup into the native debug database."""

from __future__ import annotations

import getpass
import json
import os
import shutil
import sys
import tempfile
from dataclasses import dataclass
from datetime import datetime
from pathlib import Path
from urllib.parse import urlsplit
from uuid import uuid4

from ..config.environment import read_dotenv
from ..config.toolchain import Toolchain
from ..core.paths import RepoPaths
from ..core.runner import Runner
from .backup import BACKUP_TAG, BackupOperations
from .migrator import run_migrator
from .restic import run_restic


@dataclass(frozen=True)
class ResticSnapshot:
    """Restic snapshot details displayed by the interactive selector."""

    snapshot_id: str
    short_id: str
    time: str
    hostname: str
    paths: tuple[str, ...]

    @staticmethod
    def format_time(timestamp: datetime) -> str:
        """Format a snapshot time for concise terminal display."""

        hour = timestamp.strftime("%I").lstrip("0")
        return (
            f"{timestamp:%B} {timestamp.day}, {timestamp.year} at "
            f"{hour}:{timestamp:%M %p %Z}"
        )

    @property
    def label(self) -> str:
        paths = ", ".join(self.paths)
        timestamp = datetime.fromisoformat(self.time).astimezone()
        display_time = self.format_time(timestamp)
        return f"{display_time}  {self.short_id}  {self.hostname}  {paths}"


class DebugRestoreOperations:
    """Restore, validate, migrate, and install a debug database."""

    def __init__(
        self,
        repository: str | None = None,
        s3_uri: str | None = None,
        aws_profile: str | None = None,
        snapshot: str | None = None,
        paths: RepoPaths | None = None,
        runner: Runner | None = None,
    ) -> None:
        self.paths = paths or RepoPaths.discover()
        self.runner = runner or Runner()
        self.repository = repository
        self.s3_uri = s3_uri
        self.aws_profile = aws_profile
        self.snapshot = snapshot
        toolchain = Toolchain.read(self.paths.toolchain)
        self.restic_image = toolchain.require_image("restic")
        self.migrator_image = toolchain.require_image("migrator")

    def validate(self) -> None:
        """Validate the selected source and prepared debug environment."""

        if (self.repository is None) == (self.s3_uri is None):
            raise ValueError("Exactly one of --repository or --s3-uri must be provided")

        if self.repository is not None:
            repository_path = Path(self.repository).expanduser().resolve()
            if not repository_path.is_dir():
                raise ValueError(
                    f"Path {repository_path} does not point to a valid Restic repository"
                )
            if not (repository_path / "config").is_file():
                raise ValueError(
                    f"Path {repository_path} does not contain a Restic repository"
                )
            self.repository = str(repository_path)

        if self.s3_uri is not None:
            self.s3_uri = self.s3_uri.strip()
            parsed = urlsplit(self.s3_uri)
            if (
                parsed.scheme != "s3"
                or not parsed.netloc
                or parsed.query
                or parsed.fragment
            ):
                raise ValueError(
                    f"Value {self.s3_uri} is not a valid S3 bucket or prefix"
                )
            if shutil.which("aws") is None:
                raise ValueError(
                    "The AWS CLI is required to download an S3 Restic repository"
                )

        if self.aws_profile is not None:
            self.aws_profile = self.aws_profile.strip()
            if not self.aws_profile:
                raise ValueError("--aws-profile cannot be empty")

        if self.snapshot is not None:
            self.snapshot = self.snapshot.strip()
            if not self.snapshot:
                raise ValueError("--snapshot cannot be empty")

        if not self.paths.debug_environment.is_file():
            raise ValueError(
                "Debug configuration is missing. Run 'ft debug create' first."
            )
        if not self.paths.debug_data.is_dir():
            raise ValueError(
                "Debug data directory is missing. Run 'ft debug create' first."
            )

    def _profile_arguments(self) -> list[str]:
        if self.aws_profile is None:
            return []
        return ["--profile", self.aws_profile]

    def run_aws(
        self,
        arguments: list[str],
        *,
        check: bool = True,
        capture_output: bool = False,
    ):
        """Run AWS CLI with the selected profile."""

        return self.runner.run(
            ["aws", *arguments, *self._profile_arguments()],
            check=check,
            capture_output=capture_output,
        )

    def ensure_aws_login(self) -> None:
        """Ensure the selected AWS profile can access AWS."""

        identity = self.run_aws(
            ["sts", "get-caller-identity"], check=False, capture_output=True
        )
        if identity.returncode == 0:
            return

        login_mode = self.run_aws(
            ["configure", "get", "sso_session"],
            check=False,
            capture_output=True,
        )
        if not login_mode.stdout.strip():
            login_mode = self.run_aws(
                ["configure", "get", "sso_start_url"],
                check=False,
                capture_output=True,
            )

        self.run_aws(["sso", "login"] if login_mode.stdout.strip() else ["login"])
        identity = self.run_aws(
            ["sts", "get-caller-identity"], check=False, capture_output=True
        )
        if identity.returncode != 0:
            raise RuntimeError(
                "AWS authentication completed but the selected profile could not access AWS"
            )

    def download_s3_repository(self, destination: Path) -> None:
        """Download the complete Restic repository from S3."""

        if self.s3_uri is None:
            raise RuntimeError("An S3 source was not configured")
        self.ensure_aws_login()
        self.run_aws(["s3", "sync", self.s3_uri, str(destination)])
        if not (destination / "config").is_file():
            raise RuntimeError(
                "The S3 source did not contain a complete Restic repository"
            )

    @staticmethod
    def get_restic_password() -> str:
        """Read the Restic password from the environment or an interactive prompt."""

        password = os.environ.get("RESTIC_PASSWORD", "")
        if password:
            return password
        password = getpass.getpass("Restic password: ")
        if not password:
            raise ValueError("A Restic password must be provided")
        return password

    @staticmethod
    def get_rollback_path(data_directory: Path) -> Path:
        """Create a unique path for preserving the current debug database."""

        descriptor, path = tempfile.mkstemp(
            prefix="database.db.before-restore-",
            suffix=f"-{uuid4().hex}.bak",
            dir=data_directory,
        )
        os.close(descriptor)
        return Path(path)

    def get_migration_environment(self) -> dict[str, str]:
        """Return guarded development authentication settings for migration."""

        values = read_dotenv(self.paths.debug_environment)
        return {
            "AUTH_MODE": "development",
            "DEVELOPMENT_AUTH_SUBJECT": values.get(
                "DEVELOPMENT_AUTH_SUBJECT", "local-developer"
            ),
            "DEVELOPMENT_AUTH_ADDITIONAL_SUBJECTS": values.get(
                "DEVELOPMENT_AUTH_ADDITIONAL_SUBJECTS",
                "local-standard,local-read-only",
            ),
            "DEVELOPMENT_AUTH_READ_ONLY_SUBJECTS": values.get(
                "DEVELOPMENT_AUTH_READ_ONLY_SUBJECTS", "local-read-only"
            ),
        }

    def list_snapshots(
        self, repository_path: Path, password: str
    ) -> list[ResticSnapshot]:
        """Return available Financial Tracker snapshots, newest first."""

        result = run_restic(
            ["snapshots", "--tag", BACKUP_TAG, "--json"],
            repository=repository_path,
            password=password,
            pass_aws_credentials=False,
            capture_output=True,
            image=self.restic_image,
            runner=self.runner,
        )
        try:
            payload = json.loads(result.stdout)
            snapshots = [
                ResticSnapshot(
                    snapshot_id=item["id"],
                    short_id=item["short_id"],
                    time=item["time"],
                    hostname=item.get("hostname", ""),
                    paths=tuple(item.get("paths", [])),
                )
                for item in payload
            ]
        except (json.JSONDecodeError, KeyError, TypeError) as error:
            raise RuntimeError("Restic returned an invalid snapshot listing") from error
        if not snapshots:
            raise RuntimeError(f"No Restic snapshots tagged {BACKUP_TAG!r} were found")
        return sorted(snapshots, key=lambda snapshot: snapshot.time, reverse=True)

    @staticmethod
    def select_snapshot(snapshots: list[ResticSnapshot]) -> str:
        """Interactively choose a snapshot and return its ID."""

        if not sys.stdin.isatty():
            raise RuntimeError(
                "Snapshot selection requires an interactive terminal; use --snapshot <id>"
            )
        while True:
            print("\nAvailable Restic snapshots (newest first):")
            for index, snapshot in enumerate(snapshots, start=1):
                print(f"  {index:>2}. {snapshot.label}")
            response = input("Select a snapshot number or q to cancel: ").strip()
            if response.casefold() == "q":
                raise KeyboardInterrupt("Snapshot selection cancelled")
            if response.isdigit():
                selected_index = int(response) - 1
                if 0 <= selected_index < len(snapshots):
                    return snapshots[selected_index].snapshot_id
            print("That snapshot number is not available.")

    def restore(self) -> None:
        """Select, restore, and atomically install a tagged debug database."""

        password = self.get_restic_password()
        with tempfile.TemporaryDirectory(
            prefix=".financial-tracker-repository-", dir=self.paths.debug_data
        ) as repository_directory:
            if self.repository is None:
                self.download_s3_repository(Path(repository_directory))
                repository_path = Path(repository_directory)
            else:
                repository_path = Path(self.repository)

            snapshot_id = getattr(self, "snapshot", None)
            if snapshot_id is None:
                snapshot_id = self.select_snapshot(
                    self.list_snapshots(repository_path, password)
                )
            print(f"Restoring Restic snapshot {snapshot_id}")

            with tempfile.TemporaryDirectory(
                prefix=".financial-tracker-restore-", dir=self.paths.debug_data
            ) as restore_directory_value:
                restore_directory = Path(restore_directory_value)
                run_restic(
                    ["check"],
                    repository=repository_path,
                    password=password,
                    pass_aws_credentials=False,
                    image=self.restic_image,
                    runner=self.runner,
                )
                run_restic(
                    [
                        "restore",
                        snapshot_id,
                        "--tag",
                        BACKUP_TAG,
                        "--target",
                        "/restore",
                        "--verify",
                    ],
                    repository=repository_path,
                    volumes=((restore_directory, "/restore", False),),
                    password=password,
                    pass_aws_credentials=False,
                    image=self.restic_image,
                    runner=self.runner,
                )

                restored_database = restore_directory / "snapshot" / "database.db"
                if not restored_database.is_file():
                    raise RuntimeError(
                        "Restic did not restore the expected database file"
                    )

                staged_database = restore_directory / "database.db"
                shutil.copy2(restored_database, staged_database)
                restore_directory.chmod(0o777)
                staged_database.chmod(0o666)
                BackupOperations.validate_database(staged_database)

                run_migrator(
                    self.migrator_image,
                    restore_directory,
                    self.get_migration_environment(),
                    runner=self.runner,
                )
                BackupOperations.validate_database(staged_database)

                rollback_path: Path | None = None
                if self.paths.debug_database.is_file():
                    rollback_path = self.get_rollback_path(self.paths.debug_data)
                    try:
                        shutil.copy2(self.paths.debug_database, rollback_path)
                    except Exception:
                        rollback_path.unlink(missing_ok=True)
                        raise

                os.replace(staged_database, self.paths.debug_database)
                self.paths.debug_database.chmod(0o666)
                if rollback_path is not None:
                    print(f"Previous debug database preserved at {rollback_path}")
