namespace Domain.Transactions;

/// <summary>
/// Value object class representing the ID of an <see cref="Transaction"/>
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
}

/// <summary>
/// Factory for constructing a Transaction ID
/// </summary>
/// <param name="transactionRepository">Transaction Repository</param>
public class TransactionIdFactory(ITransactionRepository transactionRepository)
{
    /// <summary>
    /// Creates a new Transaction ID with the given value
    /// </summary>
    /// <param name="value">Value for this Transaction ID</param>
    /// <returns>The newly created Transaction ID</returns>
    public TransactionId Create(Guid value)
    {
        if (!transactionRepository.DoesTransactionWithIdExist(value))
        {
            throw new InvalidOperationException();
        }
        return new TransactionId(value);
    }
}