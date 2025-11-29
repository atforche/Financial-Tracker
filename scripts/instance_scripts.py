#!/usr/bin/env python3
"""Helper scripts for managing an instance of the Financial Tracker"""

import os
import pathlib
import shutil
from shared.configuration import Configuration
from shared.command import Command
from shared.command_collection import CommandCollection
from shared.step import Step

def main():
    """Builds and runs the command collection for this script"""

    commands = CommandCollection("Helper scripts for managing an instance of the Financial Tracker")
    commands.commands.append(StartCommand())
    commands.commands.append(StopCommand())
    commands.commands.append(DestroyCommand())
    commands.run()

class StartCommand(Command):
    """Command class that starts this instance of the Financial Tracker"""

    def __init__(self) -> None:
        """Constructs a new instance of this class"""

        super().__init__("start", "Starts this instance of the Financial Tracker")
        self.steps.append(Step("Start Instance", "Instance started", lambda: self.run_subprocess(f"docker compose -f {os.path.dirname(__file__)}/../compose.yaml up -d")))

class StopCommand(Command):
    """Command class that stops this instance of the Financial Tracker"""

    def __init__(self) -> None:
        """Constructs a new instance of this class"""

        super().__init__("stop", "Stops this instance of the Financial Tracker")
        self.steps.append(Step("Stop Instance", "Instance stopped", lambda: self.stop_instance(f"{os.path.dirname(__file__)}/../compose.yaml")))

    def stop_instance(self, compose_file_path: str):
        """Stops an instance of the Financial Tracker
        
        Args:
            compose_file_path (str): Path to the docker compose file for the instance to stop
        """

        self.run_subprocess(f"docker compose -f {compose_file_path} down")

class DestroyCommand(Command):
    """Command class that destroys this instance of the Financial Tracker"""

    configuration: Configuration

    def __init__(self) -> None:
        """Constructs a new instance of this class"""

        super().__init__("destroy", "Destroys this instance of the Financial Tracker")
        self.configuration = Configuration.build_from_existing_instance(f"{os.path.dirname(__file__)}/..", False)

        self.steps.append(Step("", "", self.verify))
        self.steps.append(Step("", "", lambda: StopCommand().run([])))
        self.steps.append(Step("Destroy Instance", "Instance destroyed", self.destroy))

    def verify(self) -> None:
        """Verifies that the user intends to use this command"""

        if os.geteuid() != 0:
            raise PermissionError("This command must be run as root")

        print("WARNING: This command will completely destroy this instance. All data will be lost and unrecoverable. " +
              "Please type the name of this instance to confirm your intent.")
        confirmation = input()

        if self.configuration.name.upper() != confirmation.upper():
            raise RuntimeError(f"Confirmation did not match. Expected '{self.configuration.name}'")

    def destroy(self) -> None:
        """Destroys this instance of the Financial Tracker"""

        print(f"Destroying the '{self.configuration.name}' of the Financial Tracker")
        self.run_subprocess(f"docker image rm backend-{self.configuration.name}")
        self.run_subprocess(f"docker image rm frontend-{self.configuration.name}")
        shutil.rmtree(pathlib.Path(__file__).parent.parent.resolve())

if __name__ == "__main__":
    main()
