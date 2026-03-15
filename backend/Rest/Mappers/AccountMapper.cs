using System.Diagnostics.CodeAnalysis;
using Data.Accounts;
using Domain.Accounts;
using Models.Accounts;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Accounts to Account Models
/// </summary>
public sealed class AccountMapper(AccountBalanceService accountBalanceService, AccountBalanceMapper accountBalanceMapper, AccountRepository accountRepository)
{
    /// <summary>
    /// Maps the provided Account to an Account Model
    /// </summary>
    public AccountModel ToModel(Account account) =>
        new()
        {
            Id = account.Id.Value,
            Name = account.Name,
            Type = AccountTypeMapper.ToModel(account.Type),
            CurrentBalance = accountBalanceMapper.ToModel(accountBalanceService.GetCurrentBalance(account))
        };

    /// <summary>
    /// Attempts to map the provided ID to an Account
    /// </summary>
    public bool TryToDomain(Guid accountId, [NotNullWhen(true)] out Account? account) => accountRepository.TryGetById(accountId, out account);
}