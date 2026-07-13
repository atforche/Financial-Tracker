using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Entity class representing the balance of a Fund at some point in time.
/// </summary>
public class FundBalanceHistory : Entity<FundBalanceHistoryId>
{
    /// <summary>
    /// Fund ID for this Fund Balance History
    /// </summary>
    public FundId FundId { get; init; }

    /// <summary>
    /// Transaction ID for this Fund Balance History
    /// </summary>
    public TransactionId TransactionId { get; init; }

    /// <summary>
    /// Date for this Fund Balance History
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// Sequence number for this Fund Balance History
    /// </summary>
    /// <remarks>
    /// The sequence number is used to order multiple balance history records for the same date.
    /// </remarks>
    public int Sequence { get; internal set; }

    /// <summary>
    /// Posted Balance for this Fund Balance History
    /// </summary>
    public decimal PostedBalance { get; private set; }

    /// <summary>
    /// Pending Debit Amount for this Fund Balance History
    /// </summary>
    public decimal PendingDebitAmount { get; private set; }

    /// <summary>
    /// Pending Credit Amount for this Fund Balance History
    /// </summary>
    public decimal PendingCreditAmount { get; private set; }

    /// <summary>
    /// Updates this Fund Balance History with a new Fund Balance.
    /// </summary>
    public void Update(FundBalance fundBalance)
    {
        if (fundBalance.FundId != FundId)
        {
            throw new InvalidOperationException("Cannot update Fund Balance History with a Fund Balance for a different Fund");
        }
        PostedBalance = fundBalance.PostedBalance;
        PendingDebitAmount = fundBalance.PendingDebitAmount;
        PendingCreditAmount = fundBalance.PendingCreditAmount;
    }

    /// <summary>
    /// Converts this Fund Balance History to a Fund Balance
    /// </summary>
    /// <returns></returns>
    public FundBalance ToFundBalance() => new(FundId, PostedBalance, PendingDebitAmount, PendingCreditAmount);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundBalanceHistory(FundId fundId,
        TransactionId transactionId,
        DateOnly date,
        int sequence,
        FundBalance fundBalance)
        : base(new FundBalanceHistoryId(Guid.NewGuid()))
    {
        FundId = fundId;
        TransactionId = transactionId;
        Date = date;
        Sequence = sequence;
        Update(fundBalance);
    }

    /// <summary>
    /// Creates a default instance of this class
    /// </summary>
    private FundBalanceHistory()
    {
        FundId = null!;
        TransactionId = null!;
    }
}

/// <summary>
/// Value object class representing the ID of an <see cref="FundBalanceHistory"/>
/// </summary>
public record FundBalanceHistoryId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// </summary>
    internal FundBalanceHistoryId(Guid value)
        : base(value)
    {
    }
}