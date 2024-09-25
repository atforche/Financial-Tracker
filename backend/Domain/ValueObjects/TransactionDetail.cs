using Domain.Factories;

namespace Domain.ValueObjects;

/// <summary>
/// Class representing a Transaction Detail value object
/// </summary>
public class TransactionDetail
{
    /// <summary>
    /// Account ID for this Transaction Detail
    /// </summary>
    public Guid AccountId { get; }

    /// <summary>
    /// Statement date for this Transaction Detail
    /// </summary>
    public DateOnly? StatementDate { get; }

    /// <summary>
    /// Is posted flag for this Transaction Detail
    /// </summary>
    public bool IsPosted { get; }

    /// <summary>
    /// Validates the current Transaction Detail
    /// </summary>
    private void Validate()
    {
        if (AccountId == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (IsPosted && StatementDate == null)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Factory responsible for constructing instances of a Transaction Detail
    /// </summary>
    public class TransactionDetailFactory : ITransactionDetailFactory
    {
        /// <inheritdoc/>
        public TransactionDetail Create(CreateTransactionDetailRequest request)
        {
            var transactionDetail = new TransactionDetail(request.Account.Id,
                request.StatementDate,
                request.IsPosted);
            transactionDetail.Validate();
            return transactionDetail;
        }

        /// <inheritdoc/>
        public TransactionDetail Recreate(IRecreateTransactionDetailRequest request) =>
            new TransactionDetail(request.AccountId,
                request.StatementDate,
                request.IsPosted);
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="accountId">Account ID for this Transaction Detail</param>
    /// <param name="statementDate">Statement date for this Transaction Detail</param>
    /// <param name="isPosted">Is posted flag for this Transaction Detail</param>
    private TransactionDetail(Guid accountId, DateOnly? statementDate, bool isPosted)
    {
        AccountId = accountId;
        StatementDate = statementDate;
        IsPosted = isPosted;
        Validate();
    }
}