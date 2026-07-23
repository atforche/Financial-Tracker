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
        var balances = postedBalances.ToDictionary(balance => balance.Account.Id);
        foreach (PendingAccountBalanceEffect effect in pendingAccountBalanceEffectRepository
            .GetAllByAccountIds(balances.Keys.ToList()))
        {
            AccountBalance balance = balances[effect.Account.Id];
            balances[effect.Account.Id] = balance
                .AddNewPendingDebitAmount(effect.PendingDebitAmount)
                .AddNewPendingCreditAmount(effect.PendingCreditAmount);
        }
        return postedBalances.Select(balance => balances[balance.Account.Id]).ToList();
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
            AccountBalance effect = transaction.ApplyToAccountBalance(new AccountBalance(account, 0, 0, 0));
            if (effect.PendingDebitAmount == 0 && effect.PendingCreditAmount == 0)
            {
                continue;
            }
            pendingAccountBalanceEffectRepository.Add(new PendingAccountBalanceEffect(
                account,
                transaction.Id,
                effect.PendingDebitAmount,
                effect.PendingCreditAmount));
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