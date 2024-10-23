namespace Domain.Entities;

/// <summary>
/// Entity class representing a Fund
/// </summary>
/// <remarks>
/// A Fund represents a grouping of money that can be spread across multiple Accounts. 
/// The balance of each Account may be made up of money from multiple Funds. The balance of a Fund
/// over time can be used to track financial changes in an Account-agnostic way.
/// </remarks>
public class Fund
{
    /// <summary>
    /// ID for this Fund
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Name for this Fund
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Reconstructs an existing Fund
    /// </summary>
    /// <param name="request">Request to recreate a Fund</param>
    public Fund(IRecreateFundRequest request)
    {
        Id = request.Id;
        Name = request.Name;
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="name">Name for this Fund</param>
    internal Fund(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
        Validate();
    }

    /// <summary>
    /// Validates the current Fund
    /// </summary>
    private void Validate()
    {
        if (Id == Guid.Empty)
        {
            throw new InvalidOperationException();
        }
        if (string.IsNullOrEmpty(Name))
        {
            throw new InvalidOperationException();
        }
    }
}

/// <summary>
/// Interface representing a request to recreate an existing Fund
/// </summary>
public interface IRecreateFundRequest
{
    /// <inheritdoc cref="Fund.Id"/>
    Guid Id { get; }

    /// <inheritdoc cref="Fund.Name"/>
    string Name { get; }
}