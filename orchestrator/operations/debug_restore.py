"""Restore an encrypted production backup into the native debug database."""

from __future__ import annotations

import getpass
import os
import shutil
import tempfile
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


class DebugRestoreOperations:
    """Restore, validate, migrate, and install a debug database."""

    def __init__(
        self,
        repository: str | None = None,
        s3_uri: str | None = None,
        aws_profile: str | None = None,
        paths: RepoPaths | None = None,
        runner: Runner | None = None,
    ) -> None:
        self.paths = paths or RepoPaths.discover()
        self.runner = runner or Runner()
        self.repository = repository
        self.s3_uri = s3_uri
        self.aws_profile = aws_profile
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

    def restore(self) -> None:
        """Restore and atomically install the latest tagged debug database."""

        password = self.get_restic_password()
        with tempfile.TemporaryDirectory(
            prefix=".financial-tracker-repository-", dir=self.paths.debug_data
        ) as repository_directory:
            if self.repository is None:
                self.download_s3_repository(Path(repository_directory))
                repository_path = Path(repository_directory)
            else:
                repository_path = Path(self.repository)

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
                        "latest",
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
