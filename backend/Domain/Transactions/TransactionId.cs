namespace Domain.Transactions;

/// <summary>
/// Value object class representing the ID of a <see cref="Transaction"/>
/// </summary>
public record TransactionId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// This constructor should only even be used when creating a new Transaction ID during Transaction creation. 
    /// </summary>
    /// <param name="value">Value for this Transaction ID</param>
    internal TransactionId(Guid value)
        : base(value)
    {
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private TransactionId()
        : base()
    {
    }
}