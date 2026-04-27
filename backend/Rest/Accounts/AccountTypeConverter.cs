using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;
using Models.Accounts;

namespace Rest.Accounts;

/// <summary>
/// Converter class that handles converting Account Types to Account Type Models
/// </summary>
internal sealed class AccountTypeConverter
{
    /// <summary>
    /// Converts the provided Account Type to an Account Type Model
    /// </summary>
    public static AccountTypeModel ToModel(AccountType accountType) => accountType switch
    {
        AccountType.Standard => AccountTypeModel.Standard,
        AccountType.CreditCard => AccountTypeModel.CreditCard,
        AccountType.Investment => AccountTypeModel.Investment,
        AccountType.Debt => AccountTypeModel.Debt,
        AccountType.Retirement => AccountTypeModel.Retirement,
        AccountType.Escrow => AccountTypeModel.Escrow,
        _ => throw new InvalidOperationException($"Unrecognized Account Type: {accountType}")
    };

    /// <summary>
    /// Attempts to convert the provided Account Type Model to an Account Type
    /// </summary>
    public static bool TryToDomain(AccountTypeModel accountTypeModel, [NotNullWhen(true)] out AccountType? accountType)
    {
        accountType = accountTypeModel switch
        {
            AccountTypeModel.Standard => AccountType.Standard,
            AccountTypeModel.CreditCard => AccountType.CreditCard,
            AccountTypeModel.Investment => AccountType.Investment,
            AccountTypeModel.Debt => AccountType.Debt,
            AccountTypeModel.Retirement => AccountType.Retirement,
            AccountTypeModel.Escrow => AccountType.Escrow,
            _ => null
        };
        return accountType != null;
    }
}