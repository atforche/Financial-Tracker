using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Locations;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Refunds;
using Domain.Transactions.Spending;
using Models.Funds;
using Models.Locations;
using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Update;

namespace Rest.Transactions;

/// <summary>
/// Converts REST Transaction request models to Domain Transaction requests.
/// </summary>
public sealed class TransactionRequestConverter(
    IAccountRepository accountRepository,
    IFundRepository fundRepository,
    ILocationRepository locationRepository,
    LocationService locationService)
{
    /// <summary>
    /// Converts a REST create model to a Domain create request.
    /// </summary>
    public async Task<CreateTransactionRequest?> ToCreateRequestAsync(
        AccountingPeriod accountingPeriod,
        CreateTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        CreateTransactionRequest? request = model switch
        {
            CreateSpendingTransactionModel spending => await BuildSpendingCreateRequestAsync(accountingPeriod, spending, errors),
            CreateIncomeTransactionModel income => await BuildIncomeCreateRequestAsync(accountingPeriod, income, errors),
            CreateAccountTransactionModel account => await BuildAccountCreateRequestAsync(accountingPeriod, account, errors),
            CreateFundTransactionModel fund => await BuildFundCreateRequestAsync(accountingPeriod, fund, errors),
            CreateRefundTransactionModel refund => await BuildRefundCreateRequestAsync(accountingPeriod, refund, errors),
            _ => null
        };
        if (request != null)
        {
            return request;
        }
        if (errors.Count == 0)
        {
            errors.Add("type", [$"Unsupported transaction request type: {model.GetType().Name}."]);
        }
        return null;
    }

    /// <summary>
    /// Converts a REST update model to a Domain update request.
    /// </summary>
    public async Task<UpdateTransactionRequest?> ToUpdateRequestAsync(
        Transaction transaction,
        UpdateTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        TransactionTypeModel requestType = TransactionTypeConverter.ToModel(model);
        if (TransactionTypeConverter.ToModel(transaction.Type) != requestType)
        {
            errors.Add("type", [$"Transaction type {requestType} does not match existing transaction type {transaction.Type}."]);
            return null;
        }
        UpdateTransactionRequest? request = (transaction, model) switch
        {
            (SpendingTransaction, UpdateSpendingTransactionModel spending) => await BuildSpendingUpdateRequestAsync(spending, errors),
            (IncomeTransaction, UpdateIncomeTransactionModel income) => await BuildIncomeUpdateRequestAsync(income, errors),
            (AccountTransaction, UpdateAccountTransactionModel account) => await BuildAccountUpdateRequestAsync(account, errors),
            (FundTransaction, UpdateFundTransactionModel fund) => await BuildFundUpdateRequestAsync(fund, errors),
            (RefundTransaction, UpdateRefundTransactionModel refund) => await BuildRefundUpdateRequestAsync(refund, errors),
            _ => null
        };
        if (request != null)
        {
            return request;
        }
        if (errors.Count == 0)
        {
            errors.Add("type", [$"Request type {model.GetType().Name} is not valid for transaction type {transaction.GetType().Name}."]);
        }
        return null;
    }

    private async Task<CreateSpendingTransactionRequest?> BuildSpendingCreateRequestAsync(
        AccountingPeriod accountingPeriod,
        CreateSpendingTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        SpendingTransactionSource? source = GetSpendingSource(model.Source, nameof(CreateSpendingTransactionModel.Source), errors);
        if (source == null)
        {
            return null;
        }
        IReadOnlyCollection<SpendingTransactionDestination>? destinations = await GetSpendingDestinationsAsync(model.Destinations, nameof(CreateSpendingTransactionModel.Destinations), errors);
        if (destinations == null)
        {
            return null;
        }
        return new CreateSpendingTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    private async Task<CreateIncomeTransactionRequest?> BuildIncomeCreateRequestAsync(
        AccountingPeriod accountingPeriod,
        CreateIncomeTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        IncomeTransactionSource? source = GetIncomeSource(model.Source, nameof(CreateIncomeTransactionModel.Source), errors);
        if (source == null)
        {
            return null;
        }
        IReadOnlyCollection<IncomeTransactionDestination>? destinations = await GetIncomeDestinationsAsync(model.Destinations, nameof(CreateIncomeTransactionModel.Destinations), errors);
        if (destinations == null)
        {
            return null;
        }
        return new CreateIncomeTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    private async Task<CreateRefundTransactionRequest?> BuildRefundCreateRequestAsync(AccountingPeriod accountingPeriod, CreateRefundTransactionModel model, Dictionary<string, string[]> errors)
    {
        IReadOnlyCollection<RefundTransactionSource>? sources = await GetRefundSourcesAsync(model.Sources.Select(item => (item.AccountId, item.Location, item.Amount, item.FundAssignments)).ToList(), nameof(CreateRefundTransactionModel.Sources), errors);
        Account? account = TryGetAccount(model.Destination.AccountId, $"{nameof(CreateRefundTransactionModel.Destination)}.AccountId", errors);
        return sources == null || account == null ? null : new CreateRefundTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Sources = sources,
            Destination = new RefundTransactionDestination(account, null),
        };
    }

    private async Task<CreateAccountTransactionRequest?> BuildAccountCreateRequestAsync(
        AccountingPeriod accountingPeriod,
        CreateAccountTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        AccountTransactionSource? source = GetAccountSource(model.Source, nameof(CreateAccountTransactionModel.Source), errors);
        if (source == null)
        {
            return null;
        }
        IReadOnlyCollection<AccountTransactionDestination>? destinations = await GetAccountDestinationsAsync(model.Destinations, nameof(CreateAccountTransactionModel.Destinations), errors);
        if (destinations == null)
        {
            return null;
        }
        return new CreateAccountTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    private async Task<CreateFundTransactionRequest?> BuildFundCreateRequestAsync(
        AccountingPeriod accountingPeriod,
        CreateFundTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        FundTransactionSource? source = GetFundSource(model.Source, nameof(CreateFundTransactionModel.Source), errors);
        if (source == null)
        {
            return null;
        }
        IReadOnlyCollection<FundTransactionDestination>? destinations = await GetFundDestinationsAsync(model.Destinations, nameof(CreateFundTransactionModel.Destinations), errors);
        if (destinations == null)
        {
            return null;
        }
        return new CreateFundTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    private async Task<UpdateSpendingTransactionRequest?> BuildSpendingUpdateRequestAsync(UpdateSpendingTransactionModel model, Dictionary<string, string[]> errors)
    {
        SpendingTransactionSource? source = GetSpendingSource(model.Source, nameof(UpdateSpendingTransactionModel.Source), errors);
        if (source == null)
        {
            return null;
        }
        IReadOnlyCollection<SpendingTransactionDestination>? destinations = await GetSpendingDestinationsAsync(model.Destinations, nameof(UpdateSpendingTransactionModel.Destinations), errors);
        if (destinations == null)
        {
            return null;
        }
        return new UpdateSpendingTransactionRequest
        {
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    private async Task<UpdateIncomeTransactionRequest?> BuildIncomeUpdateRequestAsync(UpdateIncomeTransactionModel model, Dictionary<string, string[]> errors)
    {
        IncomeTransactionSource? source = GetIncomeSource(model.Source, nameof(UpdateIncomeTransactionModel.Source), errors);
        if (source == null)
        {
            return null;
        }
        IReadOnlyCollection<IncomeTransactionDestination>? destinations = await GetIncomeDestinationsAsync(model.Destinations, nameof(UpdateIncomeTransactionModel.Destinations), errors);
        if (destinations == null)
        {
            return null;
        }
        return new UpdateIncomeTransactionRequest
        {
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    private async Task<UpdateRefundTransactionRequest?> BuildRefundUpdateRequestAsync(UpdateRefundTransactionModel model, Dictionary<string, string[]> errors)
    {
        IReadOnlyCollection<RefundTransactionSource>? sources = await GetRefundSourcesAsync(model.Sources.Select(item => (item.AccountId, item.Location, item.Amount, item.FundAssignments)).ToList(), nameof(UpdateRefundTransactionModel.Sources), errors);
        Account? account = TryGetAccount(model.Destination.AccountId, $"{nameof(UpdateRefundTransactionModel.Destination)}.AccountId", errors);
        return sources == null || account == null ? null : new UpdateRefundTransactionRequest
        {
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Sources = sources,
            Destination = new RefundTransactionDestination(account, null),
        };
    }

    private async Task<UpdateAccountTransactionRequest?> BuildAccountUpdateRequestAsync(UpdateAccountTransactionModel model, Dictionary<string, string[]> errors)
    {
        AccountTransactionSource? source = GetAccountSource(model.Source, nameof(UpdateAccountTransactionModel.Source), errors);
        if (source == null)
        {
            return null;
        }
        IReadOnlyCollection<AccountTransactionDestination>? destinations = await GetAccountDestinationsAsync(model.Destinations, nameof(UpdateAccountTransactionModel.Destinations), errors);
        if (destinations == null)
        {
            return null;
        }
        return new UpdateAccountTransactionRequest
        {
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    private async Task<UpdateFundTransactionRequest?> BuildFundUpdateRequestAsync(UpdateFundTransactionModel model, Dictionary<string, string[]> errors)
    {
        FundTransactionSource? source = GetFundSource(model.Source, nameof(UpdateFundTransactionModel.Source), errors);
        if (source == null)
        {
            return null;
        }
        IReadOnlyCollection<FundTransactionDestination>? destinations = await GetFundDestinationsAsync(model.Destinations, nameof(UpdateFundTransactionModel.Destinations), errors);
        if (destinations == null)
        {
            return null;
        }
        return new UpdateFundTransactionRequest
        {
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    private Account? TryGetAccount(Guid accountId, string errorKey, Dictionary<string, string[]> errors)
    {
        if (accountRepository.TryGetById(accountId, out Account? account))
        {
            return account;
        }
        errors.Add(errorKey, [$"Account with ID {accountId} was not found."]);
        return null;
    }

    private Fund? TryGetFund(Guid fundId, string errorKey, Dictionary<string, string[]> errors)
    {
        if (fundRepository.TryGetById(fundId, out Fund? fund))
        {
            return fund;
        }
        errors.Add(errorKey, [$"Fund with ID {fundId} was not found."]);
        return null;
    }

    private Task<IReadOnlyCollection<FundAmount>?> GetFundAmountsAsync(
        IReadOnlyCollection<CreateFundAmountModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        List<FundAmount> resolvedFundAmounts = [];

        foreach ((int index, CreateFundAmountModel fundAmountModel) in models.Index())
        {
            Fund? fund = TryGetFund(fundAmountModel.FundId, $"{errorKeyPrefix}[{index}]", errors);
            if (fund == null)
            {
                return Task.FromResult<IReadOnlyCollection<FundAmount>?>(null);
            }

            resolvedFundAmounts.Add(new FundAmount
            {
                FundId = fund.Id,
                Amount = fundAmountModel.Amount,
            });
        }

        return Task.FromResult<IReadOnlyCollection<FundAmount>?>(resolvedFundAmounts);
    }

    private Task<IReadOnlyCollection<IncomeFundAmount>?> GetIncomeFundAmountsAsync(
        IReadOnlyCollection<CreateIncomeFundAmountModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        List<IncomeFundAmount> resolvedFundAmounts = [];

        foreach ((int index, CreateIncomeFundAmountModel fundAmountModel) in models.Index())
        {
            Fund? fund = TryGetFund(fundAmountModel.FundId, $"{errorKeyPrefix}[{index}]", errors);
            if (fund == null)
            {
                return Task.FromResult<IReadOnlyCollection<IncomeFundAmount>?>(null);
            }

            resolvedFundAmounts.Add(new IncomeFundAmount
            {
                FundId = fund.Id,
                Amount = fundAmountModel.Amount,
                IsExtraContribution = fundAmountModel.IsExtraContribution,
            });
        }

        return Task.FromResult<IReadOnlyCollection<IncomeFundAmount>?>(resolvedFundAmounts);
    }

    private SpendingTransactionSource? GetSpendingSource(
        CreateSpendingTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Account? account = TryGetAccount(model.AccountId, $"{errorKeyPrefix}.{nameof(CreateSpendingTransactionSourceModel.AccountId)}", errors);
        if (account == null)
        {
            return null;
        }
        return new SpendingTransactionSource(account, null);
    }

    private SpendingTransactionSource? GetSpendingSource(
        UpdateSpendingTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Account? account = TryGetAccount(model.AccountId, $"{errorKeyPrefix}.{nameof(UpdateSpendingTransactionSourceModel.AccountId)}", errors);
        if (account == null)
        {
            return null;
        }
        return new SpendingTransactionSource(account, null);
    }

    private async Task<IReadOnlyCollection<SpendingTransactionDestination>?> GetSpendingDestinationsAsync(
        IReadOnlyCollection<CreateSpendingTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        List<SpendingTransactionDestination> resolvedDestinations = [];
        foreach ((int index, CreateSpendingTransactionDestinationModel model) in models.Index())
        {
            SpendingTransactionDestination? destination = await GetSpendingDestinationAsync(model.AccountId, model.Location, model.Amount, model.FundAssignments, $"{errorKeyPrefix}[{index}]", errors);
            if (destination == null)
            {
                return null;
            }
            resolvedDestinations.Add(destination);
        }
        return resolvedDestinations;
    }

    private async Task<IReadOnlyCollection<SpendingTransactionDestination>?> GetSpendingDestinationsAsync(
        IReadOnlyCollection<UpdateSpendingTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        List<SpendingTransactionDestination> resolvedDestinations = [];
        foreach ((int index, UpdateSpendingTransactionDestinationModel model) in models.Index())
        {
            SpendingTransactionDestination? destination = await GetSpendingDestinationAsync(model.AccountId, model.Location, model.Amount, model.FundAssignments, $"{errorKeyPrefix}[{index}]", errors);
            if (destination == null)
            {
                return null;
            }
            resolvedDestinations.Add(destination);
        }
        return resolvedDestinations;
    }

    private async Task<SpendingTransactionDestination?> GetSpendingDestinationAsync(
        Guid? accountId,
        LocationInputModel? locationModel,
        decimal amount,
        IReadOnlyCollection<CreateFundAmountModel> fundAssignmentModels,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Account? account = null;
        if (accountId != null && (account = TryGetAccount(accountId.Value, $"{errorKeyPrefix}.AccountId", errors)) == null)
        {
            return null;
        }
        Location? location = GetLocation(locationModel, $"{errorKeyPrefix}.Location", errors);
        if (locationModel != null && location == null)
        {
            return null;
        }
        IReadOnlyCollection<FundAmount>? fundAssignments = await GetFundAmountsAsync(fundAssignmentModels, $"{errorKeyPrefix}.FundAssignments", errors);
        if (fundAssignments == null)
        {
            return null;
        }
        return new SpendingTransactionDestination(account, null, location, amount, fundAssignments.ToList());
    }

    private static bool TryGetIncomeLines(IReadOnlyCollection<CreateIncomeLineModel> models, [NotNullWhen(true)] out IReadOnlyCollection<IncomeLine>? incomeLines)
    {
        incomeLines = models.Select(model => new IncomeLine(model.Description, model.Amount)).ToList();
        return true;
    }

    private static bool TryGetIncomeLines(IReadOnlyCollection<UpdateIncomeLineModel> models, [NotNullWhen(true)] out IReadOnlyCollection<IncomeLine>? incomeLines)
    {
        incomeLines = models.Select(model => new IncomeLine(model.Description, model.Amount)).ToList();
        return true;
    }

    private static bool TryGetIncomeDeductions(IReadOnlyCollection<CreateIncomeDeductionModel> models, [NotNullWhen(true)] out IReadOnlyCollection<IncomeDeduction>? incomeDeductions)
    {
        incomeDeductions = models.Select(model => new IncomeDeduction(model.Description, model.Amount)).ToList();
        return true;
    }

    private static bool TryGetIncomeDeductions(IReadOnlyCollection<UpdateIncomeDeductionModel> models, [NotNullWhen(true)] out IReadOnlyCollection<IncomeDeduction>? incomeDeductions)
    {
        incomeDeductions = models.Select(model => new IncomeDeduction(model.Description, model.Amount)).ToList();
        return true;
    }

    private IncomeTransactionSource? GetIncomeSource(
        CreateIncomeTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Account? account = null;
        if (model.AccountId != null && (account = TryGetAccount(model.AccountId.Value, $"{errorKeyPrefix}.{nameof(CreateIncomeTransactionSourceModel.AccountId)}", errors)) == null)
        {
            return null;
        }
        if (!TryGetIncomeLines(model.IncomeLines, out IReadOnlyCollection<IncomeLine>? incomeLines))
        {
            return null;
        }
        if (!TryGetIncomeDeductions(model.IncomeDeductions, out IReadOnlyCollection<IncomeDeduction>? incomeDeductions))
        {
            return null;
        }
        Location? location = GetLocation(model.Location, $"{errorKeyPrefix}.{nameof(CreateIncomeTransactionSourceModel.Location)}", errors);
        return model.Location != null && location == null
            ? null
            : new IncomeTransactionSource(account, null, location, incomeLines, incomeDeductions);
    }

    private IncomeTransactionSource? GetIncomeSource(
        UpdateIncomeTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Account? account = null;
        if (model.AccountId != null && (account = TryGetAccount(model.AccountId.Value, $"{errorKeyPrefix}.{nameof(UpdateIncomeTransactionSourceModel.AccountId)}", errors)) == null)
        {
            return null;
        }
        if (!TryGetIncomeLines(model.IncomeLines, out IReadOnlyCollection<IncomeLine>? incomeLines))
        {
            return null;
        }
        if (!TryGetIncomeDeductions(model.IncomeDeductions, out IReadOnlyCollection<IncomeDeduction>? incomeDeductions))
        {
            return null;
        }
        Location? location = GetLocation(model.Location, $"{errorKeyPrefix}.{nameof(UpdateIncomeTransactionSourceModel.Location)}", errors);
        return model.Location != null && location == null
            ? null
            : new IncomeTransactionSource(account, null, location, incomeLines, incomeDeductions);
    }

    private async Task<IReadOnlyCollection<IncomeTransactionDestination>?> GetIncomeDestinationsAsync(
        IReadOnlyCollection<CreateIncomeTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        List<IncomeTransactionDestination> resolvedDestinations = [];
        foreach ((int index, CreateIncomeTransactionDestinationModel model) in models.Index())
        {
            IncomeTransactionDestination? destination = await GetIncomeDestinationAsync(model.AccountId, model.Amount, model.FundAssignments, $"{errorKeyPrefix}[{index}]", errors);
            if (destination == null)
            {
                return null;
            }
            resolvedDestinations.Add(destination);
        }
        return resolvedDestinations;
    }

    private async Task<IReadOnlyCollection<IncomeTransactionDestination>?> GetIncomeDestinationsAsync(
        IReadOnlyCollection<UpdateIncomeTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        List<IncomeTransactionDestination> resolvedDestinations = [];
        foreach ((int index, UpdateIncomeTransactionDestinationModel model) in models.Index())
        {
            IncomeTransactionDestination? destination = await GetIncomeDestinationAsync(model.AccountId, model.Amount, model.FundAssignments, $"{errorKeyPrefix}[{index}]", errors);
            if (destination == null)
            {
                return null;
            }
            resolvedDestinations.Add(destination);
        }
        return resolvedDestinations;
    }

    private async Task<IncomeTransactionDestination?> GetIncomeDestinationAsync(
        Guid accountId,
        decimal amount,
        IReadOnlyCollection<CreateIncomeFundAmountModel> fundAssignmentModels,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Account? account = TryGetAccount(accountId, $"{errorKeyPrefix}.AccountId", errors);
        if (account == null)
        {
            return null;
        }
        IReadOnlyCollection<IncomeFundAmount>? fundAssignments = await GetIncomeFundAmountsAsync(fundAssignmentModels, $"{errorKeyPrefix}.FundAssignments", errors);
        if (fundAssignments == null)
        {
            return null;
        }
        return new IncomeTransactionDestination(account, amount, null, fundAssignments);
    }

    private async Task<IReadOnlyCollection<RefundTransactionSource>?> GetRefundSourcesAsync(
        IReadOnlyCollection<(Guid? AccountId, LocationInputModel? Location, decimal Amount, IReadOnlyCollection<CreateFundAmountModel> FundAssignments)> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        List<RefundTransactionSource> sources = [];
        foreach ((int Index, (Guid? AccountId, LocationInputModel? Location, decimal Amount, IReadOnlyCollection<CreateFundAmountModel> FundAssignments) Item) pair in models.Index())
        {
            (Guid? accountId, LocationInputModel? locationModel, decimal amount, IReadOnlyCollection<CreateFundAmountModel> assignments) = pair.Item;
            int index = pair.Index;
            Account? account = accountId == null ? null : TryGetAccount(accountId.Value, $"{errorKeyPrefix}[{index}].AccountId", errors);
            Location? location = GetLocation(locationModel, $"{errorKeyPrefix}[{index}].Location", errors);
            IReadOnlyCollection<FundAmount>? funds = await GetFundAmountsAsync(assignments, $"{errorKeyPrefix}[{index}].FundAssignments", errors);
            if ((accountId != null && account == null) || (locationModel != null && location == null) || funds == null)
            {
                return null;
            }

            sources.Add(new RefundTransactionSource(account, null, location, amount, funds));
        }
        return sources;
    }

    private AccountTransactionSource? GetAccountSource(
        CreateAccountTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Account? account = null;
        if (model.AccountId != null && (account = TryGetAccount(model.AccountId.Value, $"{errorKeyPrefix}.{nameof(CreateAccountTransactionSourceModel.AccountId)}", errors)) == null)
        {
            return null;
        }
        Location? location = GetLocation(model.Location, $"{errorKeyPrefix}.{nameof(CreateAccountTransactionSourceModel.Location)}", errors);
        return model.Location != null && location == null ? null : new AccountTransactionSource(account, null, location);
    }

    private AccountTransactionSource? GetAccountSource(
        UpdateAccountTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Account? account = null;
        if (model.AccountId != null && (account = TryGetAccount(model.AccountId.Value, $"{errorKeyPrefix}.{nameof(UpdateAccountTransactionSourceModel.AccountId)}", errors)) == null)
        {
            return null;
        }
        Location? location = GetLocation(model.Location, $"{errorKeyPrefix}.{nameof(UpdateAccountTransactionSourceModel.Location)}", errors);
        return model.Location != null && location == null ? null : new AccountTransactionSource(account, null, location);
    }

    private async Task<IReadOnlyCollection<AccountTransactionDestination>?> GetAccountDestinationsAsync(
        IReadOnlyCollection<CreateAccountTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        List<AccountTransactionDestination> resolvedDestinations = [];
        foreach ((int index, CreateAccountTransactionDestinationModel model) in models.Index())
        {
            AccountTransactionDestination? destination = await GetAccountDestinationAsync(model.AccountId, model.Location, model.Amount, $"{errorKeyPrefix}[{index}]", errors);
            if (destination == null)
            {
                return null;
            }
            resolvedDestinations.Add(destination);
        }
        return resolvedDestinations;
    }

    private async Task<IReadOnlyCollection<AccountTransactionDestination>?> GetAccountDestinationsAsync(
        IReadOnlyCollection<UpdateAccountTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        List<AccountTransactionDestination> resolvedDestinations = [];
        foreach ((int index, UpdateAccountTransactionDestinationModel model) in models.Index())
        {
            AccountTransactionDestination? destination = await GetAccountDestinationAsync(model.AccountId, model.Location, model.Amount, $"{errorKeyPrefix}[{index}]", errors);
            if (destination == null)
            {
                return null;
            }
            resolvedDestinations.Add(destination);
        }
        return resolvedDestinations;
    }

    private Task<AccountTransactionDestination?> GetAccountDestinationAsync(
        Guid? accountId,
        LocationInputModel? locationModel,
        decimal amount,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Account? account = null;
        if (accountId != null && (account = TryGetAccount(accountId.Value, $"{errorKeyPrefix}.AccountId", errors)) == null)
        {
            return Task.FromResult<AccountTransactionDestination?>(null);
        }
        Location? location = GetLocation(locationModel, $"{errorKeyPrefix}.Location", errors);
        if (locationModel != null && location == null)
        {
            return Task.FromResult<AccountTransactionDestination?>(null);
        }
        return Task.FromResult<AccountTransactionDestination?>(new AccountTransactionDestination(account, null, location, amount));
    }

    private Location? GetLocation(
        LocationInputModel? model,
        string errorKey,
        Dictionary<string, string[]> errors)
    {
        if (model == null)
        {
            return null;
        }
        bool hasLocationId = model.LocationId != null;
        bool hasNewLocationName = !string.IsNullOrWhiteSpace(model.NewLocationName);
        if (hasLocationId == hasNewLocationName)
        {
            errors.Add(errorKey, ["Select an existing Location or explicitly add a new Location"]);
            return null;
        }
        if (model.LocationId != null)
        {
            if (locationRepository.TryGetById(model.LocationId.Value, out Location? existingLocation))
            {
                return existingLocation;
            }
            errors.Add($"{errorKey}.LocationId", [$"Location with ID {model.LocationId.Value} was not found."]);
            return null;
        }
        string normalizedName = LocationService.NormalizeName(model.NewLocationName!);
        if (locationRepository.TryGetByNormalizedName(normalizedName, out Location? matchingLocation))
        {
            return matchingLocation;
        }
        if (locationService.TryCreate(
            new CreateLocationRequest { Name = model.NewLocationName! },
            out Location? newLocation,
            out IEnumerable<Domain.Validation.ValidationError> validationErrors))
        {
            return newLocation;
        }
        errors.Add(errorKey, validationErrors.Select(error => error.Message).ToArray());
        return null;
    }

    private FundTransactionSource? GetFundSource(
        CreateFundTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Fund? fund = TryGetFund(model.FundId, $"{errorKeyPrefix}.{nameof(CreateFundTransactionSourceModel.FundId)}", errors);
        if (fund == null)
        {
            return null;
        }
        return new FundTransactionSource(fund);
    }

    private FundTransactionSource? GetFundSource(
        UpdateFundTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Fund? fund = TryGetFund(model.FundId, $"{errorKeyPrefix}.{nameof(UpdateFundTransactionSourceModel.FundId)}", errors);
        if (fund == null)
        {
            return null;
        }
        return new FundTransactionSource(fund);
    }

    private async Task<IReadOnlyCollection<FundTransactionDestination>?> GetFundDestinationsAsync(
        IReadOnlyCollection<CreateFundTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        List<FundTransactionDestination> resolvedDestinations = [];
        foreach ((int index, CreateFundTransactionDestinationModel model) in models.Index())
        {
            FundTransactionDestination? destination = await GetFundDestinationAsync(model.FundId, model.Amount, $"{errorKeyPrefix}[{index}]", errors);
            if (destination == null)
            {
                return null;
            }
            resolvedDestinations.Add(destination);
        }
        return resolvedDestinations;
    }

    private async Task<IReadOnlyCollection<FundTransactionDestination>?> GetFundDestinationsAsync(
        IReadOnlyCollection<UpdateFundTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        List<FundTransactionDestination> resolvedDestinations = [];
        foreach ((int index, UpdateFundTransactionDestinationModel model) in models.Index())
        {
            FundTransactionDestination? destination = await GetFundDestinationAsync(model.FundId, model.Amount, $"{errorKeyPrefix}[{index}]", errors);
            if (destination == null)
            {
                return null;
            }
            resolvedDestinations.Add(destination);
        }
        return resolvedDestinations;
    }

    private Task<FundTransactionDestination?> GetFundDestinationAsync(
        Guid fundId,
        decimal amount,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Fund? fund = TryGetFund(fundId, $"{errorKeyPrefix}.FundId", errors);
        if (fund == null)
        {
            return Task.FromResult<FundTransactionDestination?>(null);
        }
        return Task.FromResult<FundTransactionDestination?>(new FundTransactionDestination(fund, amount));
    }
}
