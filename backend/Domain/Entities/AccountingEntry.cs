using Domain.Factories;

namespace Domain.Entities;

/// <summary>
/// Entity class representing an Accounting Entry
/// </summary>
public class AccountingEntry : Entity
{
    /// <summary>
    /// Id for this AccountingEntry
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Type for this AccountingEntry
    /// </summary>
    public AccountingEntryType Type { get; }

    /// <summary>
    /// Amount for this AccountingEntry
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Validates the current AccountingEntry
    /// </summary>
    public void Validate()
    {
        if (Amount <= 0)
        {
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Factory responsible for constructing instances of an AccountingEntry
    /// </summary>
    public class AccountingEntryFactory : IAccountingEntryFactory
    {
        /// <inheritdoc/>
        public AccountingEntry Create(CreateAccountingEntryRequest request)
        {
            var accountingEntry = new AccountingEntry(Guid.NewGuid(), request.Type, request.Amount);
            accountingEntry.Validate();
            return accountingEntry;
        }

        /// <inheritdoc/>
        public AccountingEntry Recreate(IRecreateAccountingEntryRequest request) =>
            new AccountingEntry(request.Id, request.Type, request.Amount);
    }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    /// <param name="id">Id for this AccountingEntry</param>
    /// <param name="type">Type for this AccountingEntry</param>
    /// <param name="amount">Amount for this AccountingEntry</param>
    private AccountingEntry(Guid id, AccountingEntryType type, decimal amount)
    {
        Id = id;
        Type = type;
        Amount = amount;
    }
}

/// <summary>
/// Enum representing the different Accounting Entry types
/// </summary>
public enum AccountingEntryType
{
    /// <summary>
    /// Accounting Entry that debits money from an Account
    /// </summary>
    Debit,

    /// <summary>
    /// Accounting Entry that credits money to an Account
    /// </summary>
    Credit,
}