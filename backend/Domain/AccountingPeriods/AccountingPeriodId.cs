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
}

/// <summary>
/// Factory for constructing an Accounting Period ID
/// </summary>
/// <param name="accountingPeriodRepository">Accounting Period Repository</param>
public class AccountingPeriodIdFactory(IAccountingPeriodRepository accountingPeriodRepository)
{
    /// <summary>
    /// Creates a new Accounting Period ID with the given value
    /// </summary>
    /// <param name="value">Value for this Accounting Period ID</param>
    /// <returns>The newly created Accounting Period ID</returns>
    public AccountingPeriodId Create(Guid value)
    {
        if (!accountingPeriodRepository.DoesAccountingPeriodWithIdExist(value))
        {
            throw new InvalidOperationException();
        }
        return new AccountingPeriodId(value);
    }
}