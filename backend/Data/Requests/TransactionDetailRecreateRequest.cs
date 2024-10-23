using Domain.Entities;

namespace Data.Requests;

/// <inheritdoc/>
internal sealed record TransactionDetailRecreateRequest : IRecreateTransactionDetailRequest
{
    /// <inheritdoc/>
    public required Guid Id { get; init; }

    /// <inheritdoc/>
    public required Guid AccountId { get; init; }

    /// <inheritdoc/>
    public DateOnly? StatementDate { get; init; }

    /// <inheritdoc/>
    public required bool IsPosted { get; init; }
}