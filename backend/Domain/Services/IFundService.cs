using Domain.Aggregates.Funds;

namespace Domain.Services;

/// <summary>
/// Interface representing a service used to create or modify Funds
/// </summary>
public interface IFundService
{
    /// <summary>
    /// Creates a new Fund with the provided properties
    /// </summary>
    /// <param name="name">Name for this Fund</param>
    /// <returns>The newly created Fund</returns>
    Fund CreateNewFund(string name);
}