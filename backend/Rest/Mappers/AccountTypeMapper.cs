using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;
using Models.Accounts;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Account Types to Account Type Models
/// </summary>
internal sealed class AccountTypeMapper
{
    /// <summary>
    /// Attempts to map the provided Account Type to an Account Type Model
    /// </summary>
    public static bool TryToModel(
        AccountType accountType,
        [NotNullWhen(true)] out AccountTypeModel? accountTypeModel)
    {
        accountTypeModel = accountType switch
        {
            AccountType.Standard => AccountTypeModel.Standard,
            AccountType.Debt => AccountTypeModel.Debt,
            _ => null
        };
        return accountTypeModel != null;
    }

    /// <summary>
    /// Attempts to map the provided Account Type Model to an Account Type
    /// </summary>
    public static bool TryToDomain(AccountTypeModel accountTypeModel, [NotNullWhen(true)] out AccountType? accountType)
    {
        accountType = accountTypeModel switch
        {
            AccountTypeModel.Standard => AccountType.Standard,
            AccountTypeModel.Debt => AccountType.Debt,
            _ => null
        };
        return accountType != null;
    }
}