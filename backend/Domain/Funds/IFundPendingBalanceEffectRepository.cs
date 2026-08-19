using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Persists pending Fund Balance effects.
/// </summary>
public interface IFundPendingBalanceEffectRepository
{
    /// <summary>
    /// Gets effects for the provided Funds.
    /// </summary>
    IReadOnlyCollection<PendingFundBalanceEffect> GetAllByFundIds(IReadOnlyCollection<FundId> fundIds);

    /// <summary>
    /// Gets effects created by a Transaction.
    /// </summary>
    IReadOnlyCollection<PendingFundBalanceEffect> GetAllByTransactionId(TransactionId transactionId);

    /// <summary>
    /// Adds a pending effect.
    /// </summary>
    void Add(PendingFundBalanceEffect effect);

    /// <summary>
    /// Deletes a pending effect.
    /// </summary>
    void Delete(PendingFundBalanceEffect effect);
}
