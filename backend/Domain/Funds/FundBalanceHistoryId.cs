namespace Domain.Funds;

/// <summary>
/// Value object class representing the ID of an <see cref="FundBalanceHistory"/>
/// </summary>
public record FundBalanceHistoryId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// This constructor should only even be used when creating a new Fund Balance History ID during Fund Balance History creation. 
    /// </summary>
    /// <param name="value">Value for this Fund ID</param>
    internal FundBalanceHistoryId(Guid value)
        : base(value)
    {
    }
}