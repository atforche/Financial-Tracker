namespace Models.AccountingPeriods;

/// <summary>
/// Model representing counts of an Accounting Period's Fund Goals and Account Goals by their progress.
/// </summary>
public class AccountingPeriodGoalSummaryModel
{
    /// <summary>
    /// The number of Fund Goals.
    /// </summary>
    public required int FundGoalCount { get; init; }

    /// <summary>
    /// The number of Fund Goals with a planned contribution.
    /// </summary>
    public required int ContributionGoalCount { get; init; }

    /// <summary>
    /// The number of Fund Goals whose expected contribution has been assigned.
    /// </summary>
    public required int ContributedGoalCount { get; init; }

    /// <summary>
    /// The number of Fund Goals whose ending balance is below their minimum.
    /// </summary>
    public required int FundGoalsBelowMinimumCount { get; init; }

    /// <summary>
    /// The number of Fund Goals whose ending balance is above their maximum.
    /// </summary>
    public required int FundGoalsAboveMaximumCount { get; init; }

    /// <summary>
    /// The number of Account Goals.
    /// </summary>
    public required int AccountGoalCount { get; init; }

    /// <summary>
    /// The number of Account Goals whose ending balance is below their minimum.
    /// </summary>
    public required int AccountGoalsBelowMinimumCount { get; init; }

    /// <summary>
    /// The number of Account Goals whose ending balance is above their maximum.
    /// </summary>
    public required int AccountGoalsAboveMaximumCount { get; init; }
}
