#!/usr/bin/env python3
import abc
import argparse
import subprocess
import sys

def main():
    # Set up the main argument parser
    parser = argparse.ArgumentParser(
        prog="scripts.py", 
        description="Helper scripts for managing the Financial Tracker backend")
    subparsers = parser.add_subparsers()

    # Set up a subparser for each subcommand
    setup_restore_solution_command(subparsers)
    setup_format_command(subparsers)
    setup_build_solution_command(subparsers)
    setup_test_solution_command(subparsers)
    setup_run_pipeline(parser, subparsers)
    setup_create_new_migration_command(subparsers)
    setup_apply_migrations_command(subparsers)

    # Parse the args and run the default function
    args = parser.parse_args(sys.argv[1:])
    args.func(args)

def setup_restore_solution_command(subparsers):
    def restore_solution(args):
        print("Restoring solution")
        run_command("dotnet restore Backend.sln")
        print("Restore completed\n")

    restore_parser = subparsers.add_parser("restore",  help="Runs a Nuget restore on the backend solution")
    restore_parser.set_defaults(func=restore_solution)

def setup_format_command(subparsers):
    def format_solution(args):
        print("Formatting solution")
        run_command("dotnet format Backend.sln --verify-no-changes --no-restore --severity info")
        print("Format completed\n")

    parser = subparsers.add_parser("format", help="Runs dotnet formatting on the backend solution")
    parser.set_defaults(func=format_solution)

def setup_build_solution_command(subparsers):
    def build_solution(args):
        print("Building solution")
        run_command("dotnet build Backend.sln --no-restore")
        print("Build completed\n")

    parser = subparsers.add_parser("build", help="Builds the backend solution")
    parser.set_defaults(func=build_solution)

def setup_test_solution_command(subparsers):
    def test_solution(args):
        print("Running database integration tests" if args.use_database else "Running unit tests")
        run_command(f"dotnet test Backend.sln --no-build --verbosity normal {'--environment USE_DATABASE=TRUE' if args.use_database else ''}")
        print("Unit tests completed\n")

    parser = subparsers.add_parser("test", help="Runs the unit tests on the backend solution")
    parser.add_argument("--use-database", action="store_true", help="Runs the unit test against an actual database")
    parser.set_defaults(func=test_solution)

def setup_run_pipeline(parser, subparsers):
    def run_pipeline(args):
        args = parser.parse_args(["restore"])
        args.func(args)

        args = parser.parse_args(["format"])
        args.func(args)

        args = parser.parse_args(["build"])
        args.func(args)

        args = parser.parse_args(["test"])
        args.func(args)

        args = parser.parse_args(["test", "--use-database"])
        args.func(args)

    subparser = subparsers.add_parser("run-pipeline", help="Runs the full build pipeline for the backend solution")
    subparser.set_defaults(func=run_pipeline)

def setup_create_new_migration_command(subparsers):
    def create_new_migration(args):
        print("Creating new migration")
        run_command(f"dotnet ef migrations add {args.name} --msbuildprojectextensionspath ../.artifacts/obj/Data")
        print("New migration created\n")
    
    parser = subparsers.add_parser("create-new-migration", help="Creates a new EF core migration for the backend solution")
    parser.add_argument("name", help="Name for the new EF core migration")
    parser.set_defaults(func=create_new_migration)

def setup_apply_migrations_command(subparsers):
    def apply_migrations(args):
        print("Applying database migrations")
        run_command("dotnet ef database update")
        print("Database migrations applied")

    parser = subparsers.add_parser("apply-migrations", help="Applies the EF core migrations to the database")
    parser.set_defaults(func=apply_migrations)

def run_command(command_string):
    result = subprocess.run(command_string.split())
    if (result.returncode != 0):
        raise Exception("Command ended with error")

if __name__ == "__main__":
    main()