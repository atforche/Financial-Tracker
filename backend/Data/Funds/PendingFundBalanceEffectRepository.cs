using Domain.Funds;
using Domain.Transactions;

namespace Data.Funds;

/// <summary>
/// Repository for pending Fund Balance effects.
/// </summary>
public sealed class PendingFundBalanceEffectRepository(DatabaseContext databaseContext) : IFundPendingBalanceEffectRepository
{
    /// <inheritdoc/>
    public IReadOnlyCollection<PendingFundBalanceEffect> GetAllByFundIds(IReadOnlyCollection<FundId> fundIds) =>
        fundIds.Count == 0 ? [] : databaseContext.PendingFundBalanceEffects.Where(effect => fundIds.Contains(effect.Fund.Id)).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<PendingFundBalanceEffect> GetAllByTransactionId(TransactionId transactionId) =>
        databaseContext.PendingFundBalanceEffects.Where(effect => effect.TransactionId == transactionId).ToList();

    /// <inheritdoc/>
    public void Add(PendingFundBalanceEffect effect) => databaseContext.Add(effect);

    /// <inheritdoc/>
    public void Delete(PendingFundBalanceEffect effect) => databaseContext.Remove(effect);
}