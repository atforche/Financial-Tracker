"""Class representing an environment variable defined in the .env file"""

import re
from typing import Generic, TypeVar

T = TypeVar('T')

class EnvironmentVariable(Generic[T]):
    """Class representing an environment variable defined in the .env file"""

    file_path: str
    name: str
    value: T

    def __init__(self, file_path: str, name: str, value: T):
        """Constructs a new instance of this class
        
        Args:
            file_path (str): Path to the .env file for this Environment Variable
            name (str): Name for this Environment Variable
            value (T): Value for this Environment Variable
        """

        self.file_path = file_path
        self.name = name
        self.value = value

    def write_to_file(self):
        """Writes this Environment Variable out to the .env file, updating the existing value if it already exists"""

        with open(self.file_path, "r", encoding="utf-8") as file:
            content = file.read()

        variable_match = re.search(rf'{self.name.upper()}="(\S+)"', content)
        if variable_match is None:
            content += f'\n{self.name.upper()}="{self.value}"'
        else:
            content = re.sub(rf'{self.name.upper()}="(\S+)"', f'{self.name.upper()}="{self.value}"', content)

        with open(self.file_path, "w", encoding="utf-8") as file:
            file.write(content)

    @classmethod
    def read_from_file(cls, file_path: str, name: str, variable_type: type):
        """Reads in the provided environment variable from the provided .env file path
        
        Args:
            file_path (str): Path to the .env file to read from
            name (str): Name of the environment variable to read
            variable_type (type): Type of the variable to read
        """

        with open(file_path, "r", encoding="utf-8") as file:
            content = file.read()

        variable_match = re.search(rf'{name.upper()}="(\S+)"', content)
        if variable_match is None:
            raise RuntimeError(f"Variable {name} was not found in environment file {file_path}")

        return EnvironmentVariable(file_path, name, variable_type(variable_match.group(1)))
