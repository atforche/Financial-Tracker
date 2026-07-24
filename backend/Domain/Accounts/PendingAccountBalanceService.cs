using Domain.Transactions;

namespace Domain.Accounts;

/// <summary>
/// Service for deriving the current pending effects for Account Balances.
/// </summary>
public sealed class PendingAccountBalanceService(
    IAccountRepository accountRepository,
    IAccountPendingBalanceEffectRepository pendingAccountBalanceEffectRepository)
{
    /// <summary>
    /// Applies the current unposted Transaction effects to the provided posted Account Balance.
    /// </summary>
    public AccountBalance ApplyPendingEffects(AccountBalance postedBalance) =>
        ApplyPendingEffects([postedBalance]).Single();

    /// <summary>
    /// Applies the current unposted Transaction effects to the provided posted Account Balances.
    /// </summary>
    public IReadOnlyCollection<AccountBalance> ApplyPendingEffects(
        IReadOnlyCollection<AccountBalance> postedBalances)
    {
        var balances = postedBalances.ToDictionary(balance => balance.Account.Id, balance => balance.BalanceIncludingPending);
        foreach (PendingAccountBalanceEffect effect in pendingAccountBalanceEffectRepository
            .GetAllByAccountIds(balances.Keys.ToList()))
        {
            balances[effect.Account.Id] += effect.Account.Type.IsDebt()
                ? effect.PendingDebitAmount - effect.PendingCreditAmount
                : effect.PendingCreditAmount - effect.PendingDebitAmount;
        }
        return postedBalances.Select(balance => new AccountBalance(balance.Account, balance.PostedBalance, balances[balance.Account.Id])).ToList();
    }

    /// <summary>
    /// Replaces the persisted pending effects for a Transaction with its current unposted Account effects.
    /// </summary>
    internal void SynchronizeTransaction(Transaction transaction)
    {
        DeleteEffectsForTransaction(transaction.Id);
        foreach (AccountId accountId in transaction.GetAllAffectedAccountIds().Distinct()
            .Where(accountId => transaction.GetPostedDateForAccount(accountId) == null))
        {
            Account account = accountRepository.GetById(accountId);
            decimal change = transaction.ApplyAsPostedToAccountBalance(new AccountBalance(account, 0)).PostedBalance;
            decimal pendingDebitAmount = account.Type.IsDebt() ? Math.Max(change, 0) : Math.Max(-change, 0);
            decimal pendingCreditAmount = account.Type.IsDebt() ? Math.Max(-change, 0) : Math.Max(change, 0);
            if (pendingDebitAmount == 0 && pendingCreditAmount == 0)
            {
                continue;
            }
            pendingAccountBalanceEffectRepository.Add(new PendingAccountBalanceEffect(
                account,
                transaction.Id,
                pendingDebitAmount,
                pendingCreditAmount));
        }
    }

    /// <summary>
    /// Deletes all persisted pending effects for a Transaction.
    /// </summary>
    internal void DeleteEffectsForTransaction(TransactionId transactionId)
    {
        foreach (PendingAccountBalanceEffect effect in pendingAccountBalanceEffectRepository
            .GetAllByTransactionId(transactionId))
        {
            pendingAccountBalanceEffectRepository.Delete(effect);
        }
    }
}