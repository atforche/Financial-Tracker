using System.Collections;
using Domain.Aggregates.Accounts;

namespace Tests.AddTransaction.Scenarios;

/// <summary>
/// Collection class that contains all the unique Account scenarios for adding a Transaction
/// </summary>
public sealed class AccountScenarios :
    IEnumerable<TheoryDataRow<AccountType?, AccountType?, SameAccountTypeBehavior>>
{
    /// <inheritdoc/>
    public IEnumerator<TheoryDataRow<AccountType?, AccountType?, SameAccountTypeBehavior>> GetEnumerator()
    {
        var accountTypes = new Tests.Scenarios.AccountScenarios().Select(row => (AccountType?)row.Data).ToList();
        accountTypes.Add(null);
        foreach (AccountType? debitAccountType in accountTypes)
        {
            foreach (AccountType? creditAccountType in accountTypes)
            {
                yield return new TheoryDataRow<AccountType?, AccountType?, SameAccountTypeBehavior>(
                    debitAccountType,
                    creditAccountType,
                    SameAccountTypeBehavior.UseDifferentAccounts);
                if (debitAccountType != null && creditAccountType != null && debitAccountType == creditAccountType)
                {
                    yield return new TheoryDataRow<AccountType?, AccountType?, SameAccountTypeBehavior>(
                        debitAccountType,
                        creditAccountType,
                        SameAccountTypeBehavior.UseSameAccount);
                }
            }
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Determines if the provided scenario is valid
    /// </summary>
    /// <param name="debitAccountType">Debit Account Type for this scenario</param>
    /// <param name="creditAccountType">Credit Account Type for this scenario</param>
    /// <param name="sameAccountTypeBehavior">Same Account Type Behavior for this scenario</param>
    /// <returns>True if this scenario is valid, false otherwise</returns>
    public static bool IsValid(AccountType? debitAccountType, AccountType? creditAccountType, SameAccountTypeBehavior sameAccountTypeBehavior)
    {
        if (debitAccountType == null && creditAccountType == null)
        {
            return false;
        }
        if (debitAccountType == creditAccountType && sameAccountTypeBehavior == SameAccountTypeBehavior.UseSameAccount)
        {
            return false;
        }
        return true;
    }
}

/// <summary>
/// Enum representing the different behaviors when the same Account type is used for the debit and credit Accounts
/// </summary>
public enum SameAccountTypeBehavior
{
    /// <summary>
    /// Use the same Account as the debit and credit Accounts
    /// </summary>
    UseSameAccount,

    /// <summary>
    /// Use two different Accounts of the same type for the debit and credit Accounts
    /// </summary>
    UseDifferentAccounts,
}