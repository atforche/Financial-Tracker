using System.Net;
using Domain.AccountGoals;
using Models.AccountGoals;
using Models.Accounts;

namespace Tests.AccountGoals;

/// <summary>
/// Covers Account Goal lifecycle integration with Accounts and Accounting Periods.
/// </summary>
public sealed class AccountGoalPeriodLifecycleTests
{
    /// <summary>
    /// Creates an onboarding Account Goal for standard Accounts and copies it into the first period.
    /// </summary>
    [Fact]
    public async Task OnboardingGoalIsCopiedToFirstAccountingPeriod()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle standard = await test.Accounts.Onboard("Checking").WithOpeningBalance(100m).CreateAsync();
        AccountHandle card = await test.Accounts.Onboard("Card").WithType(AccountTypeModel.CreditCard).WithOpeningBalance(20m).CreateAsync();

        IReadOnlyCollection<AccountGoal> onboardingGoals = test.AccountGoalQueries.GetForAccount(standard.Id);
        _ = Assert.Single(onboardingGoals);
        Assert.Empty(test.AccountGoalQueries.GetForAccount(card.Id));

        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        IReadOnlyCollection<AccountGoal> periodGoals = test.AccountGoalQueries.GetForPeriod(july.Id);
        AccountGoal copiedGoal = Assert.Single(periodGoals, goal => goal.Account.Id.Value == standard.Id);
        Assert.NotEqual(Assert.Single(onboardingGoals).Id, copiedGoal.Id);
        Assert.Contains(test.AccountGoalQueries.GetForAccount(standard.Id), goal => goal.AccountingPeriod == null);
    }

    /// <summary>
    /// Creates one Account Goal for every period applicable to a standard Account.
    /// </summary>
    [Fact]
    public async Task AccountCreatedInPeriodReceivesGoalsThroughLaterPeriods()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        AccountModel created = await test.Api.PostAsync<CreateAccountModel, AccountModel>("/accounts", new CreateAccountModel
        {
            Name = "Period savings",
            Type = AccountTypeModel.Standard,
            OpeningAccountingPeriodId = july.Id,
            DateOpened = new DateOnly(2026, 7, 1),
        });
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();

        IReadOnlyCollection<AccountGoal> goals = test.AccountGoalQueries.GetForAccount(created.Id);
        Assert.Equal(1, goals.Count(goal => goal.AccountingPeriod?.Id.Value == july.Id));
        Assert.Equal(1, goals.Count(goal => goal.AccountingPeriod?.Id.Value == august.Id));
    }

    /// <summary>
    /// Applies the goal bounds given when creating a standard Account to each of its periods.
    /// </summary>
    [Fact]
    public async Task AccountCreatedWithGoalBoundsAppliesThemToEveryPeriod()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();

        AccountModel created = await test.Api.PostAsync<CreateAccountModel, AccountModel>("/accounts", new CreateAccountModel
        {
            Name = "Goal savings",
            Type = AccountTypeModel.Standard,
            OpeningAccountingPeriodId = july.Id,
            DateOpened = new DateOnly(2026, 7, 1),
            MinimumEndingBalance = 100m,
            MaximumEndingBalance = 500m,
        });

        foreach (AccountingPeriodHandle period in new[] { july, august })
        {
            AccountGoalModel goal = await test.Api.GetAccountGoalAsync(created.Id, period.Id);
            Assert.Equal(100m, goal.MinimumEndingBalance);
            Assert.Equal(500m, goal.MaximumEndingBalance);
        }
    }

    /// <summary>
    /// Applies the goal bounds given when onboarding a standard Account to its onboarding goal.
    /// </summary>
    [Fact]
    public async Task AccountOnboardedWithGoalBoundsKeepsThem()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();

        AccountModel onboarded = await test.Api.PostAsync<OnboardAccountModel, AccountModel>("/accounts/onboard", new OnboardAccountModel
        {
            Name = "Onboarded savings",
            Type = AccountTypeModel.Standard,
            OnboardedBalance = 0m,
            MinimumEndingBalance = 25m,
        });

        AccountGoal goal = Assert.Single(test.AccountGoalQueries.GetForAccount(onboarded.Id));
        Assert.Equal(25m, goal.MinimumEndingBalance);
        Assert.Null(goal.MaximumEndingBalance);
    }

    /// <summary>
    /// Rejects goal bounds that are out of order, or given for an Account that has no goals, without creating it.
    /// </summary>
    [Fact]
    public async Task AccountCreationRejectsInvalidGoalBounds()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();

        HttpResponseMessage outOfOrder = await test.Api.PostResponseAsync("/accounts", new CreateAccountModel
        {
            Name = "Backwards",
            Type = AccountTypeModel.Standard,
            OpeningAccountingPeriodId = july.Id,
            DateOpened = new DateOnly(2026, 7, 1),
            MinimumEndingBalance = 500m,
            MaximumEndingBalance = 100m,
        });
        HttpResponseMessage notStandard = await test.Api.PostResponseAsync("/accounts", new CreateAccountModel
        {
            Name = "Card",
            Type = AccountTypeModel.CreditCard,
            OpeningAccountingPeriodId = july.Id,
            DateOpened = new DateOnly(2026, 7, 1),
            MinimumEndingBalance = 10m,
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, outOfOrder.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, notStandard.StatusCode);
        Assert.Empty(test.AccountGoalQueries.GetForPeriod(july.Id));
    }

    /// <summary>
    /// Copies Account Goal configuration without changing the previous period's goal.
    /// </summary>
    [Fact]
    public async Task CreatingNextPeriodCopiesAccountGoalAndPreservesOriginal()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle account = await test.Accounts.Onboard("Checking").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountGoalModel julyGoal = await test.Api.GetAccountGoalAsync(account.Id, july.Id);
        _ = await test.Api.PostAsync<UpdateAccountGoalModel, AccountGoalModel>(
            $"/account-goals/{julyGoal.Id}",
            new UpdateAccountGoalModel
            {
                MinimumEndingBalance = 50m,
                MaximumEndingBalance = 150m,
            });

        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();

        AccountGoalModel original = await test.Api.GetAccountGoalAsync(account.Id, july.Id);
        AccountGoalModel copied = await test.Api.GetAccountGoalAsync(account.Id, august.Id);
        Assert.NotEqual(original.Id, copied.Id);
        Assert.Equal(50m, original.MinimumEndingBalance);
        Assert.Equal(150m, original.MaximumEndingBalance);
        Assert.Equal(50m, copied.MinimumEndingBalance);
        Assert.Equal(150m, copied.MaximumEndingBalance);
    }

    /// <summary>
    /// Deletes Account Goals when their Account or Accounting Period is deleted.
    /// </summary>
    [Fact]
    public async Task DeletingAccountAndLatestPeriodDeletesTheirGoals()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountHandle onboarded = await test.Accounts.Onboard("Onboarded savings").CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountingPeriodHandle august = await test.Periods.Create(2026, 8).CreateAsync();
        AccountModel account = await test.Api.PostAsync<CreateAccountModel, AccountModel>("/accounts", new CreateAccountModel
        {
            Name = "Period savings",
            Type = AccountTypeModel.Standard,
            OpeningAccountingPeriodId = august.Id,
            DateOpened = new DateOnly(2026, 8, 1),
        });

        Assert.NotEmpty(test.AccountGoalQueries.GetForAccount(account.Id));
        await test.Api.DeleteAsync($"/accounts/{account.Id}");
        Assert.Empty(test.AccountGoalQueries.GetForAccount(account.Id));

        await test.Api.DeleteAsync($"/accounting-periods/{august.Id}");
        Assert.DoesNotContain(test.AccountGoalQueries.GetForAccount(onboarded.Id), goal => goal.AccountingPeriod?.Id.Value == august.Id);
        Assert.NotEmpty(test.AccountGoalQueries.GetForPeriod(july.Id));
    }
}
