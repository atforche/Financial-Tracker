using Data.EntityModels;
using Data.ValueObjectModels;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

/// <summary>
/// Repository that allows Transactions to be persisted to the database
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly DatabaseContext _context;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="context">Context to use to connect to the database</param>
    public TransactionRepository(DatabaseContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> FindAllByAccountingPeriod(DateOnly accountingPeriod) =>
        _context.Transactions
            .Include(transaction => transaction.AccountingEntries)
            .Include(transaction => transaction.DebitDetail)
            .Include(transaction => transaction.CreditDetail)
            .Where(transaction => transaction.AccountingDate.Year == accountingPeriod.Year &&
            transaction.AccountingDate.Month == accountingPeriod.Month)
            .Select(ConvertToEntity).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> FindAllByAccount(Guid accountId) =>
        _context.Transactions
            .Include(transaction => transaction.AccountingEntries)
            .Include(transaction => transaction.DebitDetail)
            .Include(transaction => transaction.CreditDetail)
            .Where(transaction => (transaction.DebitDetail != null && transaction.DebitDetail.AccountId == accountId) ||
                (transaction.CreditDetail != null && transaction.CreditDetail.AccountId == accountId))
            .Select(ConvertToEntity).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> FindAllByAccountOverDateRange(Guid accountId,
        DateOnly startDate,
        DateOnly endDate,
        DateToCompare dateToCompare) =>
        _context.Transactions
            .Include(transaction => transaction.AccountingEntries)
            .Include(transaction => transaction.DebitDetail)
            .Include(transaction => transaction.CreditDetail)
            .Where(transaction => (transaction.DebitDetail != null && transaction.DebitDetail.AccountId == accountId) ||
                (transaction.CreditDetail != null && transaction.CreditDetail.AccountId == accountId))
            .Where(transaction => !dateToCompare.HasFlag(DateToCompare.Accounting) || transaction.AccountingDate >= startDate && transaction.AccountingDate <= endDate)
            .Where(transaction => !dateToCompare.HasFlag(DateToCompare.Statement) ||
                (transaction.CreditDetail != null && transaction.CreditDetail.AccountId == accountId && transaction.CreditDetail.StatementDate >= startDate && transaction.CreditDetail.StatementDate <= endDate) ||
                (transaction.DebitDetail != null && transaction.DebitDetail.AccountId == accountId && transaction.DebitDetail.StatementDate >= startDate && transaction.DebitDetail.StatementDate <= endDate))
            .Select(ConvertToEntity).ToList();

    /// <inheritdoc/>
    public void Add(Transaction transaction)
    {
        var transactionData = PopulateFromTransaction(transaction, null);
        _context.Add(transactionData);
        if (transactionData.DebitDetail != null)
        {
            _context.Add(transactionData.DebitDetail);
        }
        if (transactionData.CreditDetail != null)
        {
            _context.Add(transactionData.CreditDetail);
        }
        foreach (AccountingEntryData accountingEntry in transactionData.AccountingEntries)
        {
            _context.Add(accountingEntry);
        }
    }

    /// <summary>
    /// Converts the provided <see cref="TransactionData"/> object into a <see cref="Transaction"/> domain entity.
    /// </summary>
    /// <param name="transactionData">Transaction Data to be converted</param>
    /// <returns>The converted Transaction domain entity</returns>
    private Transaction ConvertToEntity(TransactionData transactionData) => new Transaction(
        new TransactionRecreateRequest
        {
            Id = transactionData.Id,
            AccountingDate = transactionData.AccountingDate,
            DebitDetail = transactionData.DebitDetail != null ? GetRecreateRequest(transactionData.DebitDetail) : null,
            CreditDetail = transactionData.CreditDetail != null ? GetRecreateRequest(transactionData.CreditDetail) : null,
            AccountingEntries = transactionData.AccountingEntries.Select(entry => entry.Amount)
        });

    /// <summary>
    /// Converts the provided <see cref="TransactionDetailData"/> object into an <see cref="TransactionDetailRecreateRequest"/>.
    /// </summary>
    /// <param name="transactionDetailData">Transaction Detail Data to be converted</param>
    /// <returns>The converted IRecreateTransactionRequest</returns>
    private static TransactionDetailRecreateRequest GetRecreateRequest(TransactionDetailData transactionDetailData) =>
        new TransactionDetailRecreateRequest
        {
            Id = transactionDetailData.Id,
            AccountId = transactionDetailData.AccountId,
            StatementDate = transactionDetailData.StatementDate,
            IsPosted = transactionDetailData.IsPosted,
        };

    /// <summary>
    /// Converts the provided <see cref="Transaction"/> entity into a <see cref="TransactionData"/> data object
    /// </summary>
    /// <param name="transaction">Transaction entity to convert</param>
    /// <param name="existingTransactionData">Existing Transaction Data model to populate from the entity, or null if a new model should be created</param>
    /// <returns>The converted Transaction Data</returns>
    private static TransactionData PopulateFromTransaction(Transaction transaction, TransactionData? existingTransactionData)
    {
        TransactionData newTransactionData = new TransactionData
        {
            Id = transaction.Id,
            AccountingDate = transaction.AccountingDate,
            DebitDetail = transaction.DebitDetail != null ? PopulateFromTransactionDetail(transaction.DebitDetail) : null,
            CreditDetail = transaction.CreditDetail != null ? PopulateFromTransactionDetail(transaction.CreditDetail) : null,
            AccountingEntries = transaction.AccountingEntries.Select(entry =>
                new AccountingEntryData
                {
                    Amount = entry.Amount,
                }).ToList()
        };
        existingTransactionData?.Replace(newTransactionData);

        TransactionData transactionData = existingTransactionData ?? newTransactionData;
        return transactionData;
    }

    /// <summary>
    /// Converts the provided <see cref="TransactionDetail"/> entity into a <see cref="TransactionDetailData"/> data object
    /// </summary>
    /// <param name="transactionDetail">Transaction Detail entity to convert</param>
    /// <returns>The converted Transaction Detail Data</returns>
    private static TransactionDetailData PopulateFromTransactionDetail(TransactionDetail transactionDetail) =>
        new TransactionDetailData
        {
            Id = transactionDetail.Id,
            AccountId = transactionDetail.AccountId,
            StatementDate = transactionDetail.StatementDate,
            IsPosted = transactionDetail.IsPosted,
        };

    /// <inheritdoc/>
    private sealed record TransactionRecreateRequest : IRecreateTransactionRequest
    {
        /// <inheritdoc/>
        public required Guid Id { get; init; }

        /// <inheritdoc/>
        public required DateOnly AccountingDate { get; init; }

        /// <inheritdoc/>
        public IRecreateTransactionDetailRequest? DebitDetail { get; init; }

        /// <inheritdoc/>
        public IRecreateTransactionDetailRequest? CreditDetail { get; init; }

        /// <inheritdoc/>
        public required IEnumerable<decimal> AccountingEntries { get; init; }
    }

    /// <inheritdoc/>
    private sealed record TransactionDetailRecreateRequest : IRecreateTransactionDetailRequest
    {
        /// <inheritdoc/>
        public required Guid Id { get; init; }

        /// <inheritdoc/>
        public required Guid AccountId { get; init; }

        /// <inheritdoc/>
        public DateOnly? StatementDate { get; init; }

        /// <inheritdoc/>
        public required bool IsPosted { get; init; }
    }
}