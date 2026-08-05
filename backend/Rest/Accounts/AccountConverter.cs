using Domain;
using Domain.Accounts;
using Domain.Accounts.Queries;
using Models;
using Models.Accounts;

namespace Rest.Accounts;

/// <summary>
/// Converts between Account API models and Domain query types.
/// </summary>
public sealed class AccountConverter
{
    /// <summary>
    /// Converts the provided Account query model to a Domain query.
    /// </summary>
    public AccountQuery ToDomain(AccountQueryParameterModel model) => new(
        ToDomain(model.Filter),
        model.Sort switch
        {
            AccountSortModel.Name => AccountSort.Name,
            AccountSortModel.NameDescending => AccountSort.NameDescending,
            AccountSortModel.Type => AccountSort.Type,
            AccountSortModel.TypeDescending => AccountSort.TypeDescending,
            _ => AccountSort.Name,
        },
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Account Balance query model to a Domain query.
    /// </summary>
    public AccountBalanceQuery ToDomain(AccountWithBalanceQueryParameterModel model) => new(
        ToDomain(model.Filter),
        model.Sort switch
        {
            AccountWithBalanceSortModel.Name => AccountBalanceSort.Name,
            AccountWithBalanceSortModel.NameDescending => AccountBalanceSort.NameDescending,
            AccountWithBalanceSortModel.Type => AccountBalanceSort.Type,
            AccountWithBalanceSortModel.TypeDescending => AccountBalanceSort.TypeDescending,
            AccountWithBalanceSortModel.PostedBalance => AccountBalanceSort.PostedBalance,
            AccountWithBalanceSortModel.PostedBalanceDescending => AccountBalanceSort.PostedBalanceDescending,
            _ => AccountBalanceSort.Name,
        },
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Account Accounting Period range query to a Domain query.
    /// </summary>
    public AccountAccountingPeriodRangeQuery ToDomain(AccountsInAccountingPeriodRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End,
        ToDomain(model.Filter),
        model.Sort switch
        {
            AccountWithBalanceRangeSortModel.Name => AccountRangeSort.Name,
            AccountWithBalanceRangeSortModel.NameDescending => AccountRangeSort.NameDescending,
            AccountWithBalanceRangeSortModel.Type => AccountRangeSort.Type,
            AccountWithBalanceRangeSortModel.TypeDescending => AccountRangeSort.TypeDescending,
            AccountWithBalanceRangeSortModel.StartingBalance => AccountRangeSort.StartingBalance,
            AccountWithBalanceRangeSortModel.StartingBalanceDescending => AccountRangeSort.StartingBalanceDescending,
            AccountWithBalanceRangeSortModel.EndingBalance => AccountRangeSort.EndingBalance,
            AccountWithBalanceRangeSortModel.EndingBalanceDescending => AccountRangeSort.EndingBalanceDescending,
            AccountWithBalanceRangeSortModel.NetChange => AccountRangeSort.NetChange,
            AccountWithBalanceRangeSortModel.NetChangeDescending => AccountRangeSort.NetChangeDescending,
            _ => AccountRangeSort.Name,
        },
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Account date-range query to a Domain query.
    /// </summary>
    public AccountDateRangeQuery ToDomain(AccountsInDateRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End,
        ToDomain(model.Filter),
        ToDomain(model.Sort),
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts the provided Account to an Account model.
    /// </summary>
    public AccountModel ToModel(Account account) => new()
    {
        Id = account.Id.Value,
        Name = account.Name,
        FinancialInstitution = account.FinancialInstitution,
        Type = AccountTypeConverter.ToModel(account.Type),
    };

    /// <summary>
    /// Converts the provided financial institutions to a collection model.
    /// </summary>
    public CollectionModel<string> ToModel(IReadOnlyCollection<string> financialInstitutions) => new()
    {
        Items = financialInstitutions,
        TotalCount = financialInstitutions.Count,
    };

    /// <summary>
    /// Converts the provided Account page to an Account collection model.
    /// </summary>
    public CollectionModel<AccountModel> ToModel(QueryPage<Account> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts the provided Account Balance to an Account with Balance model.
    /// </summary>
    public AccountWithBalanceModel ToModel(AccountBalance balance) => new()
    {
        Id = balance.Account.Id.Value,
        Name = balance.Account.Name,
        FinancialInstitution = balance.Account.FinancialInstitution,
        Type = AccountTypeConverter.ToModel(balance.Account.Type),
        CurrentBalance = new AccountBalanceModel
        {
            PostedBalance = balance.PostedBalance,
            BalanceIncludingPending = balance.BalanceIncludingPending,
        },
    };

    /// <summary>
    /// Converts the provided Account Balance page to an Account with Balance collection model.
    /// </summary>
    public CollectionModel<AccountWithBalanceModel> ToModel(QueryPage<AccountBalance> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts the provided Account Accounting Period range to an API model.
    /// </summary>
    public AccountsInAccountingPeriodRangeModel ToModel(AccountAccountingPeriodRange range) => new()
    {
        Accounts = new CollectionModel<AccountWithBalanceRangeModel>
        {
            Items = range.Accounts.Items.Select(balance => new AccountWithBalanceRangeModel
            {
                Id = balance.Account.Id.Value,
                Name = balance.Account.Name,
                FinancialInstitution = balance.Account.FinancialInstitution,
                Type = AccountTypeConverter.ToModel(balance.Account.Type),
                StartingBalance = balance.StartingBalance,
                EndingBalance = balance.EndingBalance,
            }).ToList(),
            TotalCount = range.Accounts.TotalCount,
        },
        AvailableAccountNames = range.AvailableAccountNames,
        TotalIncome = new IncomeAmountModel
        {
            Total = range.TotalIncome,
            Tracked = range.TrackedIncome,
            Untracked = range.UntrackedIncome,
        },
        TotalSpending = range.TotalSpending,
        AccountingPeriods = range.AccountingPeriods.Select(ToModel).ToList(),
    };

    /// <summary>
    /// Converts the provided Account date range to an API model.
    /// </summary>
    public AccountsInDateRangeModel ToModel(AccountDateRange range) => new()
    {
        Accounts = new CollectionModel<AccountWithBalanceRangeModel>
        {
            Items = range.Accounts.Items.Select(balance => new AccountWithBalanceRangeModel
            {
                Id = balance.Account.Id.Value,
                Name = balance.Account.Name,
                FinancialInstitution = balance.Account.FinancialInstitution,
                Type = AccountTypeConverter.ToModel(balance.Account.Type),
                StartingBalance = balance.StartingBalance,
                EndingBalance = balance.EndingBalance,
            }).ToList(),
            TotalCount = range.Accounts.TotalCount,
        },
        AvailableAccountNames = range.AvailableAccountNames,
        TotalIncome = new IncomeAmountModel
        {
            Total = range.TotalIncome,
            Tracked = range.TrackedIncome,
            Untracked = range.UntrackedIncome,
        },
        TotalSpending = range.TotalSpending,
        Dates = range.Dates.Select(summary => new AccountBalanceSummaryByDateModel
        {
            Date = summary.Date,
            TotalBalance = summary.Balance.TotalBalance,
            TotalTrackedBalance = summary.Balance.TotalTrackedBalance,
            TotalUntrackedBalance = summary.Balance.TotalUntrackedBalance,
            BalanceByAccountType = summary.Balance.BalanceByAccountType.Select(balance => new AccountTypeBalanceModel
            {
                AccountType = AccountTypeConverter.ToModel(balance.AccountType),
                TotalBalance = balance.TotalBalance,
            }).ToList(),
        }).ToList(),
    };

    /// <summary>
    /// Converts the provided Account Period Balance Summary to an API model.
    /// </summary>
    private static AccountBalanceSummaryByPeriodModel ToModel(AccountPeriodBalanceSummary summary) => new()
    {
        AccountingPeriod = new Models.AccountingPeriods.AccountingPeriodModel
        {
            Id = summary.AccountingPeriod.Id.Value,
            Name = summary.AccountingPeriod.Name,
            Year = summary.AccountingPeriod.Year,
            Month = summary.AccountingPeriod.Month,
            IsOpen = summary.AccountingPeriod.IsOpen,
        },
        OpeningBalance = ToModel(summary.OpeningBalance),
        ClosingBalance = ToModel(summary.ClosingBalance),
    };

    /// <summary>
    /// Converts the provided Account Balance Summary to an API model.
    /// </summary>
    private static AccountBalanceSummaryModel ToModel(AccountBalanceSummary summary) => new()
    {
        TotalBalance = summary.TotalBalance,
        TotalTrackedBalance = summary.TotalTrackedBalance,
        TotalUntrackedBalance = summary.TotalUntrackedBalance,
        BalanceByAccountType = summary.BalanceByAccountType.Select(balance => new AccountTypeBalanceModel
        {
            AccountType = AccountTypeConverter.ToModel(balance.AccountType),
            TotalBalance = balance.TotalBalance,
        }).ToList(),
    };

    /// <summary>
    /// Converts the provided Account with Balance Range sort model to a Domain sort.
    /// </summary>
    private static AccountRangeSort ToDomain(AccountWithBalanceRangeSortModel? sort) => sort switch
    {
        AccountWithBalanceRangeSortModel.Name => AccountRangeSort.Name,
        AccountWithBalanceRangeSortModel.NameDescending => AccountRangeSort.NameDescending,
        AccountWithBalanceRangeSortModel.Type => AccountRangeSort.Type,
        AccountWithBalanceRangeSortModel.TypeDescending => AccountRangeSort.TypeDescending,
        AccountWithBalanceRangeSortModel.StartingBalance => AccountRangeSort.StartingBalance,
        AccountWithBalanceRangeSortModel.StartingBalanceDescending => AccountRangeSort.StartingBalanceDescending,
        AccountWithBalanceRangeSortModel.EndingBalance => AccountRangeSort.EndingBalance,
        AccountWithBalanceRangeSortModel.EndingBalanceDescending => AccountRangeSort.EndingBalanceDescending,
        AccountWithBalanceRangeSortModel.NetChange => AccountRangeSort.NetChange,
        AccountWithBalanceRangeSortModel.NetChangeDescending => AccountRangeSort.NetChangeDescending,
        _ => AccountRangeSort.Name,
    };

    /// <summary>
    /// Converts the provided Account filter model to a Domain filter.
    /// </summary>
    private static AccountFilter ToDomain(AccountFilterModel? model) => new(
        model?.NameSearch,
        model?.Names ?? [],
        model?.Types?.Select(type => (AccountType)type).ToList() ?? []);
}