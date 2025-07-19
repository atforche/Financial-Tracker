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
    IFundRepository fundRepository,
    TestUnitOfWork testUnitOfWork)
{
    private string? _name;
    private AccountType _type = AccountType.Standard;
    private AccountingPeriodId? _accountingPeriodId;
    private DateOnly _addedDate = new(2025, 1, 1);
    private List<FundAmount>? _addedFundAmounts;

    /// <summary>
    /// Builds the specified Account
    /// </summary>
    /// <returns>The newly constructed Account</returns>
    public Account Build()
    {
        Account account = accountFactory.Create(
            _name ?? Guid.NewGuid().ToString(),
            _type,
            DetermineAccountingPeriod(),
            _addedDate,
            DetermineFundAmounts());
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

    /// <summary>
    /// Determines the Accounting Period to use for this Account
    /// </summary>
    private AccountingPeriodId DetermineAccountingPeriod()
    {
        if (_accountingPeriodId != null)
        {
            return _accountingPeriodId;
        }
        return accountingPeriodRepository.FindAll()
            .Single(accountingPeriod => accountingPeriod.Year == _addedDate.Year && accountingPeriod.Month == _addedDate.Month).Id;
    }

    /// <summary>
    /// Determines the Added Fund Amounts to use for this Account
    /// </summary>
    private List<FundAmount> DetermineFundAmounts()
    {
        if (_addedFundAmounts != null)
        {
            return _addedFundAmounts;
        }
        var funds = fundRepository.FindAll().ToList();
        if (funds.Count == 1)
        {
            return
            [
                new FundAmount
                {
                    FundId = funds.First().Id,
                    Amount = 2500.00m
                }
            ];
        }
        return [];
    }
}