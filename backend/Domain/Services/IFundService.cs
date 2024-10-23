using Domain.Entities;

namespace Domain.Services;

/// <summary>
/// Interface representing a service used to create or modify Funds
/// </summary>
public interface IFundService
{
    /// <summary>
    /// Creates a new Fund with the provided properties
    /// </summary>
    /// <param name="createFundRequest">Request to create a Fund</param>
    /// <returns>The newly created Fund</returns>
    Fund CreateNewFund(CreateFundRequest createFundRequest);
}

/// <summary>
/// Record representing a request to create a Fund
/// </summary>
public record CreateFundRequest
{
    /// <inheritdoc cref="Fund.Name"/>
    public required string Name { get; init; }
}