using Domain.Entities;

namespace Data.Requests;

/// <inheritdoc/>
internal sealed record FundRecreateRequest : IRecreateFundRequest
{
    /// <inheritdoc/>
    public required Guid Id { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }
}