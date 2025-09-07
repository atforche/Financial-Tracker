"""Class representing a standalone command used in a script"""

from inspect import get_annotations
import subprocess
import sys
from typing import Any, List, get_args
from .argument import Argument
from .step import Step

class Command:
    """Class representing a standalone command used in a script"""

    name: str
    description: str
    steps: List[Step]

    def __init__(self, name: str, description: str):
        """Constructs a new instance of this class
        
        Args:
            name (str): Name for this command
        """

        self.name = name
        self.description = description
        self.steps = []
        self.validate_name()

    def run(self, arg_list: List[str]):
        """Run this command.
        
        Args:
            arg_list (List[str]): List of command list arguments provided to the program
        """

        for argument in self.build_argument_collection():
            if argument.name == "--help":
                if argument.try_parse(arg_list) is True:
                    self.print_usage()
                    return
                continue

            value = argument.parse(arg_list)
            setattr(self, argument.property_name, value)

        if len(arg_list) > 0:
            raise ValueError(f"Unexpected arguments provided: '{', '.join(arg_list)}'")

        self.validate_arguments()
        for step in self.steps:
            step.run()

    def validate_arguments(self):
        """Runs additional validation on the provided arguments"""

    def build_argument_collection(self) -> List[Argument[Any]]:
        """Builds the list of Arguments that this Command exposes"""

        results: List[Argument[Any]] = [Argument[bool]("help", "Displays the help message for this command")]

        for property_name, property_annotations in get_annotations(type(self)).items():
            annotations = get_args(property_annotations)
            results.append(Argument[annotations[0]](property_name, annotations[1]))

        return results

    def print_usage(self):
        """Prints a help message for this command"""

        arguments: List[Argument[Any]] = self.build_argument_collection()
        argument_usage = [f"[{argument.get_usage()}]" for argument in arguments]
        print(f"usage: {sys.argv[0]} {self.name} {' '.join(argument_usage)}")
        print()
        print(self.description)
        if len(arguments) > 0:
            print()
            print("arguments:")
            for argument in self.build_argument_collection():
                print(f"{argument.get_usage():<25} {argument.description}")

    def run_subprocess(self, command_string: str, process_input: str = "", throw_on_error: bool = True, suppress_output: bool = False) -> int:
        """Runs the provided command as a subprocess. An exception is raised if the command ends in error.
        
        Args:
            command_string (str): Command to be run
            process_input (str): Input string to pipe into the process
            throw_on_error (bool): True if an exception should be thrown if the command ends in error, false otherwise
            suppress_output (bool): True if output should be suppressed, false otherwise

        Returns:
            int: The return code from the subprocess
        """

        print(f"Running subprocess: '{command_string}'")
        result = subprocess.run(command_string.split(), text=True, input=process_input, check=False, stdout=subprocess.DEVNULL if suppress_output else None)
        if throw_on_error and result.returncode != 0:
            raise RuntimeError("Command ended with error")
        return result.returncode

    def validate_name(self) -> None:
        """Validates the name for this Command"""

        if self.name == "":
            raise ValueError("A command cannot have a blank name")
        if ' ' in self.name or '_' in self.name:
            raise ValueError("A command name cannot contain a space or underscore")
