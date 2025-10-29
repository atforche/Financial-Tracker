namespace Domain.Funds;

/// <summary>
/// Value object class representing the ID of an <see cref="Fund"/>
/// </summary>
public record FundId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// </summary>
    /// <param name="value">Value for this Fund ID</param>
    internal FundId(Guid value)
        : base(value)
    {
    }
}