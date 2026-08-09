from orchestrator.cli import build_parser


def test_cli_registers_leaf_commands():
    parser = build_parser()

    assert parser.parse_args(["debug", "stack-up"]).handler is not None
    assert (
        parser.parse_args(["backend", "create-migration", "initial"]).name == "initial"
    )
    assert (
        parser.parse_args(["env", "validate", "--profile", "debug"]).profile == "debug"
    )
