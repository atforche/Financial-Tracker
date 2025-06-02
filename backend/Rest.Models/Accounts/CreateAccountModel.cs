using Domain.Accounts;
using Rest.Models.Funds;

namespace Rest.Models.Accounts;

/// <summary>
/// REST model representing a request to create an <see cref="Account"/>
/// </summary>
public class CreateAccountModel
{
    /// <inheritdoc cref="Account.Name"/>
    public required string Name { get; init; }

    /// <inheritdoc cref="Account.Type"/>
    public required AccountType Type { get; init; }

    /// <inheritdoc cref="AccountAddedBalanceEvent.AccountingPeriodId"/>
    public required Guid AccountingPeriodId { get; init; }

    /// <inheritdoc cref="AccountAddedBalanceEvent.EventDate"/>
    public required DateOnly Date { get; init; }

    /// <inheritdoc cref="AccountAddedBalanceEvent.FundAmounts"/>
    public required IReadOnlyCollection<CreateFundAmountModel> StartingFundBalances { get; init; }
}