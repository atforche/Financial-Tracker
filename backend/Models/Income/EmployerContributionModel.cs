namespace Models.Income;

/// <summary>
/// Employer-funded compensation outside the cash deposit.
/// </summary>
public sealed class EmployerContributionModel
{
    /// <summary>
    /// Description shown for the contribution.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Amount contributed by the employer.
    /// </summary>
    public required decimal Amount { get; init; }
}