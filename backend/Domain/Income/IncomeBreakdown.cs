namespace Domain.Income;

/// <summary>
/// Describes the economic composition of an income receipt.
/// </summary>
public abstract class IncomeBreakdown : Entity<IncomeBreakdownId>
{
    /// <summary>
    /// Income deposited into tracked accounts.
    /// </summary>
    public abstract decimal TrackedAmount { get; }

    /// <summary>
    /// Income deposited into accounts whose balances are not tracked by the application.
    /// </summary>
    public abstract decimal UntrackedAmount { get; }

    /// <summary>
    /// Total recognized income.
    /// </summary>
    public decimal TotalAmount => TrackedAmount + UntrackedAmount;

    /// <summary>
    /// Constructs an income breakdown with the provided identity.
    /// </summary>
    protected IncomeBreakdown(IncomeBreakdownId id) : base(id) { }

    /// <summary>
    /// Constructs a default instance for Entity Framework.
    /// </summary>
    protected IncomeBreakdown() : base() { }
}

/// <summary>
/// Identifier for an <see cref="IncomeBreakdown"/>.
/// </summary>
public sealed record IncomeBreakdownId : EntityId
{
    internal IncomeBreakdownId(Guid value) : base(value) { }
}