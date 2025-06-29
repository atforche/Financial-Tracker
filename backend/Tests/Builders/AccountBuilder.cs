using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Tests.Mocks;

namespace Tests.Builders;

/// <summary>
/// Builder class that constructs an Account
/// </summary>
public sealed class AccountBuilder(
    AccountFactory accountFactory,
    IAccountRepository accountRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    TestUnitOfWork testUnitOfWork)
{
    private string _name = Guid.NewGuid().ToString();
    private AccountType _type = AccountType.Standard;
    private AccountingPeriodId? _accountingPeriodId;
    private DateOnly _addedDate = new(2025, 1, 1);
    private List<FundAmount> _addedFundAmounts = [];

    /// <summary>
    /// Builds the specified Account
    /// </summary>
    /// <returns>The newly constructed Account</returns>
    public Account Build()
    {
        AccountingPeriodId accountingPeriodId = _accountingPeriodId ?? accountingPeriodRepository.FindAll()
            .Single(accountingPeriod => accountingPeriod.Year == _addedDate.Year && accountingPeriod.Month == _addedDate.Month).Id;
        Account account = accountFactory.Create(_name, _type, accountingPeriodId, _addedDate, _addedFundAmounts);
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
    public AccountBuilder WithAccountingPeriodId(AccountingPeriodId accountingPeriodId)
    {
        _accountingPeriodId = accountingPeriodId;
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
    /// Sets the Added Fund Amounts for this Account Builder
    /// </summary>
    public AccountBuilder WithAddedFundAmounts(IEnumerable<FundAmount> addedFundAmounts)
    {
        _addedFundAmounts = addedFundAmounts.ToList();
        return this;
    }
}