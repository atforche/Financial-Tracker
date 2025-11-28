"""Class representing the configuration for the Financial Tracker"""

from enum import Enum
from typing import Any
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
    backend_port: int
    frontend_port: int
    database_revision: int

    def __init__(self, name: str, path: str, environment: Environment, backend_port: int, frontend_port: int, database_revision: int) -> None:
        """Constructs a new instance of this class
        
        Args:
            name (str): Name for this instance of the Financial Tracker
            path (str): Path to the directory where this instance is located
            environment (Environment): Type of environment for this instance. Must be one of Development or Production
            backend_port (int): Port for the backend REST API to be exposed on
            frontend_port (int): Port for the frontend to be exposed on
            database_revision (int): Revision number of the database schema
        """

        self.name = name
        self.path = path
        self.environment = environment
        self.backend_port = backend_port
        self.frontend_port = frontend_port
        self.database_revision = database_revision

    def write_to_file(self) -> None:
        """Writes the current configuration to the specified file"""

        with open(self.get_environment_file_path(), "w", encoding="utf-8") as file:
            file.write(f'INSTANCE_NAME="{self.name}"\n')
            file.write(f'INSTANCE_DIR="{self.path}"\n')
            file.write(f'ENVIRONMENT="{self.environment.value}"\n')
            file.write(f'BACKEND_PORT="{self.backend_port}"\n')
            file.write(f'FRONTEND_PORT="{self.frontend_port}"\n')
            file.write(f'DATABASE_REVISION="{self.database_revision}"\n')
            file.write("\n")
            file.write(f'ASPNETCORE_ENVIRONMENT="{self.environment.value}"\n')
            file.write('ASPNETCORE_HTTP_PORTS="8080"\n')
            file.write('DATABASE_PATH="/data/database.db"\n')
            file.write(f'FRONTEND_ORIGIN="http://localhost:{self.frontend_port}"\n')
            file.write('LOG_DIRECTORY="/logs"\n')
            file.write('NGINX_PORT="8080"\n')     

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

        backend_port = EnvironmentVariable.try_read_from_file(environment_file_path, "BACKEND_PORT", int)
        if backend_port is not None:
            results["BACKEND_PORT"] = backend_port

        frontend_port = EnvironmentVariable.try_read_from_file(environment_file_path, "FRONTEND_PORT", int)
        if frontend_port is not None:
            results["FRONTEND_PORT"] = frontend_port

        database_revision = EnvironmentVariable.try_read_from_file(environment_file_path, "DATABASE_REVISION", int)
        if database_revision is not None:
            results["DATABASE_REVISION"] = database_revision

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

        existing_backend_port = existing_values.get("BACKEND_PORT")
        if existing_backend_port is None:
            backend_port = int(input("Enter the backend port: "))
        elif change_configuration:
            backend_port = int(input(f"Enter the backend port [{existing_backend_port.value}]: ") or existing_backend_port.value)
        else:
            backend_port = existing_backend_port.value

        existing_frontend_port = existing_values.get("FRONTEND_PORT")
        if existing_frontend_port is None:
            frontend_port = int(input("Enter the frontend port: "))
        elif change_configuration:
            frontend_port = int(input(f"Enter the frontend port [{existing_frontend_port.value}]: ") or existing_frontend_port.value)
        else:
            frontend_port = existing_frontend_port.value

        existing_database_revision = existing_values.get("DATABASE_REVISION")
        if existing_database_revision is None:
            database_revision = 0
        else:
            database_revision = existing_database_revision.value

        return Configuration(name, path, environment, backend_port, frontend_port, database_revision)
