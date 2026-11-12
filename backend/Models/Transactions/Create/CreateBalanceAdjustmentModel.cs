namespace Models.Transactions.Create;

/// <summary>
/// Model representing a request to create a balance adjustment. Its amount is a signed change to the account's
/// balance as it is displayed.
/// </summary>
public sealed class CreateBalanceAdjustmentModel : CreateTransactionModel
{
    /// <summary>
    /// ID of the account whose balance is adjusted.
    /// </summary>
    public required Guid AccountId { get; init; }
}
