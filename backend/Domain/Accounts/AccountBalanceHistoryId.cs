namespace Domain.Accounts;

/// <summary>
/// Value object class representing the ID of an <see cref="AccountBalanceHistory"/>
/// </summary>
public record AccountBalanceHistoryId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// This constructor should only even be used when creating a new Account Balance History ID during Account Balance History creation. 
    /// </summary>
    /// <param name="value">Value for this Account ID</param>
    internal AccountBalanceHistoryId(Guid value)
        : base(value)
    {
    }
}