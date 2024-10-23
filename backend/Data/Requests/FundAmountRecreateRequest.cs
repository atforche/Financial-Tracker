using Domain.ValueObjects;

namespace Data.Requests;

/// <inheritdoc/>
internal sealed record FundAmountRecreateRequest : IRecreateFundAmountRequest
{
    /// <inheritdoc/>
    public required Guid FundId { get; init; }

    /// <inheritdoc/>
    public required decimal Amount { get; init; }
}