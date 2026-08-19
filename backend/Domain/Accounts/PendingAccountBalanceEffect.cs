using Domain.Transactions;

namespace Domain.Accounts;

/// <summary>
/// Persisted unposted Transaction effect for an Account Balance.
/// </summary>
public class PendingAccountBalanceEffect : Entity<PendingAccountBalanceEffectId>
{
    /// <summary>
    /// Account affected by this pending effect.
    /// </summary>
    public Account Account { get; init; }

    /// <summary>
    /// Transaction that created this pending effect.
    /// </summary>
    public TransactionId TransactionId { get; init; }

    /// <summary>
    /// Pending debit amount for the Account.
    /// </summary>
    public decimal PendingDebitAmount { get; init; }

    /// <summary>
    /// Pending credit amount for the Account.
    /// </summary>
    public decimal PendingCreditAmount { get; init; }

    /// <summary>
    /// Constructs a persisted pending Account Balance effect.
    /// </summary>
    internal PendingAccountBalanceEffect(
        Account account,
        TransactionId transactionId,
        decimal pendingDebitAmount,
        decimal pendingCreditAmount)
        : base(new PendingAccountBalanceEffectId(Guid.NewGuid()))
    {
        Account = account;
        TransactionId = transactionId;
        PendingDebitAmount = pendingDebitAmount;
        PendingCreditAmount = pendingCreditAmount;
    }

    /// <summary>
    /// Creates a default instance of this pending Account Balance effect.
    /// </summary>
    private PendingAccountBalanceEffect()
    {
        Account = null!;
        TransactionId = null!;
    }
}

/// <summary>
/// Value object class representing the ID of a <see cref="PendingAccountBalanceEffect"/>.
/// </summary>
public record PendingAccountBalanceEffectId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    internal PendingAccountBalanceEffectId(Guid value)
        : base(value)
    {
    }
}
