using Models;
using Models.Accounts;
using Tests.AccountingPeriods;
using Tests.Infrastructure;

namespace Tests.Accounts;

/// <summary>
/// Covers account-type API conversion.
/// </summary>
public sealed class AccountTypeCoverageTests
{
    /// <summary>
    /// Preserves every configured account type through creation and balance snapshots.
    /// </summary>
    [Fact]
    public async Task AccountTypesExposeTheirConfiguredTypeInBalanceSnapshots()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        var accounts = new List<(AccountHandle Handle, AccountTypeModel Type)>();
        foreach (AccountTypeModel type in Enum.GetValues<AccountTypeModel>())
        {
            AccountModel account = await test.Api.PostAsync<CreateAccountModel, AccountModel>("/accounts", new CreateAccountModel
            {
                Name = type.ToString(),
                Type = type,
                OpeningAccountingPeriodId = july.Id,
                DateOpened = new DateOnly(2026, 7, 1)
            });
            accounts.Add((new AccountHandle(account.Id, account.Name), type));
        }

        CollectionModel<AccountWithBalanceModel> response = await test.Api.GetAsync<CollectionModel<AccountWithBalanceModel>>("/accounts/with-balances");
        foreach ((AccountHandle account, AccountTypeModel type) in accounts)
        {
            AccountWithBalanceModel model = response.Items.Single(item => item.Id == account.Id);
            Assert.Equal(type, model.Type);
            Assert.Equal(0m, model.CurrentBalance.PostedBalance);
        }
    }
}
