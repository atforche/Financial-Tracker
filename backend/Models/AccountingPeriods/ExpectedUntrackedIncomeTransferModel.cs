namespace Models.AccountingPeriods;

/// <summary>
/// An expected transfer to an untracked account.
/// </summary>
public sealed class ExpectedUntrackedIncomeTransferModel
{
    /// <summary>
    /// Description of the transfer.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Expected transfer amount for each payment.
    /// </summary>
    public required decimal Amount { get; init; }
}