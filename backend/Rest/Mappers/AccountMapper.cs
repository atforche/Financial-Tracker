using Domain.Accounts;
using Models.Accounts;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Accounts to Account Models
/// </summary>
internal sealed class AccountMapper
{
    /// <summary>
    /// Converts the provided Account into an Account Model
    /// </summary>
    /// <param name="account">Account to convert into a model</param>
    /// <returns>The Account Model corresponding to the provided Account</returns>
    public static AccountModel ToModel(Account account) => new()
    {
        Id = account.Id.Value,
        Name = account.Name,
        Type = AccountTypeMapper.ToModel(account.Type)
    };
}