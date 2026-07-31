"""Class representing the configuration for the Financial Tracker"""

from enum import Enum
import secrets
from typing import Any
from urllib.parse import urlsplit
from shared.environment_variable import EnvironmentVariable

class Environment(Enum):
    """Enum class representing the different environments"""

    DEVELOPMENT = "Development"
    PRODUCTION = "Production"

class Configuration:
    """Class representing the configuration for the Financial Tracker"""

    name: str
    path: str
    environment: Environment
    database_revision: int
    public_origin: str
    google_client_id: str
    google_client_secret: str
    google_allowed_subjects: str
    auth_secret: str
    backend_image: str
    frontend_image: str

    def __init__(self, name: str, path: str, environment: Environment, database_revision: int, public_origin: str,
                 google_client_id: str, google_client_secret: str,
                 google_allowed_subjects: str, auth_secret: str,
                 backend_image: str, frontend_image: str) -> None:
        """Constructs a new instance of this class
        
        Args:
            name (str): Name for this instance of the Financial Tracker
            path (str): Path to the directory where this instance is located
            environment (Environment): Type of environment for this instance. Must be one of Development or Production
            database_revision (int): Revision number of the database schema
            public_origin (str): Public HTTPS origin used to access the instance
            google_client_id (str): Client ID from the Google OpenID Connect application registration
            google_client_secret (str): Client secret from the Google OpenID Connect application registration
            google_allowed_subjects (str): Comma-separated immutable Google subjects allowed to access the instance
            auth_secret (str): Secret used to encrypt Auth.js sessions
            backend_image (str): Container image reference for the backend
            frontend_image (str): Container image reference for the frontend
        """

        self.name = name
        self.path = path
        self.environment = environment
        self.database_revision = database_revision
        self.public_origin = public_origin
        self.google_client_id = google_client_id
        self.google_client_secret = google_client_secret
        self.google_allowed_subjects = google_allowed_subjects
        self.auth_secret = auth_secret
        self.backend_image = backend_image
        self.frontend_image = frontend_image

    def write_to_file(self) -> None:
        """Writes the current configuration to the specified file"""

        public_host = self.get_public_host()
        public_port = self.get_public_port()

        with open(self.get_environment_file_path(), "w", encoding="utf-8") as file:
            file.write(f'INSTANCE_NAME="{self.name}"\n')
            file.write(f'INSTANCE_DIR="{self.path}"\n')
            file.write(f'ENVIRONMENT="{self.environment.value}"\n')
            file.write(f'PUBLIC_ORIGIN="{self.public_origin}"\n')
            file.write(f'PUBLIC_HOST="{public_host}"\n')
            file.write(f'PUBLIC_PORT="{public_port}"\n')
            file.write(f'DATABASE_REVISION="{self.database_revision}"\n')
            file.write("\n")
            file.write(f'BACKEND_IMAGE="{self.backend_image}"\n')
            file.write(f'FRONTEND_IMAGE="{self.frontend_image}"\n')
            file.write('\n')
            file.write(f'GOOGLE_CLIENT_ID="{self.google_client_id}"\n')
            file.write(f'GOOGLE_CLIENT_SECRET="{self.google_client_secret}"\n')
            file.write(f'GOOGLE_ALLOWED_SUBJECTS="{self.google_allowed_subjects}"\n')
            file.write('\n')
            file.write(f'AUTH_SECRET="{self.auth_secret}"\n')

    def get_public_host(self) -> str:
        """Returns the hostname from the public HTTPS origin"""

        parsed_origin = urlsplit(self.public_origin)
        if parsed_origin.scheme != "https" or parsed_origin.hostname is None:
            raise ValueError("PUBLIC_ORIGIN must be an HTTPS origin with a hostname")

        return parsed_origin.hostname

    def get_public_port(self) -> int:
        """Returns the HTTPS port from the public origin"""

        parsed_origin = urlsplit(self.public_origin)
        if parsed_origin.scheme != "https" or parsed_origin.hostname is None:
            raise ValueError("PUBLIC_ORIGIN must be an HTTPS origin with a hostname")

        try:
            return parsed_origin.port or 443
        except ValueError as error:
            raise ValueError("PUBLIC_ORIGIN must use a valid HTTPS port") from error

    def get_database_file_path(self) -> str:
        """Returns the path to the database file"""

        return f"{self.path}/database.db"

    def get_environment_file_path(self) -> str:
        """Returns the path to the environment file"""

        return f"{self.path}/.env"

    def get_compose_file_path(self) -> str:
        """Returns the path to the Docker compose file"""

        return f"{self.path}/compose.yaml"

    def get_scripts_directory_path(self) -> str:
        """Returns the path to the scripts directory"""

        return f"{self.path}/scripts"

    @classmethod
    def build_from_existing_instance(cls, instance_path: str, change_configuration: bool):
        """Constructs a new instance of this class from the provided file name
        
        Args:
            file_path (str): The path of the file to build the configuration from
            change_configuration (bool): True to prompt to overwrite existing configuration values, false otherwise
        """

        results: dict[str, EnvironmentVariable[Any]] = {}
        environment_file_path = f"{instance_path}/.env"

        name = EnvironmentVariable.try_read_from_file(environment_file_path, "INSTANCE_NAME", str)
        if name is not None:
            results["INSTANCE_NAME"] = name

        path = EnvironmentVariable.try_read_from_file(environment_file_path, "INSTANCE_DIR", str)
        if path is not None:
            results["INSTANCE_DIR"] = path

        environment = EnvironmentVariable.try_read_from_file(environment_file_path, "ENVIRONMENT", Environment)
        if environment is not None:
            results["ENVIRONMENT"] = environment

        database_revision = EnvironmentVariable.try_read_from_file(environment_file_path, "DATABASE_REVISION", int)
        if database_revision is not None:
            results["DATABASE_REVISION"] = database_revision

        public_origin = EnvironmentVariable.try_read_from_file(environment_file_path, "PUBLIC_ORIGIN", str)
        if public_origin is None:
            public_domain = EnvironmentVariable.try_read_from_file(environment_file_path, "PUBLIC_DOMAIN", str)
            if public_domain is not None:
                public_origin = EnvironmentVariable(
                    environment_file_path, "PUBLIC_ORIGIN", f"https://{public_domain.value}")

        if public_origin is not None:
            results["PUBLIC_ORIGIN"] = public_origin

        google_client_id = EnvironmentVariable.try_read_from_file(environment_file_path, "GOOGLE_CLIENT_ID", str)
        if google_client_id is not None:
            results["GOOGLE_CLIENT_ID"] = google_client_id

        google_client_secret = EnvironmentVariable.try_read_from_file(environment_file_path, "GOOGLE_CLIENT_SECRET", str)
        if google_client_secret is not None:
            results["GOOGLE_CLIENT_SECRET"] = google_client_secret

        google_allowed_subjects = EnvironmentVariable.try_read_from_file(environment_file_path, "GOOGLE_ALLOWED_SUBJECTS", str)
        if google_allowed_subjects is not None:
            results["GOOGLE_ALLOWED_SUBJECTS"] = google_allowed_subjects
        else:
            google_allowed_subject = EnvironmentVariable.try_read_from_file(environment_file_path, "GOOGLE_ALLOWED_SUBJECT", str)
            if google_allowed_subject is not None:
                results["GOOGLE_ALLOWED_SUBJECTS"] = EnvironmentVariable(
                    environment_file_path, "GOOGLE_ALLOWED_SUBJECTS", google_allowed_subject.value)

        auth_secret = EnvironmentVariable.try_read_from_file(environment_file_path, "AUTH_SECRET", str)
        if auth_secret is not None:
            results["AUTH_SECRET"] = auth_secret

        backend_image = EnvironmentVariable.try_read_from_file(environment_file_path, "BACKEND_IMAGE", str)
        if backend_image is not None:
            results["BACKEND_IMAGE"] = backend_image

        frontend_image = EnvironmentVariable.try_read_from_file(environment_file_path, "FRONTEND_IMAGE", str)
        if frontend_image is not None:
            results["FRONTEND_IMAGE"] = frontend_image

        return Configuration.build_from_user_input(results, change_configuration)

    @classmethod
    def build_from_user_input(cls, existing_values: dict[str, EnvironmentVariable[Any]], change_configuration: bool):
        """Constructs a new instance of this class from user input

        Args:
            existing_values (dict[str, EnvironmentVariable[Any]]): Existing environment variable values to use as defaults
            change_configuration (bool): True to prompt to overwrite existing configuration values, false otherwise
        """

        existing_name = existing_values.get("INSTANCE_NAME")
        if existing_name is None:
            name = input("Enter the instance name: ")
        elif change_configuration:
            name = input(f"Enter the instance name [{existing_name.value}]: ") or existing_name.value
        else:
            name = existing_name.value

        existing_path = existing_values.get("INSTANCE_DIR")
        if existing_path is None:
            path = input("Enter the instance path: ")
        elif change_configuration:
            path = input(f"Enter the instance path [{existing_path.value}]: ") or existing_path.value
        else:
            path = existing_path.value

        existing_environment = existing_values.get("ENVIRONMENT")
        if existing_environment is None:
            environment = Environment(input("Enter the environment (Development/Production): "))
        elif change_configuration:
            environment = Environment(input(f"Enter the environment (Development/Production) [{existing_environment.value}]: ") or existing_environment.value)
        else:
            environment = existing_environment.value

        existing_database_revision = existing_values.get("DATABASE_REVISION")
        if existing_database_revision is None:
            database_revision = 0
        else:
            database_revision = existing_database_revision.value

        public_origin = cls.get_required_string(existing_values, "PUBLIC_ORIGIN", "public HTTPS origin", change_configuration)

        google_client_id = cls.get_required_string(existing_values, "GOOGLE_CLIENT_ID", "Google OAuth client ID", change_configuration)
        google_client_secret = cls.get_required_string(existing_values, "GOOGLE_CLIENT_SECRET", "Google OAuth client secret", change_configuration, True)
        google_allowed_subjects = cls.get_required_string(existing_values, "GOOGLE_ALLOWED_SUBJECTS", "comma-separated Google subjects allowed to access this instance", change_configuration)
        auth_secret = cls.get_auth_secret(existing_values)
        existing_backend_image = existing_values.get("BACKEND_IMAGE")
        backend_image = existing_backend_image.value if existing_backend_image is not None else f"backend-{name}"
        existing_frontend_image = existing_values.get("FRONTEND_IMAGE")
        frontend_image = existing_frontend_image.value if existing_frontend_image is not None else f"frontend-{name}"

        return Configuration(name, path, environment, database_revision, public_origin,
                             google_client_id, google_client_secret, google_allowed_subjects, auth_secret,
                             backend_image, frontend_image)

    @staticmethod
    def get_required_string(existing_values: dict[str, EnvironmentVariable[Any]], name: str, prompt: str,
                            change_configuration: bool, secret: bool = False) -> str:
        """Gets a required string value, preserving an existing secret unless it is replaced."""

        existing_value = existing_values.get(name)
        if existing_value is None:
            value = input(f"Enter the {prompt}: ")
        elif change_configuration:
            default = "configured" if secret else existing_value.value
            value = input(f"Enter the {prompt} [{default}]: ") or existing_value.value
        else:
            value = existing_value.value

        if value.strip() == "":
            raise ValueError(f"{name} must not be empty")
        return value

    @staticmethod
    def get_auth_secret(existing_values: dict[str, EnvironmentVariable[Any]]) -> str:
        """Returns the existing Auth.js secret or securely generates one for a new instance."""

        existing_secret = existing_values.get("AUTH_SECRET")
        if existing_secret is not None:
            return existing_secret.value

        print("Generating a new Auth.js session secret")
        return secrets.token_urlsafe(48)
