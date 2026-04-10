using Domain.AccountingPeriods;

namespace Domain.Funds;

/// <summary>
/// Entity class representing a Fund
/// </summary>
/// <remarks>
/// A Fund represents a collection of money that the user has allocated for a specific purpose. 
/// Funds can be used to track savings goals, monthly expenses, or any other financial goal the user may have. 
/// Each Fund has a type that determines how the assignment target is calculated and how it should be used in the application.
/// </remarks>
public class Fund : Entity<FundId>
{
    /// <summary>
    /// Name for this Fund
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Type for this Fund
    /// </summary>
    public FundType Type { get; internal set; }

    /// <summary>
    /// Description for this Fund
    /// </summary>
    public string Description { get; internal set; }

    /// <summary>
    /// Accounting Period that this Fund was added in
    /// </summary>
    public AccountingPeriodId AddAccountingPeriodId { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal Fund(string name, FundType type, string description, AccountingPeriodId addAccountingPeriodId)
        : base(new FundId(Guid.NewGuid()))
    {
        Name = name;
        Type = type;
        Description = description;
        AddAccountingPeriodId = addAccountingPeriodId;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Fund()
        : base()
    {
        Name = "";
        Description = "";
        AddAccountingPeriodId = null!;
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