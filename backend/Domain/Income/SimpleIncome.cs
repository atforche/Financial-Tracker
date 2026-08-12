namespace Domain.Income;

/// <summary>
/// A simple income receipt without a payroll breakdown.
/// </summary>
public sealed class SimpleIncome(decimal trackedAmount, decimal untrackedAmount = 0)
    : IncomeBreakdown(new IncomeBreakdownId(Guid.NewGuid()))
{
    private readonly decimal _trackedAmount = trackedAmount;
    private readonly decimal _untrackedAmount = untrackedAmount;

    /// <inheritdoc/>
    public override decimal TrackedAmount => _trackedAmount;

    /// <inheritdoc/>
    public override decimal UntrackedAmount => _untrackedAmount;

    internal SimpleIncome Snapshot() => new(TrackedAmount, UntrackedAmount);
}