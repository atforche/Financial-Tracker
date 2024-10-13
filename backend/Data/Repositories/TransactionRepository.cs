using Data.EntityModels;
using Data.ValueObjectModels;
using Domain.Entities;
using Domain.Factories;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

/// <summary>
/// Transaction repository that allows Transactions to be persisted to the database
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly DatabaseContext _context;
    private readonly ITransactionFactory _transactionFactory;

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="context">Context to use to connect to the database</param>
    /// <param name="transactionFactory">Factory used to contruct Transaction instances</param>
    public TransactionRepository(DatabaseContext context, ITransactionFactory transactionFactory)
    {
        _context = context;
        _transactionFactory = transactionFactory;
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
    public Transaction? FindOrNull(Guid id)
    {
        TransactionData? transactionData = _context.Transactions.FirstOrDefault(transaction => transaction.Id == id);
        return transactionData != null ? ConvertToEntity(transactionData) : null;
    }

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

    /// <inheritdoc/>
    public void Update(Transaction transaction)
    {
        TransactionData transactionData = _context.Transactions.Single(transactionData => transactionData.Id == transaction.Id);
        PopulateFromTransaction(transaction, transactionData);
    }

    /// <inheritdoc/>
    public void Delete(Guid id)
    {
        TransactionData transactionData = _context.Transactions
            .Include(transaction => transaction.AccountingEntries)
            .Single(transactionData => transactionData.Id == id);
        foreach (AccountingEntryData accountingEntry in transactionData.AccountingEntries)
        {
            _context.AccountingEntries.Remove(accountingEntry);
        }
        _context.Transactions.Remove(transactionData);
    }

    /// <summary>
    /// Converts the provided <see cref="TransactionData"/> object into a <see cref="Transaction"/> domain entity.
    /// </summary>
    /// <param name="transactionData">Transaction Data to be converted</param>
    /// <returns>The converted Transaction domain entity</returns>
    private Transaction ConvertToEntity(TransactionData transactionData) => _transactionFactory.Recreate(
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
            AccountId = transactionDetailData.AccountId,
            StatementDate = transactionDetailData.StatementDate,
            IsPosted = transactionDetailData.IsPosted,
        };

    /// <summary>
    /// Converts the provided <see cref="Transaction"/> domain entity into a <see cref="TransactionData"/> data object
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
    /// Converts the provided <see cref="TransactionDetail"/> domain value onject into a <see cref="TransactionDetailData"/> data model
    /// </summary>
    /// <param name="transactionDetail">Transaction Detail value object to convert</param>
    /// <returns>The converted Transaction Detail Data</returns>
    private static TransactionDetailData PopulateFromTransactionDetail(TransactionDetail transactionDetail) =>
        new TransactionDetailData
        {
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
        public required Guid AccountId { get; init; }

        /// <inheritdoc/>
        public DateOnly? StatementDate { get; init; }

        /// <inheritdoc/>
        public required bool IsPosted { get; init; }
    }
}