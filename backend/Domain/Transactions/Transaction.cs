using Domain.AccountingPeriods;
using Domain.Accounts;

namespace Domain.Transactions;

/// <summary>
/// Entity class representing a Transaction
/// </summary>
/// <remarks>
/// A Transaction represents a financial transaction were money is moved between accounts or funds.
/// </remarks>
public class Transaction : Entity<TransactionId>
{
    /// <summary>
    /// Accounting Period for this Transaction
    /// </summary>
    public AccountingPeriodId AccountingPeriod { get; private set; }

    /// <summary>
    /// Date for this Transaction
    /// </summary>
    public DateOnly Date { get; internal set; }

    /// <summary>
    /// Location for this Transaction
    /// </summary>
    public string Location { get; internal set; }

    /// <summary>
    /// Description for this Transaction
    /// </summary>
    public string Description { get; internal set; }

    /// <summary>
    /// Debit Account for this Transaction
    /// </summary>
    public TransactionAccount? DebitAccount { get; internal set; }

    /// <summary>
    /// Credit Account for this Transaction
    /// </summary>
    public TransactionAccount? CreditAccount { get; internal set; }

    /// <summary>
    /// Account ID of the Account that created this transaction when it was created, or null
    /// </summary>
    public AccountId? InitialAccountTransaction { get; internal set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal Transaction(CreateTransactionRequest request)
        : base(new TransactionId(Guid.NewGuid()))
    {
        AccountingPeriod = request.AccountingPeriod;
        Date = request.Date;
        Location = request.Location;
        Description = request.Description;
        DebitAccount = request.DebitAccount != null ? new TransactionAccount(request.DebitAccount.Account.Id, request.DebitAccount.FundAmounts) : null;
        CreditAccount = request.CreditAccount != null ? new TransactionAccount(request.CreditAccount.Account.Id, request.CreditAccount.FundAmounts) : null;
        InitialAccountTransaction = request.IsInitialTransactionForCreditAccount ? CreditAccount?.Account : null;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Transaction() : base()
    {
        AccountingPeriod = null!;
        Location = null!;
        Description = null!;
        DebitAccount = null;
        CreditAccount = null;
    }
}