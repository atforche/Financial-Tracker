using Domain.AccountingPeriods;
using Domain.Transactions;

namespace Domain.Budgets;

/// <summary>
/// Service for managing Budget Balances
/// </summary>
public class BudgetBalanceService(
    IBudgetBalanceHistoryRepository budgetBalanceHistoryRepository,
    IBudgetGoalRepository budgetGoalRepository)
{
    /// <summary>
    /// Gets the current balance for the provided Budget Goal
    /// </summary>
    public BudgetBalance GetCurrentBalance(BudgetGoal budgetGoal)
    {
        BudgetBalanceHistory? latest = budgetBalanceHistoryRepository.GetLatestForBudgetGoal(budgetGoal.Id);
        if (latest != null)
        {
            return new BudgetBalance(budgetGoal, latest.PostedBalance, latest.AvailableToSpend);
        }
        return new BudgetBalance(budgetGoal, 0.00m, 0.00m);
    }

    /// <summary>
    /// Gets the Budget Balances prior to the provided Transaction
    /// </summary>
    public IEnumerable<BudgetBalance> GetPreviousBalancesForTransaction(Transaction transaction) =>
        GetAllAffectedBudgetGoals(transaction)
            .Select(budgetGoal =>
            {
                BudgetBalanceHistory latestHistory = budgetBalanceHistoryRepository
                    .GetAllByTransactionId(transaction.Id)
                    .OrderBy(h => h.Date)
                    .ThenBy(h => h.Sequence)
                    .First();
                return GetExistingBudgetBalanceAsOf(budgetGoal, latestHistory.Date, latestHistory.Sequence);
            });

    /// <summary>
    /// Gets the Budget Balances after the provided Transaction
    /// </summary>
    public IEnumerable<BudgetBalance> GetNewBalancesForTransaction(Transaction transaction) =>
        GetAllAffectedBudgetGoals(transaction)
            .Select(budgetGoal =>
            {
                BudgetBalanceHistory latestHistory = budgetBalanceHistoryRepository
                    .GetAllByTransactionId(transaction.Id)
                    .OrderByDescending(h => h.Date)
                    .ThenByDescending(h => h.Sequence)
                    .First();
                return new BudgetBalance(budgetGoal, latestHistory.PostedBalance, latestHistory.AvailableToSpend);
            });

    /// <summary>
    /// Updates the Budget Balances for a newly added Transaction
    /// </summary>
    internal void AddTransaction(Transaction newTransaction)
    {
        foreach (BudgetGoal budgetGoal in GetAllAffectedBudgetGoals(newTransaction))
        {
            AddNewBalanceHistory(newTransaction, budgetGoal, newTransaction.Date);
        }
    }

    /// <summary>
    /// Updates the Budget Balances for an updated Transaction
    /// </summary>
    internal void UpdateTransaction(Transaction transaction)
    {
        foreach (BudgetGoal budgetGoal in GetAllAffectedBudgetGoals(transaction))
        {
            UpdateExistingBalanceHistory(transaction, budgetGoal);
        }
    }

    /// <summary>
    /// Updates the Budget Balances for a newly posted Transaction
    /// </summary>
    internal void PostTransaction(Transaction transaction, TransactionAccount transactionAccount)
    {
        if (transactionAccount.PostedDate == null)
        {
            return;
        }
        IEnumerable<BudgetGoal> affectedBudgetGoals = transaction.DebitAccount != null && transaction.CreditAccount != null && transaction.DebitAccount.AccountId == transaction.CreditAccount.AccountId
            ? GetAllAffectedBudgetGoals(transaction)
            : GetBudgetGoalsForAmounts(transactionAccount.BudgetAmounts, transaction.AccountingPeriodId);
        foreach (BudgetGoal budgetGoal in affectedBudgetGoals)
        {
            if (transactionAccount.PostedDate == transaction.Date)
            {
                UpdateExistingBalanceHistory(transaction, budgetGoal);
            }
            else
            {
                AddNewBalanceHistory(transaction, budgetGoal, transactionAccount.PostedDate.Value);
            }
        }
    }

    /// <summary>
    /// Updates the Budget Balances for an unposted Transaction
    /// </summary>
    internal void UnpostTransaction(Transaction transaction, TransactionAccount transactionAccount)
    {
        IEnumerable<BudgetGoal> affectedBudgetGoals = transaction.DebitAccount != null && transaction.CreditAccount != null && transaction.DebitAccount.AccountId == transaction.CreditAccount.AccountId
            ? GetAllAffectedBudgetGoals(transaction)
            : GetBudgetGoalsForAmounts(transactionAccount.BudgetAmounts, transaction.AccountingPeriodId);
        foreach (BudgetGoal budgetGoal in affectedBudgetGoals)
        {
            BudgetBalanceHistory? postedHistory = budgetBalanceHistoryRepository
                .GetAllByTransactionId(transaction.Id)
                .SingleOrDefault(bh => bh.BudgetGoal.Budget.Id == budgetGoal.Budget.Id && bh.Date != transaction.Date);
            if (postedHistory == null)
            {
                UpdateExistingBalanceHistory(transaction, budgetGoal);
            }
            else
            {
                DeleteExistingBalanceHistory(postedHistory);
                budgetBalanceHistoryRepository.Delete(postedHistory);
            }
        }
    }

    /// <summary>
    /// Updates the Budget Balances for a deleted Transaction
    /// </summary>
    internal void DeleteTransaction(Transaction transaction)
    {
        foreach (BudgetBalanceHistory balanceHistory in budgetBalanceHistoryRepository.GetAllByTransactionId(transaction.Id))
        {
            DeleteExistingBalanceHistory(balanceHistory);
            budgetBalanceHistoryRepository.Delete(balanceHistory);
        }
    }

    /// <summary>
    /// Adds a new Budget Balance History entry
    /// </summary>
    private void AddNewBalanceHistory(Transaction transaction, BudgetGoal budgetGoal, DateOnly date)
    {
        int sequence = budgetBalanceHistoryRepository.GetNextSequenceForBudgetGoalAndDate(budgetGoal.Id, date);
        BudgetBalance existingBalance = GetExistingBudgetBalanceAsOf(budgetGoal, date, sequence);
        BudgetBalance newBalance = transaction.ApplyToBudgetBalance(existingBalance, date);
        var newBalanceHistory = new BudgetBalanceHistory(budgetGoal,
            transaction.Id,
            date,
            sequence,
            newBalance.PostedBalance,
            newBalance.AvailableToSpend);

        foreach ((BudgetBalanceHistory history, Transaction existingTransaction) in budgetBalanceHistoryRepository
            .GetAllHistoriesLaterThan(budgetGoal.Id, date, sequence))
        {
            if (history.Date == date)
            {
                history.Sequence += 1;
            }
            BudgetBalance updatedBalance = existingTransaction.ApplyToBudgetBalance(newBalance, history.Date);
            history.Update(updatedBalance);
            newBalance = updatedBalance;
        }
        budgetBalanceHistoryRepository.Add(newBalanceHistory);
    }

    /// <summary>
    /// Updates an existing Budget Balance History entry
    /// </summary>
    private void UpdateExistingBalanceHistory(Transaction transaction, BudgetGoal budgetGoal)
    {
        BudgetBalanceHistory existingHistory = budgetBalanceHistoryRepository.GetEarliestByTransactionId(transaction.Id);
        BudgetBalance existingBalance = GetExistingBudgetBalanceAsOf(budgetGoal, existingHistory.Date, existingHistory.Sequence);
        BudgetBalance newBalance = transaction.ApplyToBudgetBalance(existingBalance, existingHistory.Date);
        existingHistory.Update(newBalance);

        foreach ((BudgetBalanceHistory history, Transaction existingTransaction) in budgetBalanceHistoryRepository
            .GetAllHistoriesLaterThan(budgetGoal.Id, existingHistory.Date, existingHistory.Sequence))
        {
            BudgetBalance updatedBalance = existingTransaction.ApplyToBudgetBalance(newBalance, history.Date);
            history.Update(updatedBalance);
            newBalance = updatedBalance;
        }
    }

    /// <summary>
    /// Deletes an existing Budget Balance History entry and cascades the change forward
    /// </summary>
    private void DeleteExistingBalanceHistory(BudgetBalanceHistory deletedHistory)
    {
        BudgetBalance existingBalance = GetExistingBudgetBalanceAsOf(deletedHistory.BudgetGoal, deletedHistory.Date, deletedHistory.Sequence);
        foreach ((BudgetBalanceHistory history, Transaction transaction) in budgetBalanceHistoryRepository
            .GetAllHistoriesLaterThan(deletedHistory.BudgetGoal.Id, deletedHistory.Date, deletedHistory.Sequence + 1))
        {
            if (history.Date == deletedHistory.Date)
            {
                history.Sequence -= 1;
            }
            BudgetBalance updatedBalance = transaction.ApplyToBudgetBalance(existingBalance, history.Date);
            history.Update(updatedBalance);
            existingBalance = updatedBalance;
        }
    }

    /// <summary>
    /// Gets the existing Budget Balance for the specified Budget Goal as of the provided date and sequence number
    /// </summary>
    private BudgetBalance GetExistingBudgetBalanceAsOf(BudgetGoal budgetGoal, DateOnly asOfDate, int asOfSequence)
    {
        BudgetBalanceHistory? existingHistory = budgetBalanceHistoryRepository
            .GetLatestHistoryEarlierThan(budgetGoal.Id, asOfDate, asOfSequence);
        if (existingHistory != null)
        {
            return new BudgetBalance(budgetGoal, existingHistory.PostedBalance, existingHistory.AvailableToSpend);
        }
        return new BudgetBalance(budgetGoal, 0.00m, 0.00m);
    }

    /// <summary>
    /// Gets all Budget Goals from both accounts of the provided Transaction
    /// </summary>
    private List<BudgetGoal> GetAllAffectedBudgetGoals(Transaction transaction)
    {
        List<BudgetGoal> goals = [];
        if (transaction.DebitAccount != null)
        {
            goals.AddRange(GetBudgetGoalsForAmounts(transaction.DebitAccount.BudgetAmounts, transaction.AccountingPeriodId));
        }
        if (transaction.CreditAccount != null)
        {
            goals.AddRange(GetBudgetGoalsForAmounts(transaction.CreditAccount.BudgetAmounts, transaction.AccountingPeriodId));
        }
        return goals;
    }

    /// <summary>
    /// Gets the list of budget goals that correspond to the provided budget amounts and accounting period
    /// </summary>
    private List<BudgetGoal> GetBudgetGoalsForAmounts(IEnumerable<BudgetAmount> budgetAmounts, AccountingPeriodId accountingPeriodId) =>
        budgetAmounts
            .Select(amount => budgetGoalRepository.GetByBudgetAndAccountingPeriod(amount.Budget.Id, accountingPeriodId))
            .OfType<BudgetGoal>()
            .ToList();
}
