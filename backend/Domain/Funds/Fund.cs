using Domain.AccountingPeriods;

namespace Domain.Funds;

/// <summary>
/// Entity class representing a Fund
/// </summary>
/// <remarks>
/// A Fund represents a grouping of money that can be spread across multiple Accounts. 
/// The balance of each Account may be made up of money from multiple Funds. The balance of a Fund
/// over time can be used to track financial changes in an Account-agnostic way.
/// </remarks>
public class Fund : Entity<FundId>
{
    /// <summary>
    /// Name for this Fund
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Description for this Funds
    /// </summary>
    public string Description { get; internal set; }

    /// <summary>
    /// Accounting Period that this Fund was added in
    /// </summary>
    public AccountingPeriodId AddAccountingPeriodId { get; private set; }

    /// <summary>
    /// Date that this Fund was added
    /// </summary>
    public DateOnly AddDate { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal Fund(string name, string description, AccountingPeriodId addAccountingPeriodId, DateOnly addDate)
        : base(new FundId(Guid.NewGuid()))
    {
        Name = name;
        Description = description;
        AddAccountingPeriodId = addAccountingPeriodId;
        AddDate = addDate;
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