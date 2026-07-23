using Domain.Transactions;

namespace Domain.Funds;

/// <summary>
/// Service for managing posted Fund Balances.
/// </summary>
public sealed class FundBalanceService(
    IFundBalanceHistoryRepository fundBalanceHistoryRepository,
    ITransactionRepository transactionRepository,
    IFundRepository fundRepository,
    PendingFundBalanceService pendingFundBalanceService)
{
    /// <summary>
    /// Gets the current balance for the provided Fund.
    /// </summary>
    public FundBalance GetCurrentBalance(FundId fundId)
    {
        Fund fund = fundRepository.GetById(fundId);
        FundBalance postedBalance = fundBalanceHistoryRepository.GetLatestForFund(fundId)?.ToFundBalance()
            ?? new FundBalance(fund, fund.OnboardedBalance ?? 0, 0, 0);
        return pendingFundBalanceService.ApplyPendingEffects(postedBalance);
    }

    /// <summary>
    /// Gets the posted Fund Balances prior to the provided Transaction.
    /// </summary>
    public IEnumerable<FundBalance> GetPreviousBalancesForTransaction(Transaction transaction) =>
        transaction.GetAllAffectedFundIds(null).Select(fundId =>
        {
            FundBalanceHistory history = fundBalanceHistoryRepository.GetAllByTransactionId(transaction.Id)
                .Where(item => item.Fund.Id == fundId).OrderBy(item => item.Date).ThenBy(item => item.Sequence).First();
            return GetExistingFundBalanceAsOf(history.Fund, history.Date, history.Sequence);
        });

    /// <summary>
    /// Gets the posted Fund Balances after the provided Transaction.
    /// </summary>
    public IEnumerable<FundBalance> GetNewBalanceForTransaction(Transaction transaction) =>
        transaction.GetAllAffectedFundIds(null).Select(fundId => fundBalanceHistoryRepository.GetAllByTransactionId(transaction.Id)
            .Where(item => item.Fund.Id == fundId).OrderByDescending(item => item.Date).ThenByDescending(item => item.Sequence)
            .First().ToFundBalance());

    /// <summary>
    /// Rebuilds posted histories for a newly added Transaction.
    /// </summary>
    internal void AddTransaction(Transaction transaction) => SynchronizePostedHistories(transaction);

    /// <summary>
    /// Rebuilds posted histories after a Transaction posts to an Account.
    /// </summary>
    internal void PostTransaction(Transaction transaction) => SynchronizePostedHistories(transaction);

    /// <summary>
    /// Removes posted histories before a Transaction's posted dates are cleared.
    /// </summary>
    internal void UnpostTransaction(Transaction transaction) => DeleteTransaction(transaction);

    /// <summary>
    /// Deletes all posted balance histories for a Transaction.
    /// </summary>
    internal void DeleteTransaction(Transaction transaction)
    {
        foreach (FundBalanceHistory history in fundBalanceHistoryRepository.GetAllByTransactionId(transaction.Id).ToList())
        {
            DeleteExistingBalanceHistory(transaction, history);
            fundBalanceHistoryRepository.Delete(history);
        }
    }

    /// <summary>
    /// Replaces a Transaction's posted histories with entries for its current posted dates.
    /// </summary>
    private void SynchronizePostedHistories(Transaction transaction)
    {
        DeleteTransaction(transaction);
        foreach (FundId fundId in transaction.GetAllAffectedFundIds(null).Distinct())
        {
            Fund fund = fundRepository.GetById(fundId);
            foreach (DateOnly postedDate in GetPostedDates(transaction, fundId))
            {
                AddNewBalanceHistory(transaction, fund, postedDate);
            }
        }
    }

    /// <summary>
    /// Gets the dates on which a Transaction has posted effects for a Fund.
    /// </summary>
    private static List<DateOnly> GetPostedDates(Transaction transaction, FundId fundId)
    {
        var accountIds = transaction.GetAllAffectedAccountIds().ToList();
        return accountIds.Count == 0
            ? [transaction.Date]
            : accountIds.Where(accountId => transaction.GetAllAffectedFundIds(accountId).Contains(fundId))
                .Select(transaction.GetPostedDateForAccount).OfType<DateOnly>().Distinct().OrderBy(date => date).ToList();
    }

    /// <summary>
    /// Adds a posted Fund Balance History entry and replays later posted entries.
    /// </summary>
    private void AddNewBalanceHistory(Transaction transaction, Fund fund, DateOnly date)
    {
        int sequence = GetSequenceForTransaction(fund.Id, transaction, date);
        FundBalance existingBalance = GetExistingFundBalanceAsOf(fund, date, sequence);
        var history = new FundBalanceHistory(
            fund,
            transaction.Id,
            date,
            sequence,
            transaction.ApplyPostedEffectsToFundBalance(existingBalance, date));
        foreach (FundBalanceHistory later in fundBalanceHistoryRepository.GetAllHistoriesLaterThan(fund.Id, date, sequence))
        {
            if (later.Date == date)
            {
                later.Sequence += 1;
            }
            FundBalance updated = transaction.ApplyPostedEffectsToFundBalance(later.ToFundBalance(), date);
            later.Update(updated);
        }
        fundBalanceHistoryRepository.Add(history);
    }

    /// <summary>
    /// Removes a posted history entry and reverses its effects from subsequent entries.
    /// </summary>
    private void DeleteExistingBalanceHistory(Transaction transaction, FundBalanceHistory history)
    {
        FundBalance existingBalance = GetExistingFundBalanceAsOf(history.Fund, history.Date, history.Sequence);
        foreach (FundBalanceHistory later in fundBalanceHistoryRepository.GetAllHistoriesLaterThan(history.Fund.Id, history.Date, history.Sequence + 1))
        {
            if (later.Date == history.Date)
            {
                later.Sequence -= 1;
            }
            FundBalance updated = transaction.ApplyPostedEffectsToFundBalance(existingBalance, history.Date, reverse: true);
            later.Update(updated);
            existingBalance = updated;
        }
    }

    /// <summary>
    /// Gets the balance history sequence number for the provided Transaction.
    /// </summary>
    /// <remarks>
    /// This keeps rebuilt balance history entries in transaction-sequence order when an updated transaction is removed and re-added on the same date.
    /// </remarks>
    private int GetSequenceForTransaction(FundId fundId, Transaction transaction, DateOnly date) =>
        fundBalanceHistoryRepository.GetAllHistoriesLaterThan(fundId, date, 0)
            .Count(history => history.Date == date && transactionRepository.GetById(history.TransactionId).Sequence < transaction.Sequence) + 1;

    /// <summary>
    /// Gets the existing Fund Balance for the specified Fund ID as of the provided date and sequence number
    /// </summary>
    private FundBalance GetExistingFundBalanceAsOf(Fund fund, DateOnly asOfDate, int asOfSequence) =>
        fundBalanceHistoryRepository.GetLatestHistoryEarlierThan(fund.Id, asOfDate, asOfSequence)?.ToFundBalance()
        ?? new FundBalance(fund, fund.OnboardedBalance ?? 0, 0, 0);
}