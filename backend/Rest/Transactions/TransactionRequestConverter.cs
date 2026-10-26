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
    public CreateTransactionRequest? ToCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        CreateTransactionRequest? request = model switch
        {
            CreateSpendingTransactionModel spending => BuildSpendingCreateRequest(accountingPeriod, spending, errors),
            CreateIncomeTransactionModel income => BuildIncomeCreateRequest(accountingPeriod, income, errors),
            CreateAccountTransactionModel account => BuildAccountCreateRequest(accountingPeriod, account, errors),
            CreateFundTransactionModel fund => BuildFundCreateRequest(accountingPeriod, fund, errors),
            CreateRefundTransactionModel refund => BuildRefundCreateRequest(accountingPeriod, refund, errors),
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
    public UpdateTransactionRequest? ToUpdateRequest(
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
            (SpendingTransaction, UpdateSpendingTransactionModel spending) => BuildSpendingUpdateRequest(spending, errors),
            (IncomeTransaction, UpdateIncomeTransactionModel income) => BuildIncomeUpdateRequest(income, errors),
            (AccountTransaction, UpdateAccountTransactionModel account) => BuildAccountUpdateRequest(account, errors),
            (FundTransaction, UpdateFundTransactionModel fund) => BuildFundUpdateRequest(fund, errors),
            (RefundTransaction, UpdateRefundTransactionModel refund) => BuildRefundUpdateRequest(refund, errors),
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

    private CreateSpendingTransactionRequest? BuildSpendingCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateSpendingTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        SpendingTransactionSource? source = GetSpendingSource(model.Source, nameof(CreateSpendingTransactionModel.Source), errors);
        IReadOnlyCollection<SpendingTransactionDestination>? destinations = source == null ? null : GetAll(model.Destinations, nameof(CreateSpendingTransactionModel.Destinations), errors, GetSpendingDestination);
        return source == null || destinations == null ? null : new CreateSpendingTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    private CreateIncomeTransactionRequest? BuildIncomeCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateIncomeTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        IncomeTransactionSource? source = GetIncomeSource(model.Source, nameof(CreateIncomeTransactionModel.Source), errors);
        IReadOnlyCollection<IncomeTransactionDestination>? destinations = source == null ? null : GetAll(model.Destinations, nameof(CreateIncomeTransactionModel.Destinations), errors, GetIncomeDestination);
        return source == null || destinations == null ? null : new CreateIncomeTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    private CreateRefundTransactionRequest? BuildRefundCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateRefundTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        IReadOnlyCollection<RefundTransactionSource>? sources = GetAll(model.Sources, nameof(CreateRefundTransactionModel.Sources), errors, GetRefundSource);
        RefundTransactionDestination? destination = GetRefundDestination(model.Destination, nameof(CreateRefundTransactionModel.Destination), errors);
        return sources == null || destination == null ? null : new CreateRefundTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Sources = sources,
            Destination = destination,
        };
    }

    private CreateAccountTransactionRequest? BuildAccountCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateAccountTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        AccountTransactionSource? source = GetAccountSource(model.Source, nameof(CreateAccountTransactionModel.Source), errors);
        IReadOnlyCollection<AccountTransactionDestination>? destinations = source == null ? null : GetAll(model.Destinations, nameof(CreateAccountTransactionModel.Destinations), errors, GetAccountDestination);
        return source == null || destinations == null ? null : new CreateAccountTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    private CreateFundTransactionRequest? BuildFundCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateFundTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        FundTransactionSource? source = GetFundSource(model.Source, nameof(CreateFundTransactionModel.Source), errors);
        IReadOnlyCollection<FundTransactionDestination>? destinations = source == null ? null : GetAll(model.Destinations, nameof(CreateFundTransactionModel.Destinations), errors, GetFundDestination);
        return source == null || destinations == null ? null : new CreateFundTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    private UpdateSpendingTransactionRequest? BuildSpendingUpdateRequest(UpdateSpendingTransactionModel model, Dictionary<string, string[]> errors)
    {
        SpendingTransactionSource? source = GetSpendingSource(model.Source, nameof(UpdateSpendingTransactionModel.Source), errors);
        IReadOnlyCollection<SpendingTransactionDestination>? destinations = source == null ? null : GetAll(model.Destinations, nameof(UpdateSpendingTransactionModel.Destinations), errors, GetSpendingDestination);
        return source == null || destinations == null ? null : new UpdateSpendingTransactionRequest
        {
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    private UpdateIncomeTransactionRequest? BuildIncomeUpdateRequest(UpdateIncomeTransactionModel model, Dictionary<string, string[]> errors)
    {
        IncomeTransactionSource? source = GetIncomeSource(model.Source, nameof(UpdateIncomeTransactionModel.Source), errors);
        IReadOnlyCollection<IncomeTransactionDestination>? destinations = source == null ? null : GetAll(model.Destinations, nameof(UpdateIncomeTransactionModel.Destinations), errors, GetIncomeDestination);
        return source == null || destinations == null ? null : new UpdateIncomeTransactionRequest
        {
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    private UpdateRefundTransactionRequest? BuildRefundUpdateRequest(UpdateRefundTransactionModel model, Dictionary<string, string[]> errors)
    {
        IReadOnlyCollection<RefundTransactionSource>? sources = GetAll(model.Sources, nameof(UpdateRefundTransactionModel.Sources), errors, GetRefundSource);
        RefundTransactionDestination? destination = GetRefundDestination(model.Destination, nameof(UpdateRefundTransactionModel.Destination), errors);
        return sources == null || destination == null ? null : new UpdateRefundTransactionRequest
        {
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Sources = sources,
            Destination = destination,
        };
    }

    private UpdateAccountTransactionRequest? BuildAccountUpdateRequest(UpdateAccountTransactionModel model, Dictionary<string, string[]> errors)
    {
        AccountTransactionSource? source = GetAccountSource(model.Source, nameof(UpdateAccountTransactionModel.Source), errors);
        IReadOnlyCollection<AccountTransactionDestination>? destinations = source == null ? null : GetAll(model.Destinations, nameof(UpdateAccountTransactionModel.Destinations), errors, GetAccountDestination);
        return source == null || destinations == null ? null : new UpdateAccountTransactionRequest
        {
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    private UpdateFundTransactionRequest? BuildFundUpdateRequest(UpdateFundTransactionModel model, Dictionary<string, string[]> errors)
    {
        FundTransactionSource? source = GetFundSource(model.Source, nameof(UpdateFundTransactionModel.Source), errors);
        IReadOnlyCollection<FundTransactionDestination>? destinations = source == null ? null : GetAll(model.Destinations, nameof(UpdateFundTransactionModel.Destinations), errors, GetFundDestination);
        return source == null || destinations == null ? null : new UpdateFundTransactionRequest
        {
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            Source = source,
            Destinations = destinations,
        };
    }

    /// <summary>
    /// Converts each model in a collection, stopping at the first one that fails.
    /// </summary>
    private static List<TResult>? GetAll<TModel, TResult>(
        IReadOnlyCollection<TModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        Func<TModel, string, Dictionary<string, string[]>, TResult?> convert)
        where TResult : class
    {
        List<TResult> results = [];
        foreach ((int index, TModel model) in models.Index())
        {
            TResult? result = convert(model, $"{errorKeyPrefix}[{index}]", errors);
            if (result == null)
            {
                return null;
            }
            results.Add(result);
        }
        return results;
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

    /// <summary>
    /// Resolves an optional account, returning false only when an account ID was provided but not found.
    /// </summary>
    private bool TryGetOptionalAccount(Guid? accountId, string errorKey, Dictionary<string, string[]> errors, out Account? account)
    {
        account = accountId == null ? null : TryGetAccount(accountId.Value, errorKey, errors);
        return accountId == null || account != null;
    }

    /// <summary>
    /// Resolves an optional location, returning false only when a location was provided but could not be resolved.
    /// </summary>
    private bool TryGetOptionalLocation(LocationInputModel? model, string errorKey, Dictionary<string, string[]> errors, out Location? location)
    {
        location = GetLocation(model, errorKey, errors);
        return model == null || location != null;
    }

    private FundAmount? GetFundAmount(CreateFundAmountModel model, string errorKey, Dictionary<string, string[]> errors)
    {
        Fund? fund = TryGetFund(model.FundId, errorKey, errors);
        return fund == null ? null : new FundAmount
        {
            FundId = fund.Id,
            Amount = model.Amount,
        };
    }

    private IncomeFundAmount? GetIncomeFundAmount(CreateIncomeFundAmountModel model, string errorKey, Dictionary<string, string[]> errors)
    {
        Fund? fund = TryGetFund(model.FundId, errorKey, errors);
        return fund == null ? null : new IncomeFundAmount
        {
            FundId = fund.Id,
            Amount = model.Amount,
            IsExtraContribution = model.IsExtraContribution,
        };
    }

    private SpendingTransactionSource? GetSpendingSource(
        CreateSpendingTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Account? account = TryGetAccount(model.AccountId, $"{errorKeyPrefix}.{nameof(CreateSpendingTransactionSourceModel.AccountId)}", errors);
        return account == null ? null : new SpendingTransactionSource(account, null);
    }

    private SpendingTransactionDestination? GetSpendingDestination(
        CreateSpendingTransactionDestinationModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        if (!TryGetOptionalAccount(model.AccountId, $"{errorKeyPrefix}.AccountId", errors, out Account? account) ||
            !TryGetOptionalLocation(model.Location, $"{errorKeyPrefix}.Location", errors, out Location? location))
        {
            return null;
        }
        List<FundAmount>? fundAssignments = GetAll(model.FundAssignments, $"{errorKeyPrefix}.FundAssignments", errors, GetFundAmount);
        return fundAssignments == null ? null : new SpendingTransactionDestination(account, null, location, model.Amount, fundAssignments);
    }

    private IncomeTransactionSource? GetIncomeSource(
        CreateIncomeTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        if (!TryGetOptionalAccount(model.AccountId, $"{errorKeyPrefix}.{nameof(CreateIncomeTransactionSourceModel.AccountId)}", errors, out Account? account) ||
            !TryGetOptionalLocation(model.Location, $"{errorKeyPrefix}.{nameof(CreateIncomeTransactionSourceModel.Location)}", errors, out Location? location))
        {
            return null;
        }
        return new IncomeTransactionSource(
            account,
            null,
            location,
            model.IncomeLines.Select(line => new IncomeLine(line.Description, line.Amount)).ToList(),
            model.IncomeDeductions.Select(deduction => new IncomeDeduction(deduction.Description, deduction.Amount)).ToList());
    }

    private IncomeTransactionDestination? GetIncomeDestination(
        CreateIncomeTransactionDestinationModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Account? account = TryGetAccount(model.AccountId, $"{errorKeyPrefix}.AccountId", errors);
        if (account == null)
        {
            return null;
        }
        IReadOnlyCollection<IncomeFundAmount>? fundAssignments = GetAll(model.FundAssignments, $"{errorKeyPrefix}.FundAssignments", errors, GetIncomeFundAmount);
        return fundAssignments == null ? null : new IncomeTransactionDestination(account, model.Amount, null, fundAssignments);
    }

    private RefundTransactionSource? GetRefundSource(
        CreateRefundTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        bool hasAccount = TryGetOptionalAccount(model.AccountId, $"{errorKeyPrefix}.AccountId", errors, out Account? account);
        bool hasLocation = TryGetOptionalLocation(model.Location, $"{errorKeyPrefix}.Location", errors, out Location? location);
        IReadOnlyCollection<FundAmount>? fundAssignments = GetAll(model.FundAssignments, $"{errorKeyPrefix}.FundAssignments", errors, GetFundAmount);
        return !hasAccount || !hasLocation || fundAssignments == null
            ? null
            : new RefundTransactionSource(account, null, location, model.Amount, fundAssignments);
    }

    private RefundTransactionDestination? GetRefundDestination(
        CreateRefundTransactionDestinationModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Account? account = TryGetAccount(model.AccountId, $"{errorKeyPrefix}.AccountId", errors);
        return account == null ? null : new RefundTransactionDestination(account, null);
    }

    private AccountTransactionSource? GetAccountSource(
        CreateAccountTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        if (!TryGetOptionalAccount(model.AccountId, $"{errorKeyPrefix}.{nameof(CreateAccountTransactionSourceModel.AccountId)}", errors, out Account? account) ||
            !TryGetOptionalLocation(model.Location, $"{errorKeyPrefix}.{nameof(CreateAccountTransactionSourceModel.Location)}", errors, out Location? location))
        {
            return null;
        }
        return new AccountTransactionSource(account, null, location);
    }

    private AccountTransactionDestination? GetAccountDestination(
        CreateAccountTransactionDestinationModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        if (!TryGetOptionalAccount(model.AccountId, $"{errorKeyPrefix}.AccountId", errors, out Account? account) ||
            !TryGetOptionalLocation(model.Location, $"{errorKeyPrefix}.Location", errors, out Location? location))
        {
            return null;
        }
        return new AccountTransactionDestination(account, null, location, model.Amount);
    }

    private FundTransactionSource? GetFundSource(
        CreateFundTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Fund? fund = TryGetFund(model.FundId, $"{errorKeyPrefix}.{nameof(CreateFundTransactionSourceModel.FundId)}", errors);
        return fund == null ? null : new FundTransactionSource(fund);
    }

    private FundTransactionDestination? GetFundDestination(
        CreateFundTransactionDestinationModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors)
    {
        Fund? fund = TryGetFund(model.FundId, $"{errorKeyPrefix}.FundId", errors);
        return fund == null ? null : new FundTransactionDestination(fund, model.Amount);
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
}
