namespace Domain.Funds;

/// <summary>
/// Entity class representing a Fund
/// </summary>
/// <remarks>
/// A Fund represents a grouping of money that can be spread across multiple Accounts. 
/// The balance of each Account may be made up of money from multiple Funds. The balance of a Fund
/// over time can be used to track financial changes in an Account-agnostic way.
/// </remarks>
public class Fund : Entity<FundId>
{
    /// <summary>
    /// Name for this Fund
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Description for this Funds
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="name">Name for this Fund</param>
    /// <param name="description">Description for this Fund</param>
    internal Fund(string name, string description)
        : base(new FundId(Guid.NewGuid()))
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private Fund()
        : base()
    {
        Name = "";
        Description = "";
    }
}