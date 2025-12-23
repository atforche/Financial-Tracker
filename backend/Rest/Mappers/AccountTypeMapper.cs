using Domain.Accounts;
using Models.Accounts;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Account Types to Account Type Models
/// </summary>
internal sealed class AccountTypeMapper
{
    /// <summary>
    /// Converts the provided Account Type model into an Account Type
    /// </summary>
    /// <param name="accountType">Account Type model to convert</param>
    /// <returns>The Account Type corresponding to the provided Account Type model</returns>
    public static AccountType ToDomain(AccountTypeModel accountType) => accountType switch
    {
        AccountTypeModel.Standard => AccountType.Standard,
        AccountTypeModel.Debt => AccountType.Debt,
        _ => throw new InvalidOperationException($"Unrecognized Account Type: {accountType}")
    };

    /// <summary>
    /// Converts the provided Account Type into an Account Type Model
    /// </summary>
    /// <param name="accountType">Account Type to convert</param>
    /// <returns>The Account Type Model corresponding to the provided Account Type</returns>
    public static AccountTypeModel ToModel(AccountType accountType) => accountType switch
    {
        AccountType.Standard => AccountTypeModel.Standard,
        AccountType.Debt => AccountTypeModel.Debt,
        _ => throw new InvalidOperationException($"Unrecognized Account Type: {accountType}")
    };
}