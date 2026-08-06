from types import SimpleNamespace

import deploy_scripts


def test_apply_migrations_passes_the_configured_bootstrap_administrator_to_the_migrator(
    monkeypatch, tmp_path
):
    instance_path = tmp_path / "instance"
    instance_path.mkdir()
    database_path = instance_path / "database.db"
    database_path.touch()
    captured: dict[str, object] = {}

    def fake_run_migrator(image, data_directory, environment):
        captured["image"] = image
        captured["data_directory"] = data_directory
        captured["environment"] = environment

    monkeypatch.setattr(deploy_scripts, "run_migrator", fake_run_migrator)
    configuration = SimpleNamespace(
        path=str(instance_path),
        migrator_image="registry.example/migrator@sha256:abc",
        get_database_file_path=lambda: str(database_path),
    )

    deploy_scripts.ApplyMigrations(configuration, "owner@example.com").run([])

    assert captured["image"] == "registry.example/migrator@sha256:abc"
    assert captured["environment"] == {"BOOTSTRAP_ADMIN_EMAIL": "owner@example.com"}


def test_deploy_command_accepts_a_missing_instance_path(tmp_path):
    release_directory = tmp_path / "release"
    release_directory.mkdir()
    manifest_path = release_directory / "release-manifest.json"
    manifest_path.touch()
    (release_directory / "compose.yaml").touch()
    (release_directory / "Caddyfile").touch()
    command = deploy_scripts.DeployCommand()
    command.path = str(tmp_path / "instance")
    command.release_manifest = str(manifest_path)

    command.validate_arguments()
