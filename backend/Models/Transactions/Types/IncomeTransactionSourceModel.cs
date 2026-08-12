using Models.Accounts;
using Models.Income;

namespace Models.Transactions.Types;

/// <summary>
/// Model representing the source of an income transaction response.
/// </summary>
public sealed class IncomeTransactionSourceModel
{
    /// <summary>
    /// Optional account for the source.
    /// </summary>
    public AccountBalanceEventModel? Account { get; init; }

    /// <summary>
    /// Optional location for the source.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Economic composition of the income receipt.
    /// </summary>
    public required IncomeBreakdownModel Income { get; init; }
}