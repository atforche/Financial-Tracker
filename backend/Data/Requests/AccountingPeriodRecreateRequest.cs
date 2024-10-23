using Domain.Entities;

namespace Data.Requests;

/// <inheritdoc/>
internal sealed record AccountingPeriodRecreateRequest : IRecreateAccountingPeriodRequest
{
    /// <inheritdoc/>
    public required Guid Id { get; init; }

    /// <inheritdoc/>
    public required int Year { get; init; }

    /// <inheritdoc/>
    public required int Month { get; init; }

    /// <inheritdoc/>
    public required bool IsOpen { get; init; }
};