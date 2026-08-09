"""Stable paths used by repository commands."""

from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path


@dataclass(frozen=True)
class RepoPaths:
    """Filesystem locations relative to the repository root."""

    root: Path

    @classmethod
    def discover(cls) -> RepoPaths:
        """Discover the repository root from this installed source tree."""

        return cls(Path(__file__).resolve().parents[2])

    @property
    def orchestrator(self) -> Path:
        return self.root / "orchestrator"

    @property
    def backend(self) -> Path:
        return self.root / "backend"

    @property
    def frontend(self) -> Path:
        return self.root / "frontend"

    @property
    def debug(self) -> Path:
        return self.root / "debug"

    @property
    def debug_data(self) -> Path:
        return self.debug / "data"

    @property
    def debug_database(self) -> Path:
        return self.debug_data / "database.db"

    @property
    def debug_logs(self) -> Path:
        return self.debug / "logs"

    @property
    def compose_dev(self) -> Path:
        return self.root / "compose.dev.yaml"

    @property
    def artifacts(self) -> Path:
        return self.root / ".artifacts"

    @property
    def backend_artifacts(self) -> Path:
        return self.backend / ".artifacts"

    @property
    def backend_solution(self) -> Path:
        return self.backend / "Backend.sln"

    @property
    def backend_project(self) -> Path:
        return self.backend / "Rest" / "Rest.csproj"

    @property
    def backend_dockerfile(self) -> Path:
        return self.backend / "Dockerfile"

    @property
    def migrator_dockerfile(self) -> Path:
        return self.backend / "Migrator.Dockerfile"

    @property
    def frontend_dockerfile(self) -> Path:
        return self.frontend / "Dockerfile"

    @property
    def backend_data_project(self) -> Path:
        return self.backend / "Data" / "Data.csproj"

    @property
    def backend_data_artifacts(self) -> Path:
        return self.backend_artifacts / "obj" / "Data"

    @property
    def backend_test_results(self) -> Path:
        return self.backend_artifacts / "TestResults"

    @property
    def openapi_contract(self) -> Path:
        return self.backend_artifacts / "obj" / "Rest" / "Financial-Tracker-API.json"

    @property
    def frontend_api_models(self) -> Path:
        return self.frontend / "framework" / "data" / "api.ts"

    @property
    def migrator_project(self) -> Path:
        return self.backend / "Migrator" / "Migrator.csproj"

    @property
    def dotnet_tool_manifest(self) -> Path:
        return self.root / ".config" / "dotnet-tools.json"

    @property
    def debug_environment(self) -> Path:
        return self.debug / ".env"

    @property
    def environment_schema(self) -> Path:
        return self.root / "config" / "environment.toml"

    @property
    def toolchain(self) -> Path:
        return self.root / "config" / "toolchain.toml"

    @property
    def production_template(self) -> Path:
        return self.root / ".env.example"

    @property
    def pyproject(self) -> Path:
        return self.root / "pyproject.toml"

    @property
    def python_environment(self) -> Path:
        return self.root / ".venv"

    @property
    def local_tools(self) -> Path:
        return self.root / ".tools"

    @property
    def trivy(self) -> Path:
        return self.local_tools / "trivy"

    @property
    def quality_python(self) -> Path:
        return self.python_environment / "bin" / "python"

    def require(self, path: Path, description: str) -> Path:
        """Return a path or raise a useful repository error."""

        if not path.exists():
            raise FileNotFoundError(f"{description} does not exist: {path}")
        return path
