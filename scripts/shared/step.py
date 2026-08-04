"""Class representing an individual step in a Command"""

from collections.abc import Callable
from typing import Any


class Step:
    """Class representing an individual step in a Command"""

    header: str
    footer: str
    func: Callable[[], Any]

    def __init__(self, header: str, footer: str, func: Callable[[], Any]) -> None:
        """Constructs a new instance of this class

        Args:
            header (str): Header for this Step
            footer (str): Footer for this Step
            func (Callable): Callback to run with this Step is run
        """

        self.header = header
        self.footer = footer
        self.func = func

    def run(self):
        """Runs this Step"""
        if self.header != "":
            print(f"{self.header:{'-'}^{100}}")
        self.func()
        if self.footer != "":
            print(self.footer)
            print()
