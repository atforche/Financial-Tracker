using RestApi.Models.Account;
using RestApi.Models.AccountingPeriod;

namespace Utilities.BulkDataUpload.Models;

/// <summary>
/// Bulk data upload model representing a Transaction Posted Balance Event
/// </summary>
public class TransactionPostedUploadModel : BalanceEventUploadModel
{
    /// <summary>
    /// Transaction Upload ID for this Transaction Posted Balance Event
    /// </summary>
    public required Guid TransactionId { get; init; }

    /// <summary>
    /// Event Date for this Transaction Posted Balance Event
    /// </summary>
    public required DateOnly EventDate { get; init; }

    /// <summary>
    /// Account Name for this Transaction Posted Balance Event
    /// </summary>
    public required string AccountName { get; init; }

    /// <summary>
    /// Gets the ID of the Transaction to post
    /// </summary>
    /// <param name="existingTransactions">Dictionary mapping the upload ID of a Transaction to its Transaction Model</param>
    /// <returns>The ID of the Transaction to post</returns>
    public Guid GetTransactionIdToPost(Dictionary<Guid, TransactionModel> existingTransactions) =>
        existingTransactions[TransactionId].Id;

    /// <summary>
    /// Gets a Post Transaction Model corresponding to this Transaction Posted Upload Model
    /// </summary>
    /// <param name="existingAccounts">List of existing Accounts</param>
    /// <returns>A Post Transaction Model corresponding to this Post Transaction Upload Model</returns>
    public PostTransactionModel GetAsPostTransactionModel(ICollection<AccountModel> existingAccounts) =>
        new PostTransactionModel
        {
            AccountId = existingAccounts.Single(account => account.Name == AccountName).Id,
            PostedStatementDate = EventDate,
        };
}