using System.Diagnostics.CodeAnalysis;
using Domain.Accounts;
using Microsoft.AspNetCore.Mvc;
using Models.Accounts;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Accounts to Account Models
/// </summary>
public sealed class AccountMapper(AccountBalanceService accountBalanceService, IAccountRepository accountRepository)
{
    /// <summary>
    /// Maps the provided Account to an Account Model
    /// </summary>
    public AccountModel ToModel(Account account)
    {
        AccountBalance currentBalance = accountBalanceService.GetCurrentBalance(account.Id);
        return new AccountModel
        {
            Id = account.Id.Value,
            Name = account.Name,
            Type = AccountTypeMapper.ToModel(account.Type),
            CurrentBalance = currentBalance.Balance,
            PendingDebitAmount = currentBalance.PendingDebits.Sum(fundAmount => fundAmount.Amount),
            PendingCreditAmount = currentBalance.PendingCredits.Sum(fundAmount => fundAmount.Amount)
        };
    }

    /// <summary>
    /// Attempts to map the provided ID to an Account
    /// </summary>
    public bool TryToDomain(
        Guid accountId,
        [NotNullWhen(true)] out Account? account,
        [NotNullWhen(false)] out IActionResult? errorResult)
    {
        errorResult = null;
        if (!accountRepository.TryFindById(accountId, out account))
        {
            errorResult = new NotFoundObjectResult(ErrorMapper.ToModel($"Account with ID {accountId} was not found.", []));
            return false;
        }
        return true;
    }
}