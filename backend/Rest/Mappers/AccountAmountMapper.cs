using Domain.Accounts;
using Models.Accounts;

namespace Rest.Mappers;

/// <summary>
/// Mapper class that handles mapping Account Amounts to Account Amount Models
/// </summary>
public sealed class AccountAmountMapper(IAccountRepository accountRepository)
{
    /// <summary>
    /// Maps the provided Account Amount to an Account Amount Model
    /// </summary>
    public AccountAmountModel ToModel(AccountAmount accountAmount) => new()
    {
        AccountId = accountAmount.AccountId.Value,
        AccountName = accountRepository.FindById(accountAmount.AccountId).Name,
        Amount = accountAmount.Amount
    };
}