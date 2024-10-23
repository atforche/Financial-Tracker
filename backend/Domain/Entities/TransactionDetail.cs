namespace Domain.Entities;

/// <summary>
/// Entity class representing a Transaction Detail
/// </summary>
/// <remarks>
/// A Transaction Detail represents the details of a Transaction that are specific
/// to only one of the Accounts that the Transaction effects. For example, 
/// a Transaction may appear as posted for the Account that it is debiting 
/// before it appears as posted for the Account that it is crediting.
/// </remarks>
public class TransactionDetail
{
    /// <summary>
    /// ID for this Transaction Detail
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Account ID for this Transaction Detail
    /// </summary>
    public Guid AccountId { get; }

    /// <summary>
    /// Statement Date for this Transaction Detail
    /// </summary>
    /// <remarks>
    /// The Statement Date represents the date that the Account believes that the Transaction occurred on
    /// For example, if a purchase is actually made on a Friday but the Account shows the transaction as 
    /// being made on Saturday, the statement date would be the date that corresponds to Saturday.
    /// </remarks>
    public DateOnly? StatementDate { get; }

    /// <summary>
    /// Is Posted flag for this Transaction Detail
    /// </summary>
    public bool IsPosted { get; }

    /// <summary>
    /// Reconstructs an existing Transaction Detail
    /// </summary>
    /// <param name="request">Request to recreate a Transaction Detail</param>
    public TransactionDetail(IRecreateTransactionDetailRequest request)
    {
        Id = request.Id;
        AccountId = request.AccountId;
        StatementDate = request.StatementDate;
        IsPosted = request.IsPosted;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="account">Account for this Transaction Detail</param>
    /// <param name="statementDate">Statement Date for this Transaction Detail</param>
    /// <param name="isPosted">Is Posted flag for this Transaction Detail</param>
    internal TransactionDetail(Account account, DateOnly? statementDate, bool isPosted)
    {
        Id = Guid.NewGuid();
        AccountId = account.Id;
        StatementDate = statementDate;
        IsPosted = isPosted;
        Validate();
    }

    /// <summary>
    /// Validates the current Transaction Detail
    /// </summary>
    private void Validate()
    {
        if (Id == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (AccountId == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (IsPosted && StatementDate == null)
        {
            throw new InvalidOperationException();
        }
    }
}

/// <summary>
/// Record representing a request to create a Transaction Detail
/// </summary>
public record CreateTransactionDetailRequest
{
    /// <inheritdoc cref="TransactionDetail.AccountId"/>
    public required Account Account { get; init; }

    /// <inheritdoc cref="TransactionDetail.StatementDate"/>
    public DateOnly? StatementDate { get; init; }

    /// <inheritdoc cref="TransactionDetail.IsPosted"/>
    public required bool IsPosted { get; init; }
}

/// <summary>
/// Interface representing a request to recreate an existing Transaction Detail
/// </summary>
public interface IRecreateTransactionDetailRequest
{
    /// <inheritdoc cref="TransactionDetail.Id"/>
    Guid Id { get; }

    /// <inheritdoc cref="TransactionDetail.AccountId"/>
    Guid AccountId { get; }

    /// <inheritdoc cref="TransactionDetail.StatementDate"/>
    DateOnly? StatementDate { get; }

    /// <inheritdoc cref="TransactionDetail.IsPosted"/>
    bool IsPosted { get; }
}