using Domain.Transactions.Income;

namespace Domain.Transactions.IncomeTransfer;

/// <summary>
/// Record representing a request to update an <see cref="IncomeTransferTransaction"/>
/// </summary>
public record UpdateIncomeTransferTransactionRequest : UpdateIncomeTransactionRequest
{
    /// <summary>
    /// Posted Date for the Debit Account of this Income Transfer Transaction
    /// </summary>
    public DateOnly? DebitPostedDate { get; init; }
}