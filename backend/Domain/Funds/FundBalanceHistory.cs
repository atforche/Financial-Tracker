using Domain.AccountingPeriods;
using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Entity class representing the balance of a Fund at some point in time.
/// </summary>
public class FundBalanceHistory : Entity<FundBalanceHistoryId>
{
    /// <summary>
    /// Fund for this Fund Balance History
    /// </summary>
    public Fund Fund { get; init; }

    /// <summary>
    /// Transaction ID for this Fund Balance History
    /// </summary>
    public TransactionId TransactionId { get; init; }

    /// <summary>
    /// Accounting Period that owns this posted movement.
    /// </summary>
    public AccountingPeriodId AccountingPeriodId { get; init; }

    /// <summary>
    /// Cumulative posted change within this Accounting Period, excluding its opening balance.
    /// </summary>
    public decimal AccountingPeriodBalanceChange { get; private set; }

    /// <summary>
    /// Date for this Fund Balance History
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// Sequence number for this Fund Balance History
    /// </summary>
    public int Sequence { get; internal set; }

    /// <summary>
    /// Posted Balance for this Fund Balance History
    /// </summary>
    public decimal PostedBalance { get; private set; }

    /// <summary>
    /// Updates this Fund Balance History with a new Fund Balance.
    /// </summary>
    public void Update(FundBalance fundBalance)
    {
        if (fundBalance.Fund.Id != Fund.Id)
        {
            throw new InvalidOperationException("Cannot update Fund Balance History with a Fund Balance for a different Fund");
        }
        PostedBalance = fundBalance.PostedBalance;
    }

    /// <summary>
    /// Adjusts later within-period balances after an earlier movement changes.
    /// </summary>
    internal void AdjustAccountingPeriodBalanceChange(decimal change) => AccountingPeriodBalanceChange += change;

    /// <summary>
    /// Converts this Fund Balance History to a Fund Balance
    /// </summary>
    public FundBalance ToFundBalance() => new(Fund, PostedBalance);

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    internal FundBalanceHistory(Fund fund,
        TransactionId transactionId,
        AccountingPeriodId accountingPeriodId,
        DateOnly date,
        int sequence,
        FundBalance fundBalance,
        decimal accountingPeriodBalanceChange)
        : base(new FundBalanceHistoryId(Guid.NewGuid()))
    {
        Fund = fund;
        TransactionId = transactionId;
        AccountingPeriodId = accountingPeriodId;
        AccountingPeriodBalanceChange = accountingPeriodBalanceChange;
        Date = date;
        Sequence = sequence;
        Update(fundBalance);
    }

    /// <summary>
    /// Creates a default instance of this class
    /// </summary>
    private FundBalanceHistory()
    {
        Fund = null!;
        TransactionId = null!;
        AccountingPeriodId = null!;
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
