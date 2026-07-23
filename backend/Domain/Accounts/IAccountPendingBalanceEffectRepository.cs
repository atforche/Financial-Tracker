using Domain.Transactions;

namespace Domain.Accounts;

/// <summary>
/// Interface representing persisted pending Account Balance effects.
/// </summary>
public interface IAccountPendingBalanceEffectRepository
{
    /// <summary>
    /// Gets pending effects for the provided Accounts.
    /// </summary>
    IReadOnlyCollection<PendingAccountBalanceEffect> GetAllByAccountIds(IReadOnlyCollection<AccountId> accountIds);

    /// <summary>
    /// Gets pending effects for the provided Transaction.
    /// </summary>
    IReadOnlyCollection<PendingAccountBalanceEffect> GetAllByTransactionId(TransactionId transactionId);

    /// <summary>
    /// Adds the provided pending Account Balance effect.
    /// </summary>
    void Add(PendingAccountBalanceEffect effect);

    /// <summary>
    /// Deletes the provided pending Account Balance effect.
    /// </summary>
    void Delete(PendingAccountBalanceEffect effect);
}