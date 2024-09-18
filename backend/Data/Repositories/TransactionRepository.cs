using Data.Models;
using Domain.Entities;
using Domain.Events;
using Domain.Factories;
using Domain.Repositories;
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
            .Where(transaction => transaction.AccountingDate.Year == accountingPeriod.Year &&
            transaction.AccountingDate.Month == accountingPeriod.Month)
            .Select(ConvertToEntity).ToList();

    /// <inheritdoc/>
    public IReadOnlyCollection<Transaction> FindAllByAccount(Guid accountId) =>
        _context.Transactions
            .Include(transaction => transaction.AccountingEntries)
            .Where(transaction => transaction.AccountId == accountId)
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
    /// <param name="transactionData">TransactionData to be converted</param>
    /// <returns>The converted Transaction domain entity</returns>
    private Transaction ConvertToEntity(TransactionData transactionData) => _transactionFactory.Recreate(
        new TransactionRecreateRequest
        {
            Id = transactionData.Id,
            AccountingDate = transactionData.AccountingDate,
            StatementDate = transactionData.StatementDate,
            Type = transactionData.Type,
            IsPosted = transactionData.IsPosted,
            AccountId = transactionData.AccountId,
            AccountingEntries = transactionData.AccountingEntries.Select(entry =>
                new AccountingEntryRecreateRequest
                {
                    Id = entry.Id,
                    Type = entry.Type,
                    Amount = entry.Amount
                })
        });

    /// <summary>
    /// Converts the provided <see cref="Transaction"/> domain entity into a <see cref="TransactionData"/> data object
    /// </summary>
    /// <param name="transaction">Transaction entity to convert</param>
    /// <param name="existingTransactionData">Existing TransactionData model to populate from the entity, or null if a new model should be created</param>
    /// <returns>The converted TransactionData</returns>
    private static TransactionData PopulateFromTransaction(Transaction transaction, TransactionData? existingTransactionData)
    {
        TransactionData newTransactionData = new TransactionData
        {
            Id = transaction.Id,
            AccountingDate = transaction.AccountingDate,
            StatementDate = transaction.StatementDate,
            Type = transaction.Type,
            IsPosted = transaction.IsPosted,
            AccountId = transaction.AccountId,
            AccountingEntries = transaction.AccountingEntries.Select(entry =>
                new AccountingEntryData
                {
                    Id = entry.Id,
                    Type = entry.Type,
                    Amount = entry.Amount,
                }).ToList()
        };
        existingTransactionData?.Replace(newTransactionData);

        TransactionData transactionData = existingTransactionData ?? newTransactionData;
        foreach (IDomainEvent domainEvent in transaction.GetDomainEvents())
        {
            transactionData.RaiseEvent(domainEvent);
        }
        return transactionData;
    }

    /// <summary>
    /// Record representing a request to recreate a Transaction
    /// </summary>
    private sealed record TransactionRecreateRequest : IRecreateTransactionRequest
    {
        /// <inheritdoc/>
        public required Guid Id { get; init; }

        /// <inheritdoc/>
        public required DateOnly AccountingDate { get; init; }

        /// <inheritdoc/>
        public DateOnly? StatementDate { get; init; }

        /// <inheritdoc/>
        public required TransactionType Type { get; init; }

        /// <inheritdoc/>
        public required bool IsPosted { get; init; }

        /// <inheritdoc/>
        public required Guid AccountId { get; init; }

        /// <inheritdoc/>
        public required IEnumerable<IRecreateAccountingEntryRequest> AccountingEntries { get; init; }
    }

    /// <summary>
    /// Record representing a request to recreate an Accounting Entry
    /// </summary>
    private sealed record AccountingEntryRecreateRequest : IRecreateAccountingEntryRequest
    {
        /// <inheritdoc/>
        public required Guid Id { get; init; }

        /// <inheritdoc/>
        public required AccountingEntryType Type { get; init; }

        /// <inheritdoc/>
        public required decimal Amount { get; init; }
    }
}