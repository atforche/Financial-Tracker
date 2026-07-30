using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Persisted unposted Transaction effect for a Fund Balance.
/// </summary>
public sealed class PendingFundBalanceEffect : Entity<PendingFundBalanceEffectId>
{
    /// <summary>
    /// Fund affected by this pending effect.
    /// </summary>
    public Fund Fund { get; init; }

    /// <summary>
    /// Transaction that created this pending effect.
    /// </summary>
    public TransactionId TransactionId { get; init; }

    /// <summary>
    /// Pending debit amount for the Fund.
    /// </summary>
    public decimal PendingDebitAmount { get; init; }

    /// <summary>
    /// Pending credit amount for the Fund.
    /// </summary>
    public decimal PendingCreditAmount { get; init; }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal PendingFundBalanceEffect(Fund fund, TransactionId transactionId, decimal pendingDebitAmount, decimal pendingCreditAmount)
        : base(new PendingFundBalanceEffectId(Guid.NewGuid()))
    {
        Fund = fund;
        TransactionId = transactionId;
        PendingDebitAmount = pendingDebitAmount;
        PendingCreditAmount = pendingCreditAmount;
    }

    /// <summary>
    /// Constructs a new instance of this class for EF Core.
    /// </summary>
    private PendingFundBalanceEffect()
    {
        Fund = null!;
        TransactionId = null!;
    }
}

/// <summary>
/// ID for a <see cref="PendingFundBalanceEffect"/>.
/// </summary>
public sealed record PendingFundBalanceEffectId : EntityId
{
    internal PendingFundBalanceEffectId(Guid value) : base(value) { }
}