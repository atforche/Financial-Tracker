namespace Domain.AccountingPeriods;

/// <summary>
/// Value object class representing the ID of an <see cref="AccountingPeriod"/>
/// </summary>
public record AccountingPeriodId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// This constructor should only even be used when creating a new Accounting Period ID during Accounting Period creation. 
    /// </summary>
    /// <param name="value">Value for this Accounting Period ID</param>
    internal AccountingPeriodId(Guid value)
        : base(value)
    {
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private AccountingPeriodId()
        : base()
    {
    }
}