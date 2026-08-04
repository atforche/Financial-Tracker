"""Class representing an environment variable defined in the .env file"""

import re
from typing import Any, Generic, TypeVar

T = TypeVar("T")


class EnvironmentVariable(Generic[T]):
    """Class representing an environment variable defined in the .env file"""

    name: str
    value: T

    def __init__(self, name: str, value: T):
        """Constructs a new instance of this class

        Args:
            name (str): Name for this Environment Variable
            value (T): Value for this Environment Variable
        """

        self.name = name
        self.value = value

    @classmethod
    def read_from_file(
        cls, file_path: str, variable_types: dict[str, type]
    ) -> dict[str, EnvironmentVariable[Any]]:
        """Reads the requested environment variables from a single .env file pass.

        Args:
            file_path (str): Path to the .env file to read from
            variable_types (dict[str, type]): Expected variable names and value types
        """

        values: dict[str, EnvironmentVariable[Any]] = {}
        with open(file_path, encoding="utf-8") as file:
            for line in file:
                variable_match = re.fullmatch(r'([A-Z][A-Z0-9_]*)="(\S+)"\n?', line)
                if variable_match is None:
                    continue
                name, value = variable_match.groups()
                variable_type = variable_types.get(name)
                if variable_type is not None:
                    values[name] = cls(name, variable_type(value))

        return values
