using Domain.Entities;
using RestApi.Models.FundAmount;

namespace RestApi.Models.Account;

/// <summary>
/// REST model representing a request to create an Account
/// </summary>
public class CreateAccountModel
{
    /// <inheritdoc cref="Domain.Entities.Account.Name"/>
    public required string Name { get; init; }

    /// <inheritdoc cref="Domain.Entities.Account.Type"/>
    public required AccountType Type { get; init; }

    /// <inheritdoc cref="Domain.Entities.AccountStartingBalance.StartingFundBalances"/>
    public required ICollection<CreateFundAmountModel> StartingFundBalances { get; init; }
}