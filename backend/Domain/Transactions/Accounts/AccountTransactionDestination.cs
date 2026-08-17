using Domain.Accounts;
using Domain.Locations;

namespace Domain.Transactions.Accounts;

/// <summary>
/// Value object representing the destination of money for an Account Transaction
/// </summary>
public class AccountTransactionDestination
{
    /// <summary>
    /// Account of the account transaction destination
    /// </summary>
    public Account? Account { get; private set; }

    /// <summary>
    /// Posted date of the account transaction destination
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Location of the account transaction destination
    /// </summary>
    public Location? Location { get; private set; }

    /// <summary>
    /// Amount of the account transaction destination
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public AccountTransactionDestination(Account? account, DateOnly? postedDate, Location? location, decimal amount)
    {
        Account = account;
        PostedDate = postedDate;
        Location = location;
        Amount = amount;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private AccountTransactionDestination() { }
}