using Domain.Factories;

namespace Domain.Entities;

/// <summary>
/// Entity class representing a Transaction
/// </summary>
public class Transaction : Entity
{
    /// <summary>
    /// Id for this Transaction
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Accounting date for this Transaction
    /// </summary>
    public DateOnly AccountingDate { get; }

    /// <summary>
    /// Statement date for this Transaction
    /// </summary>
    public DateOnly? StatementDate { get; }

    /// <summary>
    /// Type for this Transaction
    /// </summary>
    public TransactionType Type { get; }

    /// <summary>
    /// Is posted flag for this Transaction
    /// </summary>
    public bool IsPosted { get; }

    /// <summary>
    /// Id of the Account associated with this Transaction
    /// </summary>
    public Guid AccountId { get; }

    /// <summary>
    /// List of Accounting Entries that are associated with this Transaction
    /// </summary>
    public ICollection<AccountingEntry> AccountingEntries { get; }

    /// <summary>
    /// Validates the current Transaction
    /// </summary>
    public void Validate()
    {
        if (Id == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (AccountingDate == DateOnly.MinValue)
        {
            throw new InvalidOperationException();
        }
        if (IsPosted && StatementDate == null)
        {
            throw new InvalidOperationException();
        }
        if (AccountId == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (Type == TransactionType.Debit && AccountingEntries.Any(entry => entry.Type != AccountingEntryType.Debit))
        {
            throw new InvalidOperationException();
        }
        if (Type == TransactionType.Credit && AccountingEntries.Any(entry => entry.Type != AccountingEntryType.Credit))
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Factory responsible for constructing instances of a Transaction
    /// </summary>    
    public class TransactionFactory : ITransactionFactory
    {
        private readonly IAccountingEntryFactory _accountingEntryFactory;

        /// <summary>
        /// Constructs a new instance of this class
        /// </summary>
        /// <param name="accountingEntryFactory">Factory responsible for constructing AccountingEntries</param>
        public TransactionFactory(IAccountingEntryFactory accountingEntryFactory)
        {
            _accountingEntryFactory = accountingEntryFactory;
        }

        /// <inheritdoc/>
        public Transaction Create(CreateTransactionRequest request)
        {
            var transaction = new Transaction(Guid.NewGuid(),
                request.AccountingDate,
                request.StatementDate,
                request.Type,
                request.IsPosted,
                request.AccountId,
                request.AccountingEntries.Select(_accountingEntryFactory.Create));
            transaction.Validate();
            return transaction;
        }

        /// <inheritdoc/>
        public Transaction Recreate(IRecreateTransactionRequest request) =>
            new Transaction(request.Id,
                request.AccountingDate,
                request.StatementDate,
                request.Type,
                request.IsPosted,
                request.AccountId,
                request.AccountingEntries.Select(_accountingEntryFactory.Recreate));
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="id">Id for this Transaction</param>
    /// <param name="accountingDate">Accounting date for this Transaction</param>
    /// <param name="statementDate">Statement date for this Transaction</param>
    /// <param name="type">Type for this Transaction</param>
    /// <param name="isPosted">Is posted flag for this Transaction</param>
    /// <param name="accountId">Account Id for this Transaction</param>
    /// <param name="accountingEntries">AccountingEntries for this Transaction</param>
    private Transaction(Guid id,
        DateOnly accountingDate,
        DateOnly? statementDate,
        TransactionType type,
        bool isPosted,
        Guid accountId,
        IEnumerable<AccountingEntry> accountingEntries)
    {
        Id = id;
        AccountingDate = accountingDate;
        StatementDate = statementDate;
        Type = type;
        IsPosted = isPosted;
        AccountId = accountId;
        AccountingEntries = accountingEntries.ToList();
    }
}

/// <summary>
/// Enum representing the different Transaction types
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Transaction that debits money from an Account
    /// </summary>
    Debit,

    /// <summary>
    /// Transaction that credits money to an Account
    /// </summary>
    Credit,
}