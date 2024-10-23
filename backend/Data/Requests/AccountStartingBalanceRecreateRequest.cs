using Domain.Entities;
using Domain.ValueObjects;

namespace Data.Requests;

/// <inheritdoc/>
internal sealed record AccountStartingBalanceRecreateRequest : IRecreateAccountStartingBalanceRequest
{
    /// <inheritdoc/>
    public required Guid Id { get; init; }

    /// <inheritdoc/>
    public required Guid AccountId { get; init; }

    /// <inheritdoc/>
    public required Guid AccountingPeriodId { get; init; }

    /// <inheritdoc/>
    public required IEnumerable<IRecreateFundAmountRequest> StartingFundBalances { get; init; }
};