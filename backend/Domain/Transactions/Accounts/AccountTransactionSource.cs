using Domain.Accounts;
using Domain.Locations;

namespace Domain.Transactions.Accounts;

/// <summary>
/// Value object representing the source of money for an Account Transaction
/// </summary>
public class AccountTransactionSource
{
    /// <summary>
    /// Account of the account transaction source
    /// </summary>
    public Account? Account { get; private set; }

    /// <summary>
    /// Posted date of the account transaction source
    /// </summary>
    public DateOnly? PostedDate { get; internal set; }

    /// <summary>
    /// Location of the account transaction source
    /// </summary>
    public Location? Location { get; private set; }

    /// <summary>
    /// Constructs a new instance of this class
    /// </summary>
    public AccountTransactionSource(Account? account, DateOnly? postedDate, Location? location)
    {
        Account = account;
        PostedDate = postedDate;
        Location = location;
    }

    /// <summary>
    /// Constructs a new default instance of this class
    /// </summary>
    private AccountTransactionSource() { }
}
