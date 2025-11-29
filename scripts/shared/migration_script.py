"""Class representing a database migration script"""

import os
import re

class MigrationScript:
    """Class representing a database migration script"""

    directory: str = "../backend/Data/Migrations/Scripts"

    id: int
    name: str
    file_name: str

    def __init__(self, migration_id: int, name: str) -> None:
        """Constructs a new instance of this class
        
        Args:
            migration_id (int): Id for this Migration Script
            name (str): Name for this Migration Script
        """

        self.id = migration_id
        self.name = name
        self.file_name = f"{self.id} - {self.name}.sql"

    @classmethod
    def from_file_name(cls, file_name: str):
        """Constructs a new instance of this class from the provided file name
        
        Args:
            file_name (str): Name of the file for this Migration Script
        """

        match = re.match(r"(\d*) - (\w*)\.sql", file_name)
        if match is None:
            raise ValueError(f"Provided file name '{file_name}' is not a valid migration script")
        return cls(int(match.group(1)), match.group(2))

    @classmethod
    def get_all(cls):
        """Gets all the Migration Scripts currently defined in the scripts directory"""

        migrations = ([MigrationScript.from_file_name(file) for file in os.listdir(cls.directory)])
        migrations.sort(key=lambda migration: migration.id)
        return migrations
