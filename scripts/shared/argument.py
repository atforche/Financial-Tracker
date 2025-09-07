"""Class representing an argument that can be passed in from the command lines"""

from typing import Generic, List, TypeVar, Union

T = TypeVar('T')

class Argument(Generic[T]):
    """Class representing an argument that can be passed in from the command line"""

    name: str
    property_name: str
    description: str

    def __init__(self, property_name: str, description: str) -> None:
        """Constructs a new instance of this class
        
        Args:
            property_name (str): Name of the property corresponding to this Argument
            description (str): Description of the property corresponding to this Argument
        """

        self.property_name = property_name
        self.name = f"--{property_name.replace('_', '-')}"
        self.description = description
        self.validate_name()

    def get_usage(self) -> str:
        """Gets the usage string for this Argument"""

        if self.get_generic_type_parameter() is bool:
            return f"{self.name}"
        return f"{self.name} {self.get_generic_type_parameter().__name__.upper()}"

    def parse(self, arg_list: List[str]) -> T:
        """Parses this argument from the provided argument list. An exception is thrown if the argument cannot be found or is invalid
        
        Args:
            arg_list (List[str]): List of command list arguments provided to the program

        Returns:
            T: The value provided for this argument
        """

        result = self.try_parse(arg_list)
        if isinstance(result, Exception):
            raise result
        return result

    def try_parse(self, arg_list: List[str]) -> Union[T, Exception]:
        """Attempts to parse this argument from the provided argument list. An exception is returned if the argument cannot be found or isn't valid
        
        Args:
            arg_list (List[str]): List of command list arguments provided to the program

        Returns:
            Union[T, Exception]: The value provided for this argument or an exception if the argument cannot be parsed
        """

        if not arg_list or not self.name in arg_list:
            if self.get_generic_type_parameter() is bool:
                # If a boolean argument isn't provided, just return the value as being false
                return False # type: ignore
            else:
                return Exception(f"Required argument '{self.name}' was not provided")

        if arg_list.count(self.name) > 1:
            return Exception(f"Multiple instances of argument '{self.name}' were provided")

        argument_index: int = arg_list.index(self.name)

        if self.get_generic_type_parameter() is bool:
            del arg_list[argument_index]
            return True # type: ignore

        if argument_index == len(arg_list) - 1:
            return Exception(f"Value was not provided for argument '{self.name}'")

        argument_value = arg_list.pop(argument_index + 1)
        del arg_list[argument_index]
        return self.get_generic_type_parameter()(argument_value)

    def get_generic_type_parameter(self) -> type:
        """Gets the generic type parameter used for this Argument"""

        return self.__orig_class__.__args__[0] # type: ignore # pylint: disable=maybe-no-member

    def validate_name(self) -> None:
        """Validates the name for this Argument"""

        if self.name == "":
            raise ValueError("An argument cannot have a blank name")
        if self.name[:2] != "--":
            raise ValueError("An argument name must start with '--'")
        if ' ' in self.name or '_' in self.name:
            raise ValueError("An argument name cannot contain a space or underscore")
