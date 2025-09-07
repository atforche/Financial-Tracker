"""Class representing a collection of commands that can be run from the command line"""

import sys
from typing import List
from .argument import Argument
from .command import Command

class CommandCollection:
    """Class representing a collection of commands that can be run from the command line"""

    def __init__(self, description: str) -> None:
        """Constructs a new instance of this class
        
        Args:
            description (str): Description of this command collection
        """

        self.commands: List[Command] = []
        self.help_argument = Argument[bool]("help", "Displays the help message for this program")
        self.description = description

    def run(self) -> None:
        """Parses the command line arguments and runs the specified command"""

        if len(sys.argv) == 1:
            self.print_usage()
            return

        command_string = sys.argv[1]
        if self.help_argument.try_parse([command_string]) is True:
            self.print_usage()
            return

        for command in self.commands:
            if command.name == command_string:
                command.run(sys.argv[2:])
                return

        raise ValueError(f"Unexpected command provided: '{command_string}'")

    def print_usage(self) -> None:
        """Prints a help message with usage information for this command collection"""

        command_usage = f"{{{', '.join([command.name for command in self.commands])}}}"
        print(f"usage: {sys.argv[0]} [{self.help_argument.get_usage()}] {command_usage}")
        print()
        print(self.description)
        print()
        print("arguments:")
        print(f"{self.help_argument.get_usage():<25} {self.help_argument.description}")
        for command in self.commands:
            print(f"{command.name:<25} {command.description}")
