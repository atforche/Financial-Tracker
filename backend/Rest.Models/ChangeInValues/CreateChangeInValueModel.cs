using Domain.ChangeInValues;
using Rest.Models.Funds;

namespace Rest.Models.ChangeInValues;

/// <summary>
/// REST model representing a request to create a <see cref="ChangeInValue"/>
/// </summary>
public class CreateChangeInValueModel
{
    /// <inheritdoc cref="ChangeInValue.AccountingPeriodId"/>
    public required Guid AccountingPeriodId { get; init; }

    /// <inheritdoc cref="ChangeInValue.EventDate"/>
    public required DateOnly EventDate { get; init; }

    /// <inheritdoc cref="ChangeInValue.AccountId"/>
    public required Guid AccountId { get; init; }

    /// <inheritdoc cref="ChangeInValue.FundAmount"/>
    public required CreateFundAmountModel FundAmount { get; init; }
}