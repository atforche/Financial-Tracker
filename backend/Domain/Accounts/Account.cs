using Domain.AccountingPeriods;
using Domain.Transactions;

namespace Domain.Accounts;

/// <summary>
/// Entity class representing an Account
/// </summary> 
/// <remarks>
/// An Account represents a financial account that money can be held in and transferred from.
/// </remarks>
public class Account : Entity<AccountId>
{
    /// <summary>
    /// Name for this Account
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Type for this Account
    /// </summary>
    public AccountType Type { get; private set; }

    /// <summary>
    /// Accounting Period that this Account was added in
    /// </summary>
    public AccountingPeriodId AddAccountingPeriodId { get; private set; }

    /// <summary>
    /// Date that this Account was added
    /// </summary>
    public DateOnly AddDate { get; private set; }

    /// <summary>
    /// Initial Transaction for this Account (if this account was created with an initial balance)
    /// </summary>
    public TransactionId? InitialTransaction { get; internal set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal Account(string name, AccountType type, AccountingPeriodId addAccountingPeriodId, DateOnly addDate)
        : base(new AccountId(Guid.NewGuid()))
    {
        Name = name;
        Type = type;
        AddAccountingPeriodId = addAccountingPeriodId;
        AddDate = addDate;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Account() : base()
    {
        Name = "";
        AddAccountingPeriodId = null!;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="Account"/>
/// </summary>
public record AccountId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// </summary>
    internal AccountId(Guid value)
        : base(value)
    {
    }
}