"""Restore a production backup into the native debug database."""

from argparse import ArgumentParser, Namespace

from ..core.context import Context
from ..operations.debug_restore import DebugRestoreOperations


def configure(parser: ArgumentParser) -> None:
    sources = parser.add_mutually_exclusive_group(required=True)
    sources.add_argument("--repository", help="Path to a local Restic repository")
    sources.add_argument("--s3-uri", help="S3 bucket or prefix containing a Restic repository")
    parser.add_argument("--aws-profile", help="AWS CLI profile used for an S3 download")


def run(context: Context, args: Namespace) -> int:
    operations = DebugRestoreOperations(
        repository=args.repository,
        s3_uri=args.s3_uri,
        aws_profile=args.aws_profile,
        paths=context.paths,
        runner=context.runner,
    )
    operations.validate()
    operations.restore()
    return 0
