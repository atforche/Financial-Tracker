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
        var balances = postedBalances.ToDictionary(balance => balance.Fund.Id, balance => balance.BalanceIncludingPending);
        foreach (PendingFundBalanceEffect effect in repository.GetAllByFundIds(balances.Keys.ToList()))
        {
            balances[effect.Fund.Id] += effect.PendingCreditAmount - effect.PendingDebitAmount;
        }
        return postedBalances.Select(balance => new FundBalance(balance.Fund, balance.PostedBalance, balances[balance.Fund.Id])).ToList();
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
            decimal change = transaction.ApplyAsPostedToFundBalance(new FundBalance(fund, 0)).PostedBalance;
            decimal pendingDebitAmount = Math.Max(-change, 0);
            decimal pendingCreditAmount = Math.Max(change, 0);
            if (pendingDebitAmount != 0 || pendingCreditAmount != 0)
            {
                repository.Add(new PendingFundBalanceEffect(fund, transaction.Id, pendingDebitAmount, pendingCreditAmount));
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