namespace Models.Funds;

/// <summary>
/// Model representing a Fund amount assigned by an income transaction.
/// </summary>
public sealed class CreateIncomeFundAmountModel : CreateFundAmountModel
{
    /// <summary>
    /// Whether this assignment is extra funding outside the regular monthly
    /// contribution.
    /// </summary>
    public bool IsExtraContribution { get; init; }
}