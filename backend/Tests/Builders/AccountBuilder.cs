using Domain.AccountingPeriods;
using Domain.Accounts;
using Tests.Mocks;

namespace Tests.Builders;

/// <summary>
/// Builder class that constructs an Account
/// </summary>
public sealed class AccountBuilder(
    AccountService accountService,
    IAccountRepository accountRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    TestUnitOfWork testUnitOfWork)
{
    private string? _name;
    private AccountType _type = AccountType.Standard;
    private AccountingPeriod? _accountingPeriod;
    private DateOnly _addedDate = new(2025, 1, 1);
    private decimal _initialBalance;

    /// <summary>
    /// Builds the specified Account
    /// </summary>
    /// <returns>The newly constructed Account</returns>
    public Account Build()
    {
        if (!accountService.TryCreate(
            new CreateAccountRequest
            {
                Name = _name ?? Guid.NewGuid().ToString(),
                Type = _type,
                AccountingPeriod = DetermineAccountingPeriod(),
                AddDate = _addedDate,
                InitialBalance = _initialBalance,
                InitialFundAssignments = [],
            },
            out Account? account,
            out IEnumerable<Exception> exceptions))
        {
            throw new InvalidOperationException("Failed to create Account.", exceptions.First());
        }
        accountRepository.Add(account);
        testUnitOfWork.SaveChanges();
        return account;
    }

    /// <summary>
    /// Sets the Name for this Account Builder
    /// </summary>
    public AccountBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the Type for this Account Builder
    /// </summary>
    public AccountBuilder WithType(AccountType type)
    {
        _type = type;
        return this;
    }

    /// <summary>
    /// Sets the Accounting Period ID for this Account Builder
    /// </summary>
    public AccountBuilder WithAccountingPeriod(AccountingPeriod accountingPeriod)
    {
        _accountingPeriod = accountingPeriod;
        return this;
    }

    /// <summary>
    /// Sets the Added Date for this Account Builder
    /// </summary>
    public AccountBuilder WithAddedDate(DateOnly addedDate)
    {
        _addedDate = addedDate;
        return this;
    }

    /// <summary>
    /// Sets the Initial Balance for this Account Builder
    /// </summary>
    public AccountBuilder WithInitialBalance(decimal initialBalance)
    {
        _initialBalance = initialBalance;
        return this;
    }

    /// <summary>
    /// Determines the Accounting Period to use for this Account
    /// </summary>
    private AccountingPeriod DetermineAccountingPeriod()
    {
        if (_accountingPeriod != null)
        {
            return _accountingPeriod;
        }
        return accountingPeriodRepository.GetByYearAndMonth(_addedDate.Year, _addedDate.Month) ?? throw new InvalidOperationException();
    }
}