using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Entity class representing a Transaction
/// </summary>
/// <remarks>
/// A Transaction represents an event where money moves in one of the following ways:
/// 1. Money is debited from an Account
/// 2. Money is credited to an Account
/// 3. Money is debited from one Account and credited to another Account
/// If money moves from one Account to another, the amount debited is equal to the amount credited. 
/// </remarks>
public class Transaction
{
    /// <summary>
    /// ID for this Transaction
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Accounting Date for this Transaction
    /// </summary>
    /// <remarks>
    /// The Accounting Date is the date that a Transaction actually took place, regardless of when it
    /// appears on either the debited or credited Accounts. For example, if a purchase is actually made 
    /// on a Friday but the debited Account shows the transaction as being made on Saturday, 
    /// the accounting date would be the date that corresponds to Friday.
    /// </remarks>
    public DateOnly AccountingDate { get; }

    /// <summary>
    /// Debit Transaction Detail for this Transaction
    /// </summary>
    public TransactionDetail? DebitDetail { get; }

    /// <summary>
    /// Credit Transaction Detail for this Transaction
    /// </summary>
    public TransactionDetail? CreditDetail { get; }

    /// <summary>
    /// List of Accounting Entries for this Transaction
    /// </summary>
    public ICollection<FundAmount> AccountingEntries { get; }

    /// <summary>
    /// Reconstructs an existing Transaction
    /// </summary>
    /// <param name="request">Request to recreate a Transaction</param>
    public Transaction(IRecreateTransactionRequest request)
    {
        Id = request.Id;
        AccountingDate = request.AccountingDate;
        DebitDetail = request.DebitDetail != null ? new TransactionDetail(request.DebitDetail) : null;
        CreditDetail = request.CreditDetail != null ? new TransactionDetail(request.CreditDetail) : null;
        AccountingEntries = request.AccountingEntries
            .Select(request => new FundAmount(request.FundId, request.Amount)).ToList();
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="request">Request to create a Transaction</param>
    public Transaction(CreateTransactionRequest request)
    {
        Id = Guid.NewGuid();
        AccountingDate = request.AccountingDate;
        DebitDetail = request.DebitDetail != null
            ? new TransactionDetail(request.DebitDetail.Account, request.DebitDetail.StatementDate, request.DebitDetail.IsPosted)
            : null;
        CreditDetail = request.CreditDetail != null
            ? new TransactionDetail(request.CreditDetail.Account, request.CreditDetail.StatementDate, request.CreditDetail.IsPosted)
            : null;
        AccountingEntries = request.AccountingEntries
            .Select(request => new FundAmount(request.Fund.Id, request.Amount)).ToList();
        Validate();
    }

    /// <summary>
    /// Validates the current Transaction
    /// </summary>
    private void Validate()
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
        if (AccountingEntries.Count == 0 ||
            AccountingEntries.Sum(entry => entry.Amount) == 0 ||
            AccountingEntries.GroupBy(entry => entry.FundId).Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException();
        }
    }
}

/// <summary>
/// Record representing a request to create a Transaction
/// </summary>
public record CreateTransactionRequest
{
    /// <inheritdoc cref="Transaction.AccountingDate"/>
    public required DateOnly AccountingDate { get; init; }

    /// <inheritdoc cref="Transaction.DebitDetail"/>
    public CreateTransactionDetailRequest? DebitDetail { get; init; }

    /// <inheritdoc cref="Transaction.CreditDetail"/>
    public CreateTransactionDetailRequest? CreditDetail { get; init; }

    /// <inheritdoc cref="Transaction.AccountingEntries"/>
    public required IEnumerable<CreateFundAmountRequest> AccountingEntries { get; init; }
}

/// <summary>
/// Interface representing a request to recreate an existing Transaction
/// </summary>
public interface IRecreateTransactionRequest
{
    /// <inheritdoc cref="Transaction.Id"/>
    Guid Id { get; }

    /// <inheritdoc cref="Transaction.AccountingDate"/>
    DateOnly AccountingDate { get; }

    /// <inheritdoc cref="Transaction.DebitDetail"/>
    IRecreateTransactionDetailRequest? DebitDetail { get; }

    /// <inheritdoc cref="Transaction.CreditDetail"/>
    IRecreateTransactionDetailRequest? CreditDetail { get; }

    /// <inheritdoc cref="Transaction.AccountingEntries"/>
    IEnumerable<IRecreateFundAmountRequest> AccountingEntries { get; }
}