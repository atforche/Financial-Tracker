using System.Diagnostics.CodeAnalysis;
using Domain.AccountGoals;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Validation;

namespace Tests.AccountGoals;

/// <summary>
/// Covers Account Goal invariant validation.
/// </summary>
public sealed class AccountGoalServiceTests
{
    /// <summary>
    /// Rejects non-standard Accounts and invalid ending-balance bounds.
    /// </summary>
    [Fact]
    public void TryCreateRejectsInvalidAccountAndBounds()
    {
        AccountingPeriod period = new(2026, 7);
        Account account = new("Card", null, AccountType.CreditCard, period.Id, new DateOnly(2026, 7, 1));
        AccountGoalService service = CreateService(period);

        bool created = service.TryCreate(
            new CreateAccountGoalRequest
            {
                Account = account,
                AccountingPeriod = period,
                MinimumEndingBalance = 200m,
                MaximumEndingBalance = 100m,
            },
            out _,
            out IEnumerable<ValidationError> errors);

        Assert.False(created);
        Assert.Contains(errors, error => error.Path.Value == nameof(CreateAccountGoalRequest.Account));
        Assert.Contains(errors, error => error.Message.Contains("less than or equal", StringComparison.Ordinal));
    }

    /// <summary>
    /// Rejects a period before the Account's opening period.
    /// </summary>
    [Fact]
    public void TryCreateRejectsPeriodBeforeAccountOpeningPeriod()
    {
        AccountingPeriod july = new(2026, 7);
        AccountingPeriod august = new(2026, 8);
        Account account = new("Checking", null, AccountType.Standard, august.Id, new DateOnly(2026, 8, 1));
        AccountGoalService service = CreateService(july, august);

        bool created = service.TryCreate(
            new CreateAccountGoalRequest { Account = account, AccountingPeriod = july },
            out _,
            out IEnumerable<ValidationError> errors);

        Assert.False(created);
        Assert.Contains(errors, error => error.Message.Contains("predate", StringComparison.Ordinal));
    }

    /// <summary>
    /// Rejects changes to an Account Goal in a closed period.
    /// </summary>
    [Fact]
    public void TryUpdateRejectsClosedPeriodAndInvalidBounds()
    {
        AccountingPeriod period = new(2026, 7);
        Account account = new("Checking", null, AccountType.Standard, period.Id, new DateOnly(2026, 7, 1));
        AccountGoal goal = new(account, period, 100m, 200m);
        period.IsOpen = false;

        bool updated = AccountGoalService.TryUpdate(
            goal,
            new UpdateAccountGoalRequest
            {
                MinimumEndingBalance = -1m,
                MaximumEndingBalance = 0m,
            },
            out IEnumerable<ValidationError> errors);

        Assert.False(updated);
        Assert.Equal(100m, goal.MinimumEndingBalance);
        Assert.Contains(errors, error => error.Message.Contains("greater than or equal", StringComparison.Ordinal));
        Assert.Contains(errors, error => error.Message.Contains("closed", StringComparison.Ordinal));
    }

    /// <summary>
    /// Calculates Account Goal progress from the period's closing Account balance.
    /// </summary>
    [Fact]
    public void TryGetProgressUsesClosingBalance()
    {
        AccountingPeriod period = new(2026, 7);
        Account account = new("Checking", null, AccountType.Standard, period.Id, new DateOnly(2026, 7, 1));
        AccountGoal goal = new(account, period, 100m, 200m);
        AccountingPeriodBalanceHistory history = new(
            period,
            [new AccountingPeriodAccountBalanceHistory(account, period, 80m, 150m)],
            [],
            []);
        AccountGoalService service = new(
            new InMemoryAccountGoalRepository(),
            new InMemoryAccountingPeriodRepository([period]),
            new InMemoryAccountRepository(),
            new InMemoryBalanceHistoryRepository([history]));

        bool calculated = service.TryGetProgress(goal, period, out AccountGoalProgress? progress, out _);

        Assert.True(calculated);
        Assert.Equal(150m, progress!.PositiveBalance.CurrentBalance);
        Assert.True(progress.IsSatisfied);
    }

    private static AccountGoalService CreateService(params AccountingPeriod[] periods) =>
        new(
            new InMemoryAccountGoalRepository(),
            new InMemoryAccountingPeriodRepository(periods),
            new InMemoryAccountRepository(),
            new InMemoryBalanceHistoryRepository([]));

    private sealed class InMemoryAccountGoalRepository : IAccountGoalRepository
    {
        private readonly List<AccountGoal> _accountGoals = [];

        public AccountGoal GetById(AccountGoalId id) => _accountGoals.Single(accountGoal => accountGoal.Id == id);

        public bool TryGetById(Guid id, [NotNullWhen(true)] out AccountGoal? accountGoal)
        {
            accountGoal = _accountGoals.SingleOrDefault(candidate => candidate.Id.Value == id);
            return accountGoal != null;
        }

        public IReadOnlyCollection<AccountGoal> GetAllByAccount(AccountId accountId) =>
            _accountGoals.Where(accountGoal => accountGoal.Account.Id == accountId).ToList();

        public IReadOnlyCollection<AccountGoal> GetAllByAccountingPeriod(AccountingPeriodId? accountingPeriodId) =>
            _accountGoals.Where(accountGoal => accountGoal.AccountingPeriod?.Id == accountingPeriodId).ToList();

        public AccountGoal? GetByAccountAndAccountingPeriod(AccountId accountId, AccountingPeriodId? accountingPeriodId) =>
            _accountGoals.SingleOrDefault(accountGoal => accountGoal.Account.Id == accountId
                && accountGoal.AccountingPeriod?.Id == accountingPeriodId);

        public bool TryAdd(AccountGoal accountGoal)
        {
            if (GetByAccountAndAccountingPeriod(accountGoal.Account.Id, accountGoal.AccountingPeriod?.Id) != null)
            {
                return false;
            }
            _accountGoals.Add(accountGoal);
            return true;
        }

        public void Delete(AccountGoal accountGoal) => _accountGoals.Remove(accountGoal);
    }

    private sealed class InMemoryAccountingPeriodRepository(IReadOnlyCollection<AccountingPeriod> periods) : IAccountingPeriodRepository
    {
        public IReadOnlyCollection<AccountingPeriod> GetAll() => periods;

        public IReadOnlyCollection<AccountingPeriod> GetAllOpenPeriods() => periods.Where(period => period.IsOpen).ToList();

        public AccountingPeriod GetById(AccountingPeriodId id) => periods.Single(period => period.Id == id);

        public bool TryGetById(Guid id, [NotNullWhen(true)] out AccountingPeriod? accountingPeriod)
        {
            accountingPeriod = periods.SingleOrDefault(period => period.Id.Value == id);
            return accountingPeriod != null;
        }

        public AccountingPeriod? GetByYearAndMonth(int year, int month) =>
            periods.SingleOrDefault(period => period.Year == year && period.Month == month);

        public AccountingPeriod? GetLatestAccountingPeriod() => periods.OrderBy(period => period.PeriodStartDate).LastOrDefault();

        public AccountingPeriod? GetNextAccountingPeriod(AccountingPeriodId id) =>
            periods.SingleOrDefault(period => period.PeriodStartDate == GetById(id).PeriodStartDate.AddMonths(1));

        public AccountingPeriod? GetPreviousAccountingPeriod(AccountingPeriodId id) =>
            periods.SingleOrDefault(period => period.PeriodStartDate == GetById(id).PeriodStartDate.AddMonths(-1));

        public void Add(AccountingPeriod accountingPeriod) => throw new NotSupportedException();

        public void Delete(AccountingPeriod accountingPeriod) => throw new NotSupportedException();
    }

    private sealed class InMemoryAccountRepository : IAccountRepository
    {
        public IReadOnlyCollection<Account> GetAll() => [];

        public IReadOnlyCollection<Account> GetAllAccountsAddedInPeriod(AccountingPeriodId accountingPeriodId) => [];

        public Account GetById(AccountId id) => throw new NotSupportedException();

        public bool TryGetById(Guid id, [NotNullWhen(true)] out Account? account)
        {
            account = null;
            return false;
        }

        public bool TryGetByName(string name, [NotNullWhen(true)] out Account? account)
        {
            account = null;
            return false;
        }

        public void Add(Account account) => throw new NotSupportedException();

        public void Delete(Account account) => throw new NotSupportedException();
    }

    private sealed class InMemoryBalanceHistoryRepository(IReadOnlyCollection<AccountingPeriodBalanceHistory> histories) : IAccountingPeriodBalanceHistoryRepository
    {
        public AccountingPeriodBalanceHistory GetForAccountingPeriod(AccountingPeriodId accountingPeriodId) =>
            histories.Single(history => history.AccountingPeriod.Id == accountingPeriodId);

        public void Add(AccountingPeriodBalanceHistory accountingPeriodBalanceHistory) => throw new NotSupportedException();

        public void Delete(AccountingPeriodBalanceHistory accountingPeriodBalanceHistory) => throw new NotSupportedException();
    }
}
