using Domain.Payroll.Withholding;

namespace Domain.AccountingPeriods;

/// <summary>
/// A state election paired with the exact published rules used to calculate it.
/// </summary>
public sealed record StateWithholdingSelection(
    StateWithholdingElection Election,
    WithholdingRuleSet RuleSet)
{
    internal StateWithholdingSelection Snapshot() => new(Election.Snapshot(), RuleSet.Snapshot());
}