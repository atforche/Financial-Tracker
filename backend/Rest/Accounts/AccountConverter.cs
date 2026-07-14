using System.Diagnostics.CodeAnalysis;
using Data.Accounts;
using Domain.Accounts;
using Models.Accounts;

namespace Rest.Accounts;

/// <summary>
/// Converter class that handles converting Accounts to Account Models
/// </summary>
public sealed class AccountConverter(AccountRepository accountRepository)
{
    /// <summary>
    /// Converts the provided Account to an Account Model
    /// </summary>
    public AccountModel ToModel(Account account) => new()
    {
        Id = account.Id.Value,
        Name = account.Name,
        Type = AccountTypeConverter.ToModel(account.Type),
    };

    /// <summary>
    /// Attempts to convert the provided ID to an Account
    /// </summary>
    public bool TryToDomain(Guid accountId, [NotNullWhen(true)] out Account? account) => accountRepository.TryGetById(accountId, out account);
}