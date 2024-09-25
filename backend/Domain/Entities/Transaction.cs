using Domain.Factories;
using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Entity class representing a Transaction
/// </summary>
public class Transaction
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
    /// Debit detail for this Transaction
    /// </summary>
    public TransactionDetail? DebitDetail { get; }

    /// <summary>
    /// Credit detail for this Transaction
    /// </summary>
    public TransactionDetail? CreditDetail { get; }

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
        if (DebitDetail == null && CreditDetail == null)
        {
            throw new InvalidOperationException();
        }
        if (AccountingEntries.Count == 0)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Factory responsible for constructing instances of a Transaction
    /// </summary>    
    public class TransactionFactory : ITransactionFactory
    {
        private readonly ITransactionDetailFactory _detailFactory;

        /// <summary>
        /// Constructs a new instance of this class
        /// </summary>
        /// <param name="detailFactory">Factory responsible for creating instances of a Transaction Detail</param>
        public TransactionFactory(ITransactionDetailFactory detailFactory)
        {
            _detailFactory = detailFactory;
        }

        /// <inheritdoc/>
        public Transaction Create(CreateTransactionRequest request)
        {
            TransactionDetail? debitDetail = request.DebitDetail != null
                ? _detailFactory.Create(request.DebitDetail)
                : null;
            TransactionDetail? creditDetail = request.CreditDetail != null
                ? _detailFactory.Create(request.CreditDetail)
                : null;
            var transaction = new Transaction(Guid.NewGuid(),
                request.AccountingDate,
                debitDetail,
                creditDetail,
                request.AccountingEntries.Select(amount => new AccountingEntry(amount)));
            transaction.Validate();
            return transaction;
        }

        /// <inheritdoc/>
        public Transaction Recreate(IRecreateTransactionRequest request)
        {
            TransactionDetail? debitDetail = request.DebitDetail != null
                ? _detailFactory.Recreate(request.DebitDetail)
                : null;
            TransactionDetail? creditDetail = request.CreditDetail != null
                ? _detailFactory.Recreate(request.CreditDetail)
                : null;
            return new Transaction(request.Id,
                request.AccountingDate,
                debitDetail,
                creditDetail,
                request.AccountingEntries.Select(amount => new AccountingEntry(amount)));
        }
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="id">Id for this Transaction</param>
    /// <param name="accountingDate">Accounting date for this Transaction</param>
    /// <param name="debitDetail">Debit detail for this transaction</param>
    /// <param name="creditDetail">Credit detail for this transaction</param>
    /// <param name="accountingEntries">AccountingEntries for this Transaction</param>
    private Transaction(Guid id,
        DateOnly accountingDate,
        TransactionDetail? debitDetail,
        TransactionDetail? creditDetail,
        IEnumerable<AccountingEntry> accountingEntries)
    {
        Id = id;
        AccountingDate = accountingDate;
        DebitDetail = debitDetail;
        CreditDetail = creditDetail;
        AccountingEntries = accountingEntries.ToList();
    }
}