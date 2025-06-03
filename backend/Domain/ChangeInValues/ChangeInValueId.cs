namespace Domain.ChangeInValues;

/// <summary>
/// Value object class representing the ID of an <see cref="ChangeInValue"/>
/// </summary>
public record ChangeInValueId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// This constructor should only even be used when creating a new Change In Value ID during Change In Value creation. 
    /// </summary>
    /// <param name="value">Value for this Change In Value ID</param>
    internal ChangeInValueId(Guid value)
        : base(value)
    {
    }
}

/// <summary>
/// Factory for constructing a Change In Value ID
/// </summary>
/// <param name="changeInValueRepository">Change In Value Repository</param>
public class ChangeInValueIdFactory(IChangeInValueRepository changeInValueRepository)
{
    /// <summary>
    /// Creates a new Change In Value ID with the given value
    /// </summary>
    /// <param name="value">Value for this Change In Value ID</param>
    /// <returns>The newly created Change In Value ID</returns>
    public ChangeInValueId Create(Guid value)
    {
        if (!changeInValueRepository.DoesChangeInValueWithIdExist(value))
        {
            throw new InvalidOperationException();
        }
        return new ChangeInValueId(value);
    }
}