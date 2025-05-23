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
    /// <returns>The Fund that was found, or null if one wasn't found</returns>
    Fund? FindByIdOrNull(FundId id);

    /// <summary>
    /// Finds the Fund with the specified name
    /// </summary>
    /// <param name="name">Name of the Fund to find</param>
    /// <returns>The Fund that was found, or null if one wasn't found</returns>
    Fund? FindByNameOrNull(string name);

    /// <summary>
    /// Adds the provided Fund to the repository
    /// </summary>
    /// <param name="fund">Fund that should be added</param>
    void Add(Fund fund);
}