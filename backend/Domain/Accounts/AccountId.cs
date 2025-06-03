namespace Domain.Accounts;

/// <summary>
/// Value object class representing the ID of an <see cref="Account"/>
/// </summary>
public record AccountId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// This constructor should only even be used when creating a new Account ID during Account creation. 
    /// </summary>
    /// <param name="value">Value for this Account ID</param>
    internal AccountId(Guid value)
        : base(value)
    {
    }
}

/// <summary>
/// Factory for constructing an Account ID
/// </summary>
/// <param name="accountRepository">Account Repository</param>
public class AccountIdFactory(IAccountRepository accountRepository)
{
    /// <summary>
    /// Creates a new Account ID with the given value
    /// </summary>
    /// <param name="value">Value for this Account ID</param>
    /// <returns>The newly created Account ID</returns>
    public AccountId Create(Guid value)
    {
        if (!accountRepository.DoesAccountWithIdExist(value))
        {
            throw new InvalidOperationException();
        }
        return new AccountId(value);
    }
}