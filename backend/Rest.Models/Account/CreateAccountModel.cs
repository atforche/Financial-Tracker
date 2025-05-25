using Domain.Accounts;
using Rest.Models.FundAmount;

namespace Rest.Models.Account;

/// <summary>
/// REST model representing a request to create an Account
/// </summary>
public class CreateAccountModel
{
    /// <inheritdoc cref="Domain.Accounts.Account.Name"/>
    public required string Name { get; init; }

    /// <inheritdoc cref="Domain.Accounts.Account.Type"/>
    public required AccountType Type { get; init; }

    /// <inheritdoc cref="Domain.BalanceEvents.BalanceEvent.AccountingPeriodId"/>
    public required Guid AccountingPeriodId { get; init; }

    /// <inheritdoc cref="Domain.BalanceEvents.BalanceEvent.EventDate"/>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Starting Fund Balances for this Account 
    /// </summary>
    public required IReadOnlyCollection<CreateFundAmountModel> StartingFundBalances { get; init; }
}