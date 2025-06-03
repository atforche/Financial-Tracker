using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Funds;

namespace Domain.Accounts;

/// <summary>
/// Factory for building an <see cref="Account"/>
/// </summary>
public class AccountFactory(IAccountRepository accountRepository, AccountAddedBalanceEventFactory accountAddedBalanceEventFactory)
{
    /// <summary>
    /// Create a new Account
    /// </summary>
    /// <param name="name">Name for the Account</param>
    /// <param name="type">Type for the Account</param>
    /// <param name="accountingPeriodId">Accounting Period ID for the Account Added Balance Event for the Account</param>
    /// <param name="eventDate">Event Date for the Account Added Balance Event for the Account</param>
    /// <param name="fundAmounts">Fund Amounts for the Account Added Balance Event for the Account</param>
    /// <returns>The newly created Account</returns>
    public Account Create(
        string name,
        AccountType type,
        AccountingPeriodId accountingPeriodId,
        DateOnly eventDate,
        IEnumerable<FundAmount> fundAmounts)
    {
        if (!ValidateAccountName(name, out Exception? exception))
        {
            throw exception;
        }
        var account = new Account(name, type);
        account.AccountAddedBalanceEvent = accountAddedBalanceEventFactory.Create(new CreateAccountAddedBalanceEventRequest
        {
            AccountingPeriodId = accountingPeriodId,
            EventDate = eventDate,
            AccountId = account.Id,
            Account = account,
            FundAmounts = fundAmounts.ToList()
        });
        return account;
    }

    /// <summary>
    /// Validates the name for this Account
    /// </summary>
    /// <param name="name">Name for the Account</param>
    /// <param name="exception">Exception encountered during validation</param>
    /// <returns>True if name is valid for this Account, false otherwise</returns>
    private bool ValidateAccountName(string name, [NotNullWhen(false)] out Exception? exception)
    {
        exception = null;

        if (string.IsNullOrEmpty(name))
        {
            exception = new InvalidOperationException();
        }
        if (accountRepository.FindByNameOrNull(name) != null)
        {
            exception ??= new InvalidOperationException();
        }
        return exception == null;
    }
}