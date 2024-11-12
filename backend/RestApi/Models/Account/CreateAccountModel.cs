using Domain.Aggregates.Accounts;
using RestApi.Models.FundAmount;

namespace RestApi.Models.Account;

/// <summary>
/// REST model representing a request to create an Account
/// </summary>
public class CreateAccountModel
{
    /// <inheritdoc cref="Domain.Aggregates.Accounts.Account.Name"/>
    public required string Name { get; init; }

    /// <inheritdoc cref="Domain.Aggregates.Accounts.Account.Type"/>
    public required AccountType Type { get; init; }

    /// <summary>
    /// Starting Fund Balances for this Account 
    /// </summary>
    public required IReadOnlyCollection<CreateFundAmountModel> StartingFundBalances { get; init; }
}