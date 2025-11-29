using System.Diagnostics.CodeAnalysis;

namespace Domain.Funds;

/// <summary>
/// Interface representing methods to interact with a collection of <see cref="Fund"/>
/// </summary>
public interface IFundRepository
{
    /// <summary>
    /// Finds all the Funds currently in the repository
    /// </summary>
    /// <returns>All the Funds in the repository</returns>
    IReadOnlyCollection<Fund> FindAll();

    /// <summary>
    /// Finds the Fund with the specified ID.
    /// </summary>
    /// <param name="id">ID of the Fund to find</param>
    /// <returns>The Fund that was found</returns>
    Fund FindById(FundId id);

    /// <summary>
    /// Attempts to find the Fund with the specified ID
    /// </summary>
    /// <param name="id">ID of the Fund to find</param>
    /// <param name="fund">The Fund was was found, or null if one wasn't found</param>
    /// <returns>True if the Fund was found, false otherwise</returns>
    bool TryFindById(Guid id, [NotNullWhen(true)] out Fund? fund);

    /// <summary>
    /// Attempts to find the Fund with the specified name
    /// </summary>
    /// <param name="name">Name of the Fund to find</param>
    /// <param name="fund">The found Fund, or null if one wasn't found</param>
    /// <returns>True if the Fund was found, false otherwise</returns>
    bool TryFindByName(string name, [NotNullWhen(true)] out Fund? fund);

    /// <summary>
    /// Adds the provided Fund to the repository
    /// </summary>
    /// <param name="fund">Fund that should be added</param>
    void Add(Fund fund);

    /// <summary>
    /// Deletes the provided Fund from the repository
    /// </summary>
    /// <param name="fund">Fund to be deleted</param>
    void Delete(Fund fund);
}