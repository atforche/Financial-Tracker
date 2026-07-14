using Domain.Accounts;
using Domain.Funds;
using Domain.Goals;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Microsoft.EntityFrameworkCore;
using Models.AccountingPeriods;
using Models.Accounts;
using Models.BalanceEvents;
using Models.Funds;
using Models.Goals;
using Models.Transactions;
using Models.Transactions.Types;

namespace Data.Transactions;

/// <summary>
/// Maps materialized Transaction pages to the polymorphic API contract.
/// </summary>
public sealed class TransactionModelMapper(DatabaseContext databaseContext)
{
    /// <summary>
    /// Maps a page of Transactions without issuing per-item queries.
    /// </summary>
    public async Task<IReadOnlyCollection<TransactionModel>> MapAsync(
        IReadOnlyCollection<Transaction> transactions,
        CancellationToken cancellationToken = default)
    {
        var periodIds = transactions.Select(transaction => transaction.AccountingPeriodId).Distinct().ToList();
        Dictionary<Guid, AccountingPeriodModel> periods = await databaseContext.AccountingPeriods.AsNoTracking()
            .Where(period => periodIds.Contains(period.Id))
            .Select(period => new AccountingPeriodModel
            {
                Id = period.Id.Value,
                Name = period.Name,
                Year = period.Year,
                Month = period.Month,
                IsOpen = period.IsOpen,
            }).ToDictionaryAsync(period => period.Id, cancellationToken);
        var fundIds = transactions.SelectMany(transaction => transaction.GetAllAffectedFundIds(null)).Distinct().ToList();
        Dictionary<Guid, FundModel> funds = await databaseContext.Funds.AsNoTracking().Where(fund => fundIds.Contains(fund.Id))
            .Select(fund => new FundModel { Id = fund.Id.Value, Name = fund.Name, Description = fund.Description })
            .ToDictionaryAsync(fund => fund.Id, cancellationToken);
        var accountIds = transactions.SelectMany(transaction => transaction.GetAllAffectedAccountIds()).Distinct().ToList();
        List<AccountBalanceHistory> accountHistories = await databaseContext.AccountBalanceHistories.AsNoTracking()
            .Where(history => accountIds.Contains(history.Account.Id)).OrderBy(history => history.Date).ThenBy(history => history.Sequence).ToListAsync(cancellationToken);
        List<FundBalanceHistory> fundHistories = await databaseContext.FundBalanceHistories.AsNoTracking()
            .Where(history => fundIds.Contains(history.FundId)).OrderBy(history => history.Date).ThenBy(history => history.Sequence).ToListAsync(cancellationToken);
        List<GoalBalanceHistory> goalHistories = await databaseContext.GoalBalanceHistories.AsNoTracking()
            .Where(history => fundIds.Contains(history.FundId))
            .OrderBy(history => history.Date).ThenBy(history => history.Sequence).ToListAsync(cancellationToken);
        MappingContext context = new(periods, funds, accountHistories, fundHistories, goalHistories);
        return transactions.Select(transaction => Map(transaction, context)).ToList();
    }

    /// <summary>
    /// Maps the provided Transaction to a Transaction model
    /// </summary>
    private static TransactionModel Map(Transaction transaction, MappingContext context) => transaction switch
    {
        SpendingTransaction spending => new SpendingTransactionModel
        {
            Id = transaction.Id.Value,
            TransactionType = TransactionTypeModel.Spending,
            AccountingPeriodId = context.Periods[transaction.AccountingPeriodId.Value].Id,
            AccountingPeriodName = context.Periods[transaction.AccountingPeriodId.Value].Name,
            Date = transaction.Date,
            Sequence = transaction.Sequence,
            Description = transaction.Description,
            Amount = transaction.Amount,
            Source = new SpendingTransactionSourceModel { Account = AccountEvent(transaction, context, spending.Source.Account, spending.Source.PostedDate, transaction.Amount, BalanceEventTypeModel.Debit) },
            Destinations = spending.Destinations.Select(destination => new SpendingTransactionDestinationModel
            {
                Account = destination.Account == null ? null : AccountEvent(transaction, context, destination.Account, destination.PostedDate, destination.Amount, BalanceEventTypeModel.Credit),
                Location = destination.Location,
                Amount = destination.Amount,
                PostedDate = destination.PostedDate,
                FundAssignments = destination.FundAssignments.Select(assignment => FundEvent(transaction, context, assignment, BalanceEventTypeModel.Debit)).ToList(),
                Goals = destination.FundAssignments
                    .Where(assignment => assignment.FundId != Fund.UnassignedFundId)
                    .Select(assignment => GoalEvent(transaction, context, assignment, destination.PostedDate, BalanceEventTypeModel.Debit)).ToList(),
            }).ToList(),
        },
        IncomeTransaction income => new IncomeTransactionModel
        {
            Id = transaction.Id.Value,
            TransactionType = TransactionTypeModel.Income,
            AccountingPeriodId = context.Periods[transaction.AccountingPeriodId.Value].Id,
            AccountingPeriodName = context.Periods[transaction.AccountingPeriodId.Value].Name,
            Date = transaction.Date,
            Sequence = transaction.Sequence,
            Description = transaction.Description,
            Amount = transaction.Amount,
            TrackedAmount = income.TrackedAmount,
            Source = new IncomeTransactionSourceModel
            {
                Account = income.Source.Account == null ? null : AccountEvent(transaction, context, income.Source.Account, income.Source.PostedDate, transaction.Amount, BalanceEventTypeModel.Debit),
                Location = income.Source.Location,
                IncomeLines = income.Source.IncomeLines.Select(line => new IncomeLineModel { Description = line.Description, Amount = line.Amount }).ToList(),
                IncomeDeductions = income.Source.IncomeDeductions.Select(deduction => new IncomeDeductionModel { Description = deduction.Description, Amount = deduction.Amount }).ToList(),
            },
            Destinations = income.Destinations.Select(destination => new IncomeTransactionDestinationModel
            {
                Account = AccountEvent(transaction, context, destination.Account, destination.PostedDate, destination.Amount, BalanceEventTypeModel.Credit),
                Amount = destination.Amount,
                PostedDate = destination.PostedDate,
                FundAssignments = destination.FundAssignments.Select(assignment => FundEvent(transaction, context, assignment, BalanceEventTypeModel.Credit)).ToList(),
                Goals = destination.FundAssignments
                    .Where(assignment => assignment.FundId != Fund.UnassignedFundId)
                    .Select(assignment => GoalEvent(transaction, context, assignment, destination.PostedDate, BalanceEventTypeModel.Credit)).ToList(),
            }).ToList(),
        },
        AccountTransaction account => new AccountTransactionModel
        {
            Id = transaction.Id.Value,
            TransactionType = TransactionTypeModel.Account,
            AccountingPeriodId = context.Periods[transaction.AccountingPeriodId.Value].Id,
            AccountingPeriodName = context.Periods[transaction.AccountingPeriodId.Value].Name,
            Date = transaction.Date,
            Sequence = transaction.Sequence,
            Description = transaction.Description,
            Amount = transaction.Amount,
            Source = new AccountTransactionSourceModel
            {
                Account = account.Source.Account == null ? null : AccountEvent(transaction, context, account.Source.Account, account.Source.PostedDate, transaction.Amount, BalanceEventTypeModel.Debit),
                Location = account.Source.Location,
            },
            Destinations = account.Destinations.Select(destination => new AccountTransactionDestinationModel
            {
                Account = destination.Account == null ? null : AccountEvent(transaction, context, destination.Account, destination.PostedDate, destination.Amount, BalanceEventTypeModel.Credit),
                Location = destination.Location,
                Amount = destination.Amount,
                PostedDate = destination.PostedDate,
            }).ToList(),
        },
        FundTransaction fund => new FundTransactionModel
        {
            Id = transaction.Id.Value,
            TransactionType = TransactionTypeModel.Fund,
            AccountingPeriodId = context.Periods[transaction.AccountingPeriodId.Value].Id,
            AccountingPeriodName = context.Periods[transaction.AccountingPeriodId.Value].Name,
            Date = transaction.Date,
            Sequence = transaction.Sequence,
            Description = transaction.Description,
            Amount = transaction.Amount,
            Source = new FundTransactionSourceModel
            {
                Fund = FundEvent(transaction, context, new FundAmount { FundId = fund.Source.Fund.Id, Amount = transaction.Amount }, BalanceEventTypeModel.Debit),
                Goal = fund.Source.Fund.Id == Fund.UnassignedFundId
                    ? null
                    : GoalEvent(transaction, context, new FundAmount { FundId = fund.Source.Fund.Id, Amount = transaction.Amount }, transaction.Date, BalanceEventTypeModel.Debit),
            },
            Destinations = fund.Destinations.Select(destination => new FundTransactionDestinationModel
            {
                Fund = FundEvent(transaction, context, new FundAmount { FundId = destination.Fund.Id, Amount = destination.Amount }, BalanceEventTypeModel.Credit),
                Goal = destination.Fund.Id == Fund.UnassignedFundId
                    ? null
                    : GoalEvent(transaction, context, new FundAmount { FundId = destination.Fund.Id, Amount = destination.Amount }, transaction.Date, BalanceEventTypeModel.Credit),
            }).ToList(),
        },
        _ => throw new InvalidOperationException($"Unrecognized Transaction type '{transaction.GetType().Name}'."),
    };

    /// <summary>
    /// Maps the provided Transaction and Account to an Account Balance Event Model 
    /// </summary>
    private static AccountBalanceEventModel AccountEvent(Transaction transaction, MappingContext context, Account account, DateOnly? postedDate, decimal amount, BalanceEventTypeModel type)
    {
        var histories = context.AccountHistories.Where(history => history.Account.Id == account.Id).ToList();
        AccountBalanceHistory? current = histories.LastOrDefault(history => history.TransactionId == transaction.Id && history.Date == (postedDate ?? transaction.Date));
        int index = current == null ? -1 : histories.IndexOf(current);
        AccountBalanceHistory? previous = index > 0 ? histories[index - 1] : null;
        return new AccountBalanceEventModel
        {
            AccountingPeriod = context.Periods[transaction.AccountingPeriodId.Value],
            TransactionId = transaction.Id.Value,
            Date = postedDate,
            Type = type,
            IsPosted = postedDate.HasValue,
            Amount = amount,
            Account = new AccountModel { Id = account.Id.Value, Name = account.Name, Type = (AccountTypeModel)account.Type },
            PreviousBalance = ToBalance(previous),
            NewBalance = ToBalance(current),
        };
    }

    /// <summary>
    /// Maps the provided Transaction and Fund to a Fund Balance Event Model
    /// </summary>
    private static FundBalanceEventModel FundEvent(Transaction transaction, MappingContext context, FundAmount assignment, BalanceEventTypeModel type)
    {
        var histories = context.FundHistories.Where(history => history.FundId == assignment.FundId).ToList();
        FundBalanceHistory? current = histories.LastOrDefault(history => history.TransactionId == transaction.Id);
        int index = current == null ? -1 : histories.IndexOf(current);
        FundBalanceHistory? previous = index > 0 ? histories[index - 1] : null;
        return new FundBalanceEventModel
        {
            AccountingPeriod = context.Periods[transaction.AccountingPeriodId.Value],
            TransactionId = transaction.Id.Value,
            Date = transaction.Date,
            Type = type,
            IsPosted = true,
            Amount = assignment.Amount,
            Fund = context.Funds[assignment.FundId.Value],
            PreviousBalance = ToBalance(previous),
            NewBalance = ToBalance(current),
        };
    }

    private static GoalBalanceEventModel GoalEvent(
        Transaction transaction,
        MappingContext context,
        FundAmount amount,
        DateOnly? postedDate,
        BalanceEventTypeModel type)
    {
        DateOnly historyDate = postedDate ?? transaction.Date;
        var histories = context.GoalHistories
            .Where(history => history.FundId == amount.FundId && history.AccountingPeriodId == transaction.AccountingPeriodId)
            .ToList();
        GoalBalanceHistory? current = histories.LastOrDefault(history =>
            history.TransactionId == transaction.Id && history.Date == historyDate);
        int index = current == null ? -1 : histories.IndexOf(current);
        GoalBalanceHistory? previous = index > 0 ? histories[index - 1] : null;
        return new GoalBalanceEventModel
        {
            AccountingPeriod = context.Periods[transaction.AccountingPeriodId.Value],
            TransactionId = transaction.Id.Value,
            Date = postedDate,
            Type = type,
            IsPosted = postedDate.HasValue,
            Amount = amount.Amount,
            Fund = context.Funds[amount.FundId.Value],
            PreviousBalance = ToBalance(previous),
            NewBalance = ToBalance(current),
        };
    }

    /// <summary>
    /// Maps the provided Account Balance History to an Account Balance Model
    /// </summary>
    /// <param name="history"></param>
    /// <returns></returns>
    private static AccountBalanceModel ToBalance(AccountBalanceHistory? history) => new()
    {
        PostedBalance = history?.PostedBalance ?? 0,
        PendingDebitAmount = history?.PendingDebitAmount ?? 0,
        PendingCreditAmount = history?.PendingCreditAmount ?? 0,
    };

    /// <summary>
    /// Maps the provided Fund Balance History to a Fund Balance Model
    /// </summary>
    /// <param name="history"></param>
    /// <returns></returns>
    private static FundBalanceModel ToBalance(FundBalanceHistory? history) => new()
    {
        PostedBalance = history?.PostedBalance ?? 0,
        PendingDebitAmount = history?.PendingDebitAmount ?? 0,
        PendingCreditAmount = history?.PendingCreditAmount ?? 0,
    };

    private static GoalBalanceModel ToBalance(GoalBalanceHistory? history) => new()
    {
        AmountAssigned = history?.AmountAssigned ?? 0,
        PendingAmountAssigned = history?.PendingAmountAssigned ?? 0,
        AmountSpent = history?.AmountSpent ?? 0,
        PendingAmountSpent = history?.PendingAmountSpent ?? 0,
    };

    /// <summary>
    /// Mapping context used to map Transactions
    /// </summary>
    private sealed record MappingContext(
        Dictionary<Guid, AccountingPeriodModel> Periods,
        Dictionary<Guid, FundModel> Funds,
        List<AccountBalanceHistory> AccountHistories,
        List<FundBalanceHistory> FundHistories,
        List<GoalBalanceHistory> GoalHistories);
}