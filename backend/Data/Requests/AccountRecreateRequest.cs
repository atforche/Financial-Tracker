using Domain.Entities;

namespace Data.Requests;

/// <inheritdoc/>
internal sealed record AccountRecreateRequest : IRecreateAccountRequest
{
    /// <inheritdoc/>
    public required Guid Id { get; init; }

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public required AccountType Type { get; init; }
};