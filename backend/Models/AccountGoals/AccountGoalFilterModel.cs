namespace Models.AccountGoals;

/// <summary>
/// Filters Account Goals by related resources.
/// </summary>
public sealed class AccountGoalFilterModel
{
    /// <summary>
    /// Gets the Account IDs to include.
    /// </summary>
    public IReadOnlyCollection<Guid>? AccountIds { get; init; }

    /// <summary>
    /// Gets the Accounting Period IDs to include.
    /// </summary>
    public IReadOnlyCollection<Guid>? AccountingPeriodIds { get; init; }

    /// <summary>
    /// Gets whether onboarded Account Goals with no Accounting Period should be included.
    /// </summary>
    public bool? IncludeOnboarded { get; init; }
}
