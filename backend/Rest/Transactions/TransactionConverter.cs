using Domain;
using Domain.BalanceEvents;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Queries;
using Domain.Transactions.Spending;
using Models;
using Models.Transactions;
using Models.Transactions.Types;
using Rest.Accounts;
using Rest.FundGoals;
using Rest.Funds;
using Rest.Locations;

namespace Rest.Transactions;

/// <summary>
/// Converts interpreted Domain Transaction details to API models.
/// </summary>
public sealed class TransactionConverter(
    AccountBalanceEventConverter accountBalanceEventConverter,
    FundBalanceEventConverter fundBalanceEventConverter,
    FundGoalBalanceEventConverter fundGoalBalanceEventConverter,
    LocationConverter locationConverter)
{
    /// <summary>
    /// Converts an API Transaction query to Domain criteria.
    /// </summary>
    public TransactionQuery ToDomain(TransactionQueryParameterModel model) => new(
        ToDomain(model.Filter),
        ToDomain(model.Sort),
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts an API Transaction date-range query to Domain criteria.
    /// </summary>
    public TransactionDateRangeQuery ToDomain(TransactionsInDateRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End,
        ToDomain(model.Filter),
        ToDomain(model.Sort),
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts an API Transaction Accounting Period range query to Domain criteria.
    /// </summary>
    public TransactionAccountingPeriodRangeQuery ToDomain(TransactionsInAccountingPeriodRangeQueryParameterModel model) => new(
        model.Range.Start,
        model.Range.End,
        ToDomain(model.Filter),
        ToDomain(model.Sort),
        model.Offset ?? 0,
        model.Limit);

    /// <summary>
    /// Converts an API Transaction sort to a Domain sort.
    /// </summary>
    public TransactionSort ToDomain(TransactionSortModel? sort) => sort switch
    {
        TransactionSortModel.Date => TransactionSort.Date,
        TransactionSortModel.DateDescending => TransactionSort.DateDescending,
        TransactionSortModel.Description => TransactionSort.Description,
        TransactionSortModel.DescriptionDescending => TransactionSort.DescriptionDescending,
        TransactionSortModel.Amount => TransactionSort.Amount,
        TransactionSortModel.AmountDescending => TransactionSort.AmountDescending,
        TransactionSortModel.AccountingPeriod => TransactionSort.AccountingPeriod,
        TransactionSortModel.AccountingPeriodDescending => TransactionSort.AccountingPeriodDescending,
        TransactionSortModel.Source => TransactionSort.Source,
        TransactionSortModel.SourceDescending => TransactionSort.SourceDescending,
        TransactionSortModel.Destination => TransactionSort.Destination,
        TransactionSortModel.DestinationDescending => TransactionSort.DestinationDescending,
        TransactionSortModel.FullyPosted => TransactionSort.FullyPosted,
        TransactionSortModel.FullyPostedDescending => TransactionSort.FullyPostedDescending,
        _ => TransactionSort.DateDescending,
    };

    /// <summary>
    /// Converts an interpreted Domain page to an API collection.
    /// </summary>
    public CollectionModel<TransactionModel> ToModel(QueryPage<TransactionDetails> page) => new()
    {
        Items = page.Items.Select(ToModel).ToList(),
        TotalCount = page.TotalCount,
    };

    /// <summary>
    /// Converts an interpreted Transaction date range to an API model.
    /// </summary>
    public TransactionsInDateRangeModel ToModel(TransactionDateRange range) => new()
    {
        Transactions = ToModel(range.Transactions),
        AvailableAccountNames = range.AvailableAccountNames,
        AvailableFundNames = range.AvailableFundNames,
        TransactionTypes = range.TransactionTypes.Select(ToModel).ToList(),
        LocationIncomingAmount = range.LocationCashFlow.Incoming,
        LocationOutgoingAmount = range.LocationCashFlow.Outgoing,
        Offset = range.Offset,
        Limit = range.Limit,
    };

    /// <summary>
    /// Converts an interpreted Transaction Accounting Period range to an API model.
    /// </summary>
    public TransactionsInAccountingPeriodRangeModel ToModel(TransactionAccountingPeriodRange range) => new()
    {
        Transactions = ToModel(range.Transactions),
        AvailableAccountNames = range.AvailableAccountNames,
        AvailableFundNames = range.AvailableFundNames,
        TransactionTypes = range.TransactionTypes.Select(ToModel).ToList(),
        LocationIncomingAmount = range.LocationCashFlow.Incoming,
        LocationOutgoingAmount = range.LocationCashFlow.Outgoing,
        Offset = range.Offset,
        Limit = range.Limit,
    };

    /// <summary>
    /// Converts a Domain Transaction type summary to an API model.
    /// </summary>
    private static TransactionSummaryByTypeModel ToModel(TransactionTypeSummary summary) => new()
    {
        TransactionType = summary.TransactionType switch
        {
            TransactionType.Spending => TransactionTypeModel.Spending,
            TransactionType.Income => TransactionTypeModel.Income,
            TransactionType.Account => TransactionTypeModel.Account,
            TransactionType.Fund => TransactionTypeModel.Fund,
            _ => throw new ArgumentOutOfRangeException(nameof(summary), summary.TransactionType, null),
        },
        TotalCount = summary.TotalCount,
        TotalAmount = summary.TotalAmount,
    };

    /// <summary>
    /// Converts an API Transaction filter to Domain criteria.
    /// </summary>
    private static TransactionFilter ToDomain(TransactionFilterModel? filter) => new(
        filter?.AccountingPeriodIds ?? [],
        filter?.AccountIds ?? [],
        filter?.FundIds ?? [],
        filter?.LocationIds ?? [],
        (filter?.Types ?? []).Select(ToDomain).ToList());

    /// <summary>
    /// Converts an API Transaction Type to its Domain equivalent.
    /// </summary>
    private static TransactionType ToDomain(TransactionTypeModel type) => type switch
    {
        TransactionTypeModel.Spending => TransactionType.Spending,
        TransactionTypeModel.Income => TransactionType.Income,
        TransactionTypeModel.Account => TransactionType.Account,
        TransactionTypeModel.Fund => TransactionType.Fund,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    /// <summary>
    /// Converts interpreted Transaction details to the polymorphic API model.
    /// </summary>
    public TransactionModel ToModel(TransactionDetails details) => details.Transaction switch
    {
        SpendingTransaction spending => new SpendingTransactionModel
        {
            Id = spending.Id.Value,
            TransactionType = TransactionTypeModel.Spending,
            AccountingPeriodId = details.AccountingPeriod.Id.Value,
            AccountingPeriodName = details.AccountingPeriod.Name,
            Date = spending.Date,
            Sequence = spending.Sequence,
            Description = spending.Description,
            Amount = spending.Amount,
            FullyPosted = IsFullyPosted(spending),
            Source = new SpendingTransactionSourceModel
            {
                Account = accountBalanceEventConverter.ToModel(details.GetAccountEvent(
                    spending.Source.Account,
                    spending.Source.PostedDate,
                    spending.Amount,
                    BalanceEventType.Debit)),
            },
            Destinations = spending.Destinations.Select(destination => new SpendingTransactionDestinationModel
            {
                Account = destination.Account == null ? null : accountBalanceEventConverter.ToModel(details.GetAccountEvent(
                    destination.Account,
                    destination.PostedDate,
                    destination.Amount,
                    BalanceEventType.Credit)),
                Location = destination.Location == null ? null : locationConverter.ToModel(destination.Location, -destination.Amount),
                Amount = destination.Amount,
                PostedDate = destination.PostedDate,
                FundAssignments = destination.FundAssignments.Select(amount =>
                    fundBalanceEventConverter.ToModel(details.GetFundEvent(amount, BalanceEventType.Debit))).ToList(),
                FundGoals = destination.FundAssignments.Where(amount => amount.FundId != Fund.UnassignedFundId).Select(amount =>
                    fundGoalBalanceEventConverter.ToModel(details.GetFundGoalEvent(
                        amount,
                        destination.Account == null ? spending.Source.PostedDate : destination.PostedDate,
                        BalanceEventType.Debit))).ToList(),
            }).ToList(),
        },
        IncomeTransaction income => new IncomeTransactionModel
        {
            Id = income.Id.Value,
            TransactionType = TransactionTypeModel.Income,
            AccountingPeriodId = details.AccountingPeriod.Id.Value,
            AccountingPeriodName = details.AccountingPeriod.Name,
            Date = income.Date,
            Sequence = income.Sequence,
            Description = income.Description,
            Amount = income.Amount,
            FullyPosted = IsFullyPosted(income),
            TrackedAmount = income.TrackedAmount,
            Source = new IncomeTransactionSourceModel
            {
                Account = income.Source.Account == null ? null : accountBalanceEventConverter.ToModel(details.GetAccountEvent(
                    income.Source.Account,
                    income.Source.PostedDate,
                    income.Amount,
                    BalanceEventType.Debit)),
                Location = income.Source.Location == null ? null : locationConverter.ToModel(income.Source.Location, income.Amount),
                IncomeLines = income.Source.IncomeLines.Select(line => new IncomeLineModel
                {
                    Description = line.Description,
                    Amount = line.Amount,
                }).ToList(),
                IncomeDeductions = income.Source.IncomeDeductions.Select(deduction => new IncomeDeductionModel
                {
                    Description = deduction.Description,
                    Amount = deduction.Amount,
                }).ToList(),
            },
            Destinations = income.Destinations.Select(destination => new IncomeTransactionDestinationModel
            {
                Account = accountBalanceEventConverter.ToModel(details.GetAccountEvent(
                    destination.Account,
                    destination.PostedDate,
                    destination.Amount,
                    BalanceEventType.Credit)),
                Amount = destination.Amount,
                PostedDate = destination.PostedDate,
                FundAssignments = destination.FundAssignments.Select(amount =>
                    fundBalanceEventConverter.ToModel(
                        details.GetFundEvent(amount, BalanceEventType.Credit),
                        amount.IsExtraContribution)).ToList(),
                FundGoals = destination.FundAssignments.Where(amount => amount.FundId != Fund.UnassignedFundId).Select(amount =>
                    fundGoalBalanceEventConverter.ToModel(details.GetFundGoalEvent(
                        amount,
                        destination.PostedDate,
                        BalanceEventType.Credit))).ToList(),
            }).ToList(),
        },
        AccountTransaction account => new AccountTransactionModel
        {
            Id = account.Id.Value,
            TransactionType = TransactionTypeModel.Account,
            AccountingPeriodId = details.AccountingPeriod.Id.Value,
            AccountingPeriodName = details.AccountingPeriod.Name,
            Date = account.Date,
            Sequence = account.Sequence,
            Description = account.Description,
            Amount = account.Amount,
            FullyPosted = IsFullyPosted(account),
            Source = new AccountTransactionSourceModel
            {
                Account = account.Source.Account == null ? null : accountBalanceEventConverter.ToModel(details.GetAccountEvent(
                    account.Source.Account,
                    account.Source.PostedDate,
                    account.Amount,
                    BalanceEventType.Debit)),
                Location = account.Source.Location == null ? null : locationConverter.ToModel(account.Source.Location, account.Amount),
            },
            Destinations = account.Destinations.Select(destination => new AccountTransactionDestinationModel
            {
                Account = destination.Account == null ? null : accountBalanceEventConverter.ToModel(details.GetAccountEvent(
                    destination.Account,
                    destination.PostedDate,
                    destination.Amount,
                    BalanceEventType.Credit)),
                Location = destination.Location == null ? null : locationConverter.ToModel(destination.Location, -destination.Amount),
                Amount = destination.Amount,
                PostedDate = destination.PostedDate,
            }).ToList(),
        },
        FundTransaction fund => new FundTransactionModel
        {
            Id = fund.Id.Value,
            TransactionType = TransactionTypeModel.Fund,
            AccountingPeriodId = details.AccountingPeriod.Id.Value,
            AccountingPeriodName = details.AccountingPeriod.Name,
            Date = fund.Date,
            Sequence = fund.Sequence,
            Description = fund.Description,
            Amount = fund.Amount,
            FullyPosted = IsFullyPosted(fund),
            Source = new FundTransactionSourceModel
            {
                Fund = fundBalanceEventConverter.ToModel(details.GetFundEvent(
                    new FundAmount { FundId = fund.Source.Fund.Id, Amount = fund.Amount },
                    BalanceEventType.Debit)),
                FundGoal = fund.Source.Fund.Id == Fund.UnassignedFundId ? null : fundGoalBalanceEventConverter.ToModel(
                    details.GetFundGoalEvent(
                        new FundAmount { FundId = fund.Source.Fund.Id, Amount = fund.Amount },
                        fund.Date,
                        BalanceEventType.Debit)),
            },
            Destinations = fund.Destinations.Select(destination => new FundTransactionDestinationModel
            {
                Fund = fundBalanceEventConverter.ToModel(details.GetFundEvent(
                    new FundAmount { FundId = destination.Fund.Id, Amount = destination.Amount },
                    BalanceEventType.Credit)),
                FundGoal = destination.Fund.Id == Fund.UnassignedFundId ? null : fundGoalBalanceEventConverter.ToModel(
                    details.GetFundGoalEvent(
                        new FundAmount { FundId = destination.Fund.Id, Amount = destination.Amount },
                        fund.Date,
                        BalanceEventType.Credit)),
            }).ToList(),
        },
        _ => throw new InvalidOperationException($"Unrecognized Transaction type '{details.Transaction.GetType().Name}'."),
    };

    /// <summary>
    /// Determines whether a Transaction is posted to every affected Account.
    /// </summary>
    private static bool IsFullyPosted(Transaction transaction) => transaction.GetAllAffectedAccountIds()
        .All(accountId => transaction.GetPostedDateForAccount(accountId) != null);
}
