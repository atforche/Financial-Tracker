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
    public bool TryToModel(Account account, [NotNullWhen(true)] out AccountModel? accountModel)
    {
        accountModel = null;
        if (!AccountTypeMapper.TryToModel(account.Type, out AccountTypeModel? accountTypeModel))
        {
            return false;
        }
        accountModel = new AccountModel
        {
            Id = account.Id.Value,
            Name = account.Name,
            Type = accountTypeModel.Value,
            CurrentBalance = accountBalanceMapper.ToModel(accountBalanceService.GetCurrentBalance(account))
        };
        return true;
    }

    /// <summary>
    /// Attempts to map the provided ID to an Account
    /// </summary>
    public bool TryToDomain(Guid accountId, [NotNullWhen(true)] out Account? account) => accountRepository.TryGetById(accountId, out account);
}