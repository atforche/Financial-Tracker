using Domain.AccountingPeriods;

namespace Domain.Funds;

/// <summary>
/// Entity class representing a Fund
/// </summary>
/// <remarks>
/// A Fund represents a collection of money that the user has allocated for a specific purpose. 
/// Funds can be used to track savings goals, monthly expenses, or any other financial goal the user may have. 
/// Each Fund can optionally be marked as a system fund when it is managed by the application rather than by the user.
/// </remarks>
public class Fund : Entity<FundId>
{
    /// <summary>
    /// Name for this Fund
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Whether this Fund is a system-defined fund
    /// </summary>
    public bool IsSystemFund { get; private set; }

    /// <summary>
    /// Description for this Fund
    /// </summary>
    public string Description { get; internal set; }

    /// <summary>
    /// Accounting Period that this Fund was opened in
    /// </summary>
    public AccountingPeriodId OpeningAccountingPeriodId { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal Fund(string name, string description, AccountingPeriodId openingAccountingPeriodId, bool isSystemFund)
        : base(new FundId(Guid.NewGuid()))
    {
        Name = name;
        IsSystemFund = isSystemFund;
        Description = description;
        OpeningAccountingPeriodId = openingAccountingPeriodId;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Fund()
        : base()
    {
        Name = "";
        IsSystemFund = false;
        Description = "";
        OpeningAccountingPeriodId = null!;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="Fund"/>
/// </summary>
public record FundId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// </summary>
    internal FundId(Guid value)
        : base(value)
    {
    }
}