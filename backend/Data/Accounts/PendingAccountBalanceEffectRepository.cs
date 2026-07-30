using Domain.Accounts;
using Domain.Transactions;

namespace Data.Accounts;

/// <summary>
/// Repository that persists pending Account Balance effects.
/// </summary>
public sealed class PendingAccountBalanceEffectRepository(DatabaseContext databaseContext) : IAccountPendingBalanceEffectRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<PendingAccountBalanceEffect> GetAllByAccountIds(IReadOnlyCollection<AccountId> accountIds) =>
        accountIds.Count == 0
            ? []
            : databaseContext.PendingAccountBalanceEffects
                .Where(effect => accountIds.Contains(effect.Account.Id))
                .ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<PendingAccountBalanceEffect> GetAllByTransactionId(TransactionId transactionId) =>
        databaseContext.PendingAccountBalanceEffects
            .Where(effect => effect.TransactionId == transactionId)
            .ToList();

    /// <inheritdoc/>
    public void Add(PendingAccountBalanceEffect effect) => databaseContext.Add(effect);

    /// <inheritdoc/>
    public void Delete(PendingAccountBalanceEffect effect) => databaseContext.Remove(effect);
}