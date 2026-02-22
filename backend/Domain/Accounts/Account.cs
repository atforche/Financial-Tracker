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
    public AccountingPeriodId InitialAccountingPeriodId { get; private set; }

    /// <summary>
    /// Date that this Account was added
    /// </summary>
    public DateOnly InitialDate { get; private set; }

    /// <summary>
    /// Initial Transaction for this Account (if this account was created with an initial balance)
    /// </summary>
    public TransactionId? InitialTransaction { get; internal set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal Account(string name, AccountType type, AccountingPeriodId initialAccountingPeriodId, DateOnly initialDate)
        : base(new AccountId(Guid.NewGuid()))
    {
        Name = name;
        Type = type;
        InitialAccountingPeriodId = initialAccountingPeriodId;
        InitialDate = initialDate;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Account() : base()
    {
        Name = "";
        InitialAccountingPeriodId = null!;
    }
}