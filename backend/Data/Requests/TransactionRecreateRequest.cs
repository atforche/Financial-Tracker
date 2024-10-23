using Domain.Entities;
using Domain.ValueObjects;

namespace Data.Requests;

/// <inheritdoc/>
internal sealed record TransactionRecreateRequest : IRecreateTransactionRequest
{
    /// <inheritdoc/>
    public required Guid Id { get; init; }

    /// <inheritdoc/>
    public required DateOnly AccountingDate { get; init; }

    /// <inheritdoc/>
    public IRecreateTransactionDetailRequest? DebitDetail { get; init; }

    /// <inheritdoc/>
    public IRecreateTransactionDetailRequest? CreditDetail { get; init; }

    /// <inheritdoc/>
    public required IEnumerable<IRecreateFundAmountRequest> AccountingEntries { get; init; }
}