using Domain.AccountingPeriods;
using Domain.Accounts;
using Tests.Mocks;

namespace Tests.Builders;

/// <summary>
/// Builder class that constructs an Account
/// </summary>
public sealed class AccountBuilder(
    AccountService accountService,
    IAccountingPeriodRepository accountingPeriodRepository,
    TestUnitOfWork testUnitOfWork)
{
    private string? _name;
    private AccountType _type = AccountType.Standard;
    private AccountingPeriod? _accountingPeriod;
    private DateOnly _dateOpened = new(2025, 1, 1);

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
                OpeningAccountingPeriod = DetermineAccountingPeriod(),
                DateOpened = _dateOpened,
            },
            out Account? account,
            out IEnumerable<Exception> exceptions))
        {
            throw new InvalidOperationException("Failed to create Account.", exceptions.First());
        }
        testUnitOfWork.SaveChanges();
        return account ?? throw new InvalidOperationException("Account creation returned null.");
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
    /// Sets the opened date for this Account Builder
    /// </summary>
    public AccountBuilder WithDateOpened(DateOnly dateOpened)
    {
        _dateOpened = dateOpened;
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
        return accountingPeriodRepository.GetByYearAndMonth(_dateOpened.Year, _dateOpened.Month) ?? throw new InvalidOperationException();
    }
}