using Domain.Payroll.Withholding;

namespace Domain.AccountingPeriods;

/// <summary>
/// Withholding inputs snapshotted with expected payroll income.
/// </summary>
public sealed record PayrollWithholdingConfiguration
{
    /// <summary>
    /// Federal withholding election.
    /// </summary>
    public required FederalWithholdingElection FederalElection { get; init; }

    /// <summary>
    /// Published federal rules used for the estimate.
    /// </summary>
    public required WithholdingRuleSet FederalRuleSet { get; init; }

    /// <summary>
    /// State elections and their published rule versions.
    /// </summary>
    public required IReadOnlyCollection<StateWithholdingSelection> StateSelections { get; init; }

    internal PayrollWithholdingConfiguration Snapshot() => new()
    {
        FederalElection = FederalElection with { },
        FederalRuleSet = FederalRuleSet.Snapshot(),
        StateSelections = StateSelections.Select(selection => selection.Snapshot()).ToList().AsReadOnly(),
    };
}