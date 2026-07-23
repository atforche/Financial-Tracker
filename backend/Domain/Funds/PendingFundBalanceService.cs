using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Derives and persists current unposted Fund Balance effects.
/// </summary>
public sealed class PendingFundBalanceService(
    IFundRepository fundRepository,
    IFundPendingBalanceEffectRepository repository)
{
    /// <summary>
    /// Applies pending effects to one posted Fund Balance.
    /// </summary>
    public FundBalance ApplyPendingEffects(FundBalance postedBalance) => ApplyPendingEffects([postedBalance]).Single();

    /// <summary>
    /// Applies pending effects to posted Fund Balances.
    /// </summary>
    public IReadOnlyCollection<FundBalance> ApplyPendingEffects(IReadOnlyCollection<FundBalance> postedBalances)
    {
        var balances = postedBalances.ToDictionary(balance => balance.Fund.Id);
        foreach (PendingFundBalanceEffect effect in repository.GetAllByFundIds(balances.Keys.ToList()))
        {
            FundBalance balance = balances[effect.Fund.Id];
            balances[effect.Fund.Id] = balance
                .AddNewPendingDebitAmount(effect.PendingDebitAmount)
                .AddNewPendingCreditAmount(effect.PendingCreditAmount);
        }
        return postedBalances.Select(balance => balances[balance.Fund.Id]).ToList();
    }

    /// <summary>
    /// Rebuilds pending effects for a Transaction, replacing any existing effects for that Transaction.
    /// </summary>
    internal void SynchronizeTransaction(Transaction transaction)
    {
        DeleteEffectsForTransaction(transaction.Id);
        foreach (FundId fundId in transaction.GetAllAffectedFundIds(null).Distinct())
        {
            Fund fund = fundRepository.GetById(fundId);
            FundBalance effect = transaction.ApplyToFundBalance(new FundBalance(fund, 0, 0, 0));
            if (effect.PendingDebitAmount != 0 || effect.PendingCreditAmount != 0)
            {
                repository.Add(new PendingFundBalanceEffect(fund, transaction.Id, effect.PendingDebitAmount, effect.PendingCreditAmount));
            }
        }
    }

    /// <summary>
    /// Deletes all pending effects for a Transaction.
    /// </summary>
    internal void DeleteEffectsForTransaction(TransactionId transactionId)
    {
        foreach (PendingFundBalanceEffect effect in repository.GetAllByTransactionId(transactionId))
        {
            repository.Delete(effect);
        }
    }
}