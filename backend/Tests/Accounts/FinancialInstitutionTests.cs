using Models;
using Models.Accounts;
using Tests.AccountingPeriods;
using Tests.Infrastructure;

namespace Tests.Accounts;

/// <summary>
/// Covers Account financial institution persistence and lookup contracts.
/// </summary>
public sealed class FinancialInstitutionTests
{
    /// <summary>
    /// Normalizes financial institutions during create and update, and returns the sorted distinct lookup list.
    /// </summary>
    [Fact]
    public async Task FinancialInstitutionsNormalizeAndListAsync()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountModel onboarded = await test.Api.PostAsync<OnboardAccountModel, AccountModel>("/accounts/onboard", new OnboardAccountModel
        {
            Name = "Checking",
            FinancialInstitution = "  Chase  ",
            Type = AccountTypeModel.Standard,
            OnboardedBalance = 100m
        });
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountModel created = await test.Api.PostAsync<CreateAccountModel, AccountModel>("/accounts", new CreateAccountModel
        {
            Name = "Savings",
            FinancialInstitution = " Ally ",
            Type = AccountTypeModel.Standard,
            OpeningAccountingPeriodId = july.Id,
            DateOpened = new DateOnly(2026, 7, 1)
        });

        Assert.Equal("Chase", onboarded.FinancialInstitution);
        Assert.Equal("Ally", created.FinancialInstitution);

        CollectionModel<string> institutions = await test.Api.GetAsync<CollectionModel<string>>("/accounts/financial-institutions");
        Assert.Equal(["Ally", "Chase"], institutions.Items);
        Assert.Equal(2, institutions.TotalCount);

        AccountModel updated = await test.Api.PostAsync<UpdateAccountModel, AccountModel>($"/accounts/{created.Id}", new UpdateAccountModel
        {
            Name = "Savings",
            FinancialInstitution = "  Bank of America  "
        });
        Assert.Equal("Bank of America", updated.FinancialInstitution);

        AccountModel cleared = await test.Api.PostAsync<UpdateAccountModel, AccountModel>($"/accounts/{created.Id}", new UpdateAccountModel
        {
            Name = "Savings",
            FinancialInstitution = "  "
        });
        Assert.Null(cleared.FinancialInstitution);

        institutions = await test.Api.GetAsync<CollectionModel<string>>("/accounts/financial-institutions");
        Assert.Equal(["Chase"], institutions.Items);
        Assert.Equal(1, institutions.TotalCount);
    }

    /// <summary>
    /// Returns no institutions when no Account has an assigned institution.
    /// </summary>
    [Fact]
    public async Task FinancialInstitutionsListAsyncExcludesUnassignedAccounts()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        _ = await test.Accounts.Onboard("Checking").CreateAsync();

        CollectionModel<string> institutions = await test.Api.GetAsync<CollectionModel<string>>("/accounts/financial-institutions");

        Assert.Empty(institutions.Items);
        Assert.Equal(0, institutions.TotalCount);
    }
}