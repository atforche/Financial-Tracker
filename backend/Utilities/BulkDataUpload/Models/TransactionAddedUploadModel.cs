using Rest.Models.Accounts;
using Rest.Models.Transactions;

namespace Utilities.BulkDataUpload.Models;

/// <summary>
/// Bulk data upload model representing a Transaction Added Balance Event
/// </summary>
internal sealed class TransactionAddedUploadModel : BalanceEventUploadModel
{
    /// <summary>
    /// Transaction Date for this Transaction
    /// </summary>
    public required DateOnly TransactionDate { get; init; }

    /// <summary>
    /// Name of the Debit Account for this Transaction
    /// </summary>
    public string? DebitAccountName { get; init; }

    /// <summary>
    /// Name of the Credit Account for this Transaction
    /// </summary>
    public string? CreditAccountName { get; init; }

    /// <summary>
    /// Fund Amounts for this Transaction
    /// </summary>
    public required ICollection<FundAmountUploadModel> FundAmounts { get; init; }

    /// <summary>
    /// Gets a Create Transaction Model corresponding to this Transaction Added Upload Model
    /// </summary>
    /// <param name="existingAccounts">List of existing Accounts</param>
    /// <returns>A Create Transaction Model corresponding to this Transaction Added Upload Model</returns>
    public CreateTransactionModel GetAsCreateTransactionModel(
        ICollection<AccountModel> existingAccounts) => new()
        {
            AccountingPeriodId = Guid.NewGuid(),
            Date = TransactionDate,
            DebitAccountId = DebitAccountName != null
                ? existingAccounts.Single(account => account.Name == DebitAccountName).Id
                : null,
            CreditAccountId = CreditAccountName != null
                ? existingAccounts.Single(account => account.Name == CreditAccountName).Id
                : null,
        };
}