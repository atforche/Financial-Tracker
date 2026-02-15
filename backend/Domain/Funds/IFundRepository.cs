using System.Diagnostics.CodeAnalysis;

namespace Domain.Funds;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="Fund"/>
/// </summary>
public interface IFundRepository
{
    /// <summary>
    /// Gets all the Funds currently in the repository
    /// </summary>
    IReadOnlyCollection<Fund> GetAll(GetAllFundsRequest request);

    /// <summary>
    /// Gets the Fund with the specified ID.
    /// </summary>
    Fund GetById(FundId id);

    /// <summary>
    /// Attempts to get the Fund with the specified ID
    /// </summary>
    bool TryGetById(Guid id, [NotNullWhen(true)] out Fund? fund);

    /// <summary>
    /// Attempts to get the Fund with the specified name
    /// </summary>
    bool TryGetByName(string name, [NotNullWhen(true)] out Fund? fund);

    /// <summary>
    /// Adds the provided Fund to the repository
    /// </summary>
    void Add(Fund fund);

    /// <summary>
    /// Deletes the provided Fund from the repository
    /// </summary>
    void Delete(Fund fund);
}

/// <summary>
/// Request to retrieve all Funds
/// </summary>
public record GetAllFundsRequest
{
    /// <summary>
    /// Sort order to apply to the results
    /// </summary>
    public FundSortOrder? SortBy { get; init; }

    /// <summary>
    /// Fund names to include in the results
    /// </summary>
    public IReadOnlyCollection<string>? Names { get; init; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip
    /// </summary>
    public int? Offset { get; init; }
}