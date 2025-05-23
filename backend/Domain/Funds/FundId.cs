namespace Domain.Funds;

/// <summary>
/// Value object class representing the ID of an <see cref="Fund"/>
/// </summary>
public record FundId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// This constructor should only even be used when creating a new Fund ID during Fund creation. 
    /// </summary>
    /// <param name="value">Value for this Fund ID</param>
    internal FundId(Guid value)
        : base(value)
    {
    }
}

/// <summary>
/// Factory for constructing an Fund ID
/// </summary>
/// <param name="fundRepository">Fund Repository</param>
public class FundIdFactory(IFundRepository fundRepository)
{
    /// <summary>
    /// Creates a new Fund ID with the given value
    /// </summary>
    /// <param name="value">Value for this Fund ID</param>
    /// <returns>The newly created Fund ID</returns>
    public FundId Create(Guid value)
    {
        if (!fundRepository.DoesFundWithIdExist(value))
        {
            throw new InvalidOperationException();
        }
        return new FundId(value);
    }
}