using Models.Accounts;
using Tests.AccountingPeriods;
using Tests.Funds;
using Tests.Infrastructure;
using Tests.Transactions;

namespace Tests.Accounts;

/// <summary>
/// Covers balance direction for debt-like Accounts.
/// </summary>
public sealed class DebtAccountBalanceTests
{
    /// <summary>
    /// Increases a credit-card balance for pending and posted spending.
    /// </summary>
    [Fact]
    public async Task CreditCardSpendingIncreasesPostedAndPendingBalances()
    {
        await using FinancialTrackerTestContext test = await FinancialTrackerTestContext.CreateAsync();
        AccountingPeriodHandle july = await test.Periods.Create(2026, 7).CreateAsync();
        AccountModel card = await test.Api.PostAsync<CreateAccountModel, AccountModel>("/accounts", new CreateAccountModel
        {
            Name = "Card",
            Type = AccountTypeModel.CreditCard,
            OpeningAccountingPeriodId = july.Id,
            DateOpened = new DateOnly(2026, 7, 1)
        });
        AccountHandle cardHandle = new(card.Id, card.Name);
        FundHandle groceries = await test.Funds.Create("Groceries").In(july).CreateAsync();
        TransactionHandle transaction = await test.Transactions.Spending().In(july).On(new DateOnly(2026, 7, 15)).For(25m).From(cardHandle).To("Market", groceries).CreateAsync();

        AccountBalanceSnapshot pending = await test.AccountQueries.GetBalanceAsync(cardHandle);
        await test.Transactions.PostAsync(transaction, cardHandle, new DateOnly(2026, 7, 15));
        AccountBalanceSnapshot posted = await test.AccountQueries.GetBalanceAsync(cardHandle);

        Assert.Equal(0m, pending.Posted);
        Assert.Equal(25m, pending.IncludingPending);
        Assert.Equal(25m, posted.Posted);
        Assert.Equal(25m, posted.IncludingPending);
    }
}
