using Domain;
using Domain.AccountingPeriods;
using Domain.Funds;
using Domain.Transactions;

namespace Tests.Old.Mocks;

/// <summary>
/// Mock repository of Transactions for testing
/// </summary>
public class MockTransactionRepository : ITransactionRepository
{
    private readonly Dictionary<Guid, Transaction> _transactions = [];

    /// <inheritdoc/>
    public bool DoesTransactionWithIdExist(Guid id) => _transactions.ContainsKey(id);

    /// <inheritdoc/>
    public bool DoesTransactionWithFundExist(Fund fund) =>
        _transactions.Values.Any(transaction =>
        (transaction.DebitFundAmounts != null && transaction.DebitFundAmounts.Any(fundAmount => fundAmount.FundId == fund.Id)) ||
            (transaction.CreditFundAmounts != null && transaction.CreditFundAmounts.Any(fundAmount => fundAmount.FundId == fund.Id)));

    /// <inheritdoc/>
    public Transaction FindById(TransactionId id) => _transactions[id.Value];

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> FindAllByAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
        _transactions.Values.Where(transaction => transaction.AccountingPeriodId == accountingPeriodId).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> FindAllByDateRange(DateRange dateRange)
    {
        var dates = dateRange.GetInclusiveDates().ToList();
        return _transactions.Values
            .Where(transaction => (transaction.Date >= dates.First() && transaction.Date <= dates.Last()) ||
                transaction.TransactionBalanceEvents.Any(balanceEvent => balanceEvent.EventDate >= dates.First() && balanceEvent.EventDate <= dates.Last()))
            .ToList();
    }

    /// <inheritdoc/>
    public void Add(Transaction transaction) => _transactions.Add(transaction.Id.Value, transaction);
}