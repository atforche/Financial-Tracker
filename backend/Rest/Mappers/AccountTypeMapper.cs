using Domain.Accounts;
using Models.Accounts;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Account Types to Account Type Models
/// </summary>
internal sealed class AccountTypeMapper
{
    /// <summary>
    /// Maps the provided Account Type to an Account Type Model
    /// </summary>
    public static AccountTypeModel ToModel(AccountType accountType) => accountType switch
    {
        AccountType.Standard => AccountTypeModel.Standard,
        AccountType.Debt => AccountTypeModel.Debt,
        _ => throw new InvalidOperationException($"Unrecognized Account Type: {accountType}")
    };

    /// <summary>
    /// Maps the provided Account Type Model to an Account Type
    /// </summary>
    public static AccountType ToDomain(AccountTypeModel accountType) => accountType switch
    {
        AccountTypeModel.Standard => AccountType.Standard,
        AccountTypeModel.Debt => AccountType.Debt,
        _ => throw new InvalidOperationException($"Unrecognized Account Type: {accountType}")
    };
}