using Domain.Events;
using Domain.Factories;

namespace Domain.Entities;

/// <summary>
/// Entity class representing an Account
/// </summary>
public class Account : Entity
{
    /// <summary>
    /// Id for this Account
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Name for this Account
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Type for this Account
    /// </summary>
    public AccountType Type { get; }

    /// <summary>
    /// Is active flag for this Account
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Updates the name on this Account
    /// </summary>
    /// <param name="newName">New name for this Account</param>
    public void SetName(string newName)
    {
        Name = newName;
        RaiseEvent(new AccountRenamedDomainEvent { NewName = newName });
        Validate();
    }

    /// <summary>
    /// Verifies that the current Account is valid
    /// </summary>
    public void Validate()
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

    /// <summary>
    /// Factory responsible for constructing instances of an Account
    /// </summary>
    public class AccountFactory : IAccountFactory
    {
        /// <inheritdoc/>
        public Account Create(string name, AccountType type, bool isActive)
        {
            var account = new Account(Guid.NewGuid(), name, type, isActive);
            account.Validate();
            account.RaiseEvent(new AccountRenamedDomainEvent { NewName = name });
            return account;
        }

        /// <inheritdoc/>
        public Account Recreate(IRecreateAccountRequest request) =>
            new Account(request.Id, request.Name, request.Type, request.IsActive);
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="id">Id for this Account</param>
    /// <param name="name">Name for this Account</param>
    /// <param name="type">Type for this Account</param>
    /// <param name="isActive">Is active flag for this Account</param>
    private Account(Guid id, string name, AccountType type, bool isActive)
    {
        Id = id;
        Name = name;
        Type = type;
        IsActive = isActive;
    }
}

/// <summary>
/// Enum representing the different account types.
/// </summary>
public enum AccountType
{
    /// <summary>
    /// A Standard account represents a standard checking or savings account.
    /// </summary>
    Standard,

    /// <summary>
    /// A Debt account represents a credit card or loan account.
    /// </summary>
    Debt,

    /// <summary>
    /// An Investment account represents a retirement or brokerage account.
    /// </summary>
    Investment,
}