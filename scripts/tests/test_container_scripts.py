import shlex
from pathlib import Path

import container_scripts
import pytest


def test_prepare_smoke_test_database_provisions_the_authenticated_user(
    monkeypatch, tmp_path
):
    calls: list[tuple[str, Path, dict[str, str]]] = []

    def fake_run_migrator(image, data_directory, environment):
        calls.append((image, data_directory, environment))

    monkeypatch.setattr(container_scripts, "run_migrator", fake_run_migrator)

    container_scripts.prepare_smoke_test_database(tmp_path)

    assert calls == [
        (
            "financial-tracker-migrator:workflow",
            tmp_path,
            {
                "AUTH_MODE": "development",
                "DEVELOPMENT_AUTH_SUBJECT": "container-smoke-test",
                "DEVELOPMENT_AUTH_EMAIL": "container-smoke-test@example.test",
            },
        ),
        (
            "financial-tracker-migrator:workflow",
            tmp_path,
            {
                "AUTH_MODE": "development",
                "DEVELOPMENT_AUTH_SUBJECT": "container-smoke-test",
                "DEVELOPMENT_AUTH_EMAIL": "container-smoke-test@example.test",
            },
        ),
        (
            "financial-tracker-migrator:workflow",
            tmp_path,
            {
                "AUTH_MODE": "development",
                "DEVELOPMENT_AUTH_SUBJECT": "container-smoke-standard",
                "DEVELOPMENT_AUTH_EMAIL": "container-smoke-standard@example.test",
                "DEVELOPMENT_AUTH_ROLE": "Standard",
            },
        ),
    ]


def test_build_image_uses_docker_build_without_ci_cache(monkeypatch):
    command = container_scripts.BuildContainerImages()
    commands: list[str] = []
    monkeypatch.delenv(container_scripts.DOCKER_BUILD_CACHE, raising=False)
    monkeypatch.setattr(command, "run_subprocess", commands.append)

    command.build_image(
        context="../backend",
        tag="financial-tracker-backend:workflow",
        cache_scope="financial-tracker-backend",
    )

    assert [shlex.split(value) for value in commands] == [
        [
            "docker",
            "build",
            "--tag",
            "financial-tracker-backend:workflow",
            "../backend",
        ]
    ]


def test_build_image_uses_scoped_github_actions_cache(monkeypatch):
    command = container_scripts.BuildContainerImages()
    commands: list[str] = []
    monkeypatch.setenv(container_scripts.DOCKER_BUILD_CACHE, "gha")
    monkeypatch.setattr(command, "run_subprocess", commands.append)

    command.build_image(
        context="../backend",
        dockerfile="../backend/Migrator.Dockerfile",
        tag="financial-tracker-migrator:workflow",
        cache_scope="financial-tracker-migrator",
    )

    assert [shlex.split(value) for value in commands] == [
        [
            "docker",
            "buildx",
            "build",
            "--tag",
            "financial-tracker-migrator:workflow",
            "--file",
            "../backend/Migrator.Dockerfile",
            "--load",
            "--cache-from",
            "type=gha,scope=financial-tracker-migrator",
            "--cache-to",
            "type=gha,mode=max,ignore-error=true,scope=financial-tracker-migrator",
            "../backend",
        ]
    ]


def test_build_image_rejects_unknown_cache_backend(monkeypatch):
    command = container_scripts.BuildContainerImages()
    monkeypatch.setenv(container_scripts.DOCKER_BUILD_CACHE, "unknown")

    with pytest.raises(ValueError, match="Unsupported Docker build cache backend"):
        command.build_image(
            context="../backend",
            tag="financial-tracker-backend:workflow",
            cache_scope="financial-tracker-backend",
        )
