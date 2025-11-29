#!/usr/bin/env python3
"""Helper script for running the entire build pipeline"""

from backend_scripts import RestoreBackendSolution, FormatBackendSolution, BuildBackendSolution, TestBackendSolution
from frontend_scripts import InstallFrontendPackages, FormatFrontend, LintFrontend, VerifyFrontendModels, BuildFrontend
from shared.command import Command
from shared.command_collection import CommandCollection
from shared.step import Step

def main():
    """Builds and runs the command collection for this script"""

    commands = CommandCollection("Helper script for running the entire build pipeline")
    commands.commands.append(RunPipeline())
    commands.run()

class RunPipeline(Command):
    """Command class that runs the entire build pipeline"""

    def __init__(self):
        """Constructs a new instance of this class"""

        super().__init__("run", "Runs the entire build pipeline")
        
        # Backend Steps
        self.steps.append(Step("", "", lambda: RestoreBackendSolution().run([])))
        self.steps.append(Step("", "", lambda: FormatBackendSolution().run([])))
        self.steps.append(Step("", "", lambda: BuildBackendSolution().run([])))
        self.steps.append(Step("", "", lambda: TestBackendSolution().run([])))
        self.steps.append(Step("", "", lambda: TestBackendSolution().run(["--use-database"])))

        # Frontend Steps
        self.steps.append(Step("", "", lambda: InstallFrontendPackages().run([])))
        self.steps.append(Step("", "", lambda: FormatFrontend().run([])))
        self.steps.append(Step("", "", lambda: LintFrontend().run([])))
        self.steps.append(Step("", "", lambda: VerifyFrontendModels().run([])))
        self.steps.append(Step("", "", lambda: BuildFrontend().run([])))

if __name__ == "__main__":
    main()
