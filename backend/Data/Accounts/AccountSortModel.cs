using Domain.Accounts;

namespace Data.Accounts;

/// <summary>
/// Model representing information about an Account required for sorting
/// </summary>
internal sealed class AccountSortModel
{
    /// <summary>
    /// Account
    /// </summary>
    public required Account Account { get; init; }

    /// <summary>
    /// Posted balance for the Account
    /// </summary>
    public required decimal PostedBalance { get; init; }

    /// <summary>
    /// Available to spend balance for the Account
    /// </summary>
    public required decimal AvailableToSpend { get; init; }
}