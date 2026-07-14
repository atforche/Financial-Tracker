using System.Diagnostics.CodeAnalysis;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Funds;
using Domain.Transactions;
using Domain.Transactions.Accounts;
using Domain.Transactions.Funds;
using Domain.Transactions.Income;
using Domain.Transactions.Spending;
using Models.Funds;
using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Update;
using Rest.Accounts;
using Rest.Funds;

namespace Rest.Transactions;

/// <summary>
/// Converter class that handles converting transaction request models to domain transaction requests.
/// </summary>
public sealed class TransactionRequestConverter(
    AccountConverter accountConverter,
    FundConverter fundConverter,
    FundAmountConverter fundAmountConverter)
{
    /// <summary>
    /// Attempts to convert the provided create model to a domain create request.
    /// </summary>
    public bool TryToCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateTransactionModel model,
        [NotNullWhen(true)] out CreateTransactionRequest? request,
        out Dictionary<string, string[]> errors)
    {
        errors = [];
        request = model switch
        {
            CreateSpendingTransactionModel spending => BuildSpendingCreateRequest(accountingPeriod, spending, errors),
            CreateIncomeTransactionModel income => BuildIncomeCreateRequest(accountingPeriod, income, errors),
            CreateAccountTransactionModel account => BuildAccountCreateRequest(accountingPeriod, account, errors),
            CreateFundTransactionModel fund => BuildFundCreateRequest(accountingPeriod, fund, errors),
            _ => null
        };
        if (request != null)
        {
            return true;
        }
        if (errors.Count == 0)
        {
            errors.Add("type", [$"Unsupported transaction request type: {model.GetType().Name}."]);
        }
        return false;
    }

    /// <summary>
    /// Attempts to map the provided update model to a domain update request.
    /// </summary>
    public bool TryToUpdateRequest(
        Transaction transaction,
        UpdateTransactionModel model,
        [NotNullWhen(true)] out UpdateTransactionRequest? request,
        out Dictionary<string, string[]> errors)
    {
        errors = [];

        TransactionTypeModel requestType = TransactionTypeConverter.ToModel(model);
        if (TransactionTypeConverter.ToModel(transaction.Type) != requestType)
        {
            errors.Add("type", [$"Transaction type {requestType} does not match existing transaction type {transaction.Type}."]);
            request = null;
            return false;
        }
        request = (transaction, model) switch
        {
            (SpendingTransaction, UpdateSpendingTransactionModel spending) => BuildSpendingUpdateRequest(spending, errors),
            (IncomeTransaction, UpdateIncomeTransactionModel income) => BuildIncomeUpdateRequest(income, errors),
            (AccountTransaction, UpdateAccountTransactionModel account) => BuildAccountUpdateRequest(account, errors),
            (FundTransaction, UpdateFundTransactionModel fund) => BuildFundUpdateRequest(fund, errors),
            _ => null
        };
        if (request != null)
        {
            return true;
        }
        if (errors.Count == 0)
        {
            errors.Add("type", [$"Request type {model.GetType().Name} is not valid for transaction type {transaction.GetType().Name}."]);
        }
        return false;
    }

    private CreateSpendingTransactionRequest? BuildSpendingCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateSpendingTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        if (!TryGetSpendingSource(model.Source, nameof(CreateSpendingTransactionModel.Source), errors, out SpendingTransactionSource? source))
        {
            return null;
        }
        if (!TryGetSpendingDestinations(model.Destinations, nameof(CreateSpendingTransactionModel.Destinations), errors, out IReadOnlyCollection<SpendingTransactionDestination>? destinations))
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

    private CreateIncomeTransactionRequest? BuildIncomeCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateIncomeTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        if (!TryGetIncomeSource(model.Source, nameof(CreateIncomeTransactionModel.Source), errors, out IncomeTransactionSource? source))
        {
            return null;
        }
        if (!TryGetIncomeDestinations(model.Destinations, nameof(CreateIncomeTransactionModel.Destinations), errors, out IReadOnlyCollection<IncomeTransactionDestination>? destinations))
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

    private CreateAccountTransactionRequest? BuildAccountCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateAccountTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        if (!TryGetAccountSource(model.Source, nameof(CreateAccountTransactionModel.Source), errors, out AccountTransactionSource? source))
        {
            return null;
        }
        if (!TryGetAccountDestinations(model.Destinations, nameof(CreateAccountTransactionModel.Destinations), errors, out IReadOnlyCollection<AccountTransactionDestination>? destinations))
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

    private CreateFundTransactionRequest? BuildFundCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateFundTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        if (!TryGetFundSource(model.Source, nameof(CreateFundTransactionModel.Source), errors, out FundTransactionSource? source))
        {
            return null;
        }
        if (!TryGetFundDestinations(model.Destinations, nameof(CreateFundTransactionModel.Destinations), errors, out IReadOnlyCollection<FundTransactionDestination>? destinations))
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

    private UpdateSpendingTransactionRequest? BuildSpendingUpdateRequest(UpdateSpendingTransactionModel model, Dictionary<string, string[]> errors)
    {
        if (!TryGetSpendingSource(model.Source, nameof(UpdateSpendingTransactionModel.Source), errors, out SpendingTransactionSource? source))
        {
            return null;
        }
        if (!TryGetSpendingDestinations(model.Destinations, nameof(UpdateSpendingTransactionModel.Destinations), errors, out IReadOnlyCollection<SpendingTransactionDestination>? destinations))
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

    private UpdateIncomeTransactionRequest? BuildIncomeUpdateRequest(UpdateIncomeTransactionModel model, Dictionary<string, string[]> errors)
    {
        if (!TryGetIncomeSource(model.Source, nameof(UpdateIncomeTransactionModel.Source), errors, out IncomeTransactionSource? source))
        {
            return null;
        }
        if (!TryGetIncomeDestinations(model.Destinations, nameof(UpdateIncomeTransactionModel.Destinations), errors, out IReadOnlyCollection<IncomeTransactionDestination>? destinations))
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

    private UpdateAccountTransactionRequest? BuildAccountUpdateRequest(UpdateAccountTransactionModel model, Dictionary<string, string[]> errors)
    {
        if (!TryGetAccountSource(model.Source, nameof(UpdateAccountTransactionModel.Source), errors, out AccountTransactionSource? source))
        {
            return null;
        }
        if (!TryGetAccountDestinations(model.Destinations, nameof(UpdateAccountTransactionModel.Destinations), errors, out IReadOnlyCollection<AccountTransactionDestination>? destinations))
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

    private UpdateFundTransactionRequest? BuildFundUpdateRequest(UpdateFundTransactionModel model, Dictionary<string, string[]> errors)
    {
        if (!TryGetFundSource(model.Source, nameof(UpdateFundTransactionModel.Source), errors, out FundTransactionSource? source))
        {
            return null;
        }
        if (!TryGetFundDestinations(model.Destinations, nameof(UpdateFundTransactionModel.Destinations), errors, out IReadOnlyCollection<FundTransactionDestination>? destinations))
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

    private bool TryGetAccount(Guid accountId, string errorKey, Dictionary<string, string[]> errors, [NotNullWhen(true)] out Account? account)
    {
        if (accountConverter.TryToDomain(accountId, out account))
        {
            return true;
        }

        errors.Add(errorKey, [$"Account with ID {accountId} was not found."]);
        return false;
    }

    private bool TryGetFund(Guid fundId, string errorKey, Dictionary<string, string[]> errors, [NotNullWhen(true)] out Fund? fund)
    {
        if (fundConverter.TryToDomain(fundId, out fund))
        {
            return true;
        }

        errors.Add(errorKey, [$"Fund with ID {fundId} was not found."]);
        return false;
    }

    private bool TryGetFundAmounts(
        IReadOnlyCollection<CreateFundAmountModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out IReadOnlyCollection<FundAmount>? fundAmounts)
    {
        List<FundAmount> resolvedFundAmounts = [];

        foreach ((int index, CreateFundAmountModel fundAmountModel) in models.Index())
        {
            if (!fundAmountConverter.TryToDomain(fundAmountModel, out FundAmount? fundAmount))
            {
                errors.Add($"{errorKeyPrefix}[{index}]", [$"Fund with ID {fundAmountModel.FundId} was not found."]);
                fundAmounts = null;
                return false;
            }

            resolvedFundAmounts.Add(fundAmount);
        }

        fundAmounts = resolvedFundAmounts;
        return true;
    }

    private bool TryGetSpendingSource(
        CreateSpendingTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out SpendingTransactionSource? source)
    {
        source = null;
        if (!TryGetAccount(model.AccountId, $"{errorKeyPrefix}.{nameof(CreateSpendingTransactionSourceModel.AccountId)}", errors, out Account? account))
        {
            return false;
        }
        source = new SpendingTransactionSource(account, null);
        return true;
    }

    private bool TryGetSpendingSource(
        UpdateSpendingTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out SpendingTransactionSource? source)
    {
        source = null;
        if (!TryGetAccount(model.AccountId, $"{errorKeyPrefix}.{nameof(UpdateSpendingTransactionSourceModel.AccountId)}", errors, out Account? account))
        {
            return false;
        }
        source = new SpendingTransactionSource(account, null);
        return true;
    }

    private bool TryGetSpendingDestinations(
        IReadOnlyCollection<CreateSpendingTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out IReadOnlyCollection<SpendingTransactionDestination>? destinations)
    {
        List<SpendingTransactionDestination> resolvedDestinations = [];
        foreach ((int index, CreateSpendingTransactionDestinationModel model) in models.Index())
        {
            if (!TryGetSpendingDestination(model.AccountId, model.Location, model.Amount, model.FundAssignments, $"{errorKeyPrefix}[{index}]", errors, out SpendingTransactionDestination? destination))
            {
                destinations = null;
                return false;
            }
            resolvedDestinations.Add(destination);
        }
        destinations = resolvedDestinations;
        return true;
    }

    private bool TryGetSpendingDestinations(
        IReadOnlyCollection<UpdateSpendingTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out IReadOnlyCollection<SpendingTransactionDestination>? destinations)
    {
        List<SpendingTransactionDestination> resolvedDestinations = [];
        foreach ((int index, UpdateSpendingTransactionDestinationModel model) in models.Index())
        {
            if (!TryGetSpendingDestination(model.AccountId, model.Location, model.Amount, model.FundAssignments, $"{errorKeyPrefix}[{index}]", errors, out SpendingTransactionDestination? destination))
            {
                destinations = null;
                return false;
            }
            resolvedDestinations.Add(destination);
        }
        destinations = resolvedDestinations;
        return true;
    }

    private bool TryGetSpendingDestination(
        Guid? accountId,
        string? location,
        decimal amount,
        IReadOnlyCollection<CreateFundAmountModel> fundAssignmentModels,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out SpendingTransactionDestination? destination)
    {
        destination = null;
        Account? account = null;
        if (accountId != null && !TryGetAccount(accountId.Value, $"{errorKeyPrefix}.AccountId", errors, out account))
        {
            return false;
        }
        if (!TryGetFundAmounts(fundAssignmentModels, $"{errorKeyPrefix}.FundAssignments", errors, out IReadOnlyCollection<FundAmount>? fundAssignments))
        {
            return false;
        }
        destination = new SpendingTransactionDestination(account, null, location, amount, fundAssignments.ToList());
        return true;
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

    private bool TryGetIncomeSource(
        CreateIncomeTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out IncomeTransactionSource? source)
    {
        source = null;
        Account? account = null;
        if (model.AccountId != null && !TryGetAccount(model.AccountId.Value, $"{errorKeyPrefix}.{nameof(CreateIncomeTransactionSourceModel.AccountId)}", errors, out account))
        {
            return false;
        }
        if (!TryGetIncomeLines(model.IncomeLines, out IReadOnlyCollection<IncomeLine>? incomeLines))
        {
            return false;
        }
        if (!TryGetIncomeDeductions(model.IncomeDeductions, out IReadOnlyCollection<IncomeDeduction>? incomeDeductions))
        {
            return false;
        }
        source = new IncomeTransactionSource(account, null, model.Location, incomeLines, incomeDeductions);
        return true;
    }

    private bool TryGetIncomeSource(
        UpdateIncomeTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out IncomeTransactionSource? source)
    {
        source = null;
        Account? account = null;
        if (model.AccountId != null && !TryGetAccount(model.AccountId.Value, $"{errorKeyPrefix}.{nameof(UpdateIncomeTransactionSourceModel.AccountId)}", errors, out account))
        {
            return false;
        }
        if (!TryGetIncomeLines(model.IncomeLines, out IReadOnlyCollection<IncomeLine>? incomeLines))
        {
            return false;
        }
        if (!TryGetIncomeDeductions(model.IncomeDeductions, out IReadOnlyCollection<IncomeDeduction>? incomeDeductions))
        {
            return false;
        }
        source = new IncomeTransactionSource(account, null, model.Location, incomeLines, incomeDeductions);
        return true;
    }

    private bool TryGetIncomeDestinations(
        IReadOnlyCollection<CreateIncomeTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out IReadOnlyCollection<IncomeTransactionDestination>? destinations)
    {
        List<IncomeTransactionDestination> resolvedDestinations = [];
        foreach ((int index, CreateIncomeTransactionDestinationModel model) in models.Index())
        {
            if (!TryGetIncomeDestination(model.AccountId, model.Amount, model.FundAssignments, $"{errorKeyPrefix}[{index}]", errors, out IncomeTransactionDestination? destination))
            {
                destinations = null;
                return false;
            }
            resolvedDestinations.Add(destination);
        }
        destinations = resolvedDestinations;
        return true;
    }

    private bool TryGetIncomeDestinations(
        IReadOnlyCollection<UpdateIncomeTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out IReadOnlyCollection<IncomeTransactionDestination>? destinations)
    {
        List<IncomeTransactionDestination> resolvedDestinations = [];
        foreach ((int index, UpdateIncomeTransactionDestinationModel model) in models.Index())
        {
            if (!TryGetIncomeDestination(model.AccountId, model.Amount, model.FundAssignments, $"{errorKeyPrefix}[{index}]", errors, out IncomeTransactionDestination? destination))
            {
                destinations = null;
                return false;
            }
            resolvedDestinations.Add(destination);
        }
        destinations = resolvedDestinations;
        return true;
    }

    private bool TryGetIncomeDestination(
        Guid accountId,
        decimal amount,
        IReadOnlyCollection<CreateFundAmountModel> fundAssignmentModels,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out IncomeTransactionDestination? destination)
    {
        destination = null;
        if (!TryGetAccount(accountId, $"{errorKeyPrefix}.AccountId", errors, out Account? account))
        {
            return false;
        }
        if (!TryGetFundAmounts(fundAssignmentModels, $"{errorKeyPrefix}.FundAssignments", errors, out IReadOnlyCollection<FundAmount>? fundAssignments))
        {
            return false;
        }
        destination = new IncomeTransactionDestination(account, amount, null, fundAssignments);
        return true;
    }

    private bool TryGetAccountSource(
        CreateAccountTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out AccountTransactionSource? source)
    {
        source = null;
        Account? account = null;
        if (model.AccountId != null && !TryGetAccount(model.AccountId.Value, $"{errorKeyPrefix}.{nameof(CreateAccountTransactionSourceModel.AccountId)}", errors, out account))
        {
            return false;
        }
        source = new AccountTransactionSource(account, null, model.Location);
        return true;
    }

    private bool TryGetAccountSource(
        UpdateAccountTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out AccountTransactionSource? source)
    {
        source = null;
        Account? account = null;
        if (model.AccountId != null && !TryGetAccount(model.AccountId.Value, $"{errorKeyPrefix}.{nameof(UpdateAccountTransactionSourceModel.AccountId)}", errors, out account))
        {
            return false;
        }
        source = new AccountTransactionSource(account, null, model.Location);
        return true;
    }

    private bool TryGetAccountDestinations(
        IReadOnlyCollection<CreateAccountTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out IReadOnlyCollection<AccountTransactionDestination>? destinations)
    {
        List<AccountTransactionDestination> resolvedDestinations = [];
        foreach ((int index, CreateAccountTransactionDestinationModel model) in models.Index())
        {
            if (!TryGetAccountDestination(model.AccountId, model.Location, model.Amount, $"{errorKeyPrefix}[{index}]", errors, out AccountTransactionDestination? destination))
            {
                destinations = null;
                return false;
            }
            resolvedDestinations.Add(destination);
        }
        destinations = resolvedDestinations;
        return true;
    }

    private bool TryGetAccountDestinations(
        IReadOnlyCollection<UpdateAccountTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out IReadOnlyCollection<AccountTransactionDestination>? destinations)
    {
        List<AccountTransactionDestination> resolvedDestinations = [];
        foreach ((int index, UpdateAccountTransactionDestinationModel model) in models.Index())
        {
            if (!TryGetAccountDestination(model.AccountId, model.Location, model.Amount, $"{errorKeyPrefix}[{index}]", errors, out AccountTransactionDestination? destination))
            {
                destinations = null;
                return false;
            }
            resolvedDestinations.Add(destination);
        }
        destinations = resolvedDestinations;
        return true;
    }

    private bool TryGetAccountDestination(
        Guid? accountId,
        string? location,
        decimal amount,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out AccountTransactionDestination? destination)
    {
        destination = null;
        Account? account = null;
        if (accountId != null && !TryGetAccount(accountId.Value, $"{errorKeyPrefix}.AccountId", errors, out account))
        {
            return false;
        }
        destination = new AccountTransactionDestination(account, null, location, amount);
        return true;
    }

    private bool TryGetFundSource(
        CreateFundTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out FundTransactionSource? source)
    {
        source = null;
        if (!TryGetFund(model.FundId, $"{errorKeyPrefix}.{nameof(CreateFundTransactionSourceModel.FundId)}", errors, out Fund? fund))
        {
            return false;
        }
        source = new FundTransactionSource(fund);
        return true;
    }

    private bool TryGetFundSource(
        UpdateFundTransactionSourceModel model,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out FundTransactionSource? source)
    {
        source = null;
        if (!TryGetFund(model.FundId, $"{errorKeyPrefix}.{nameof(UpdateFundTransactionSourceModel.FundId)}", errors, out Fund? fund))
        {
            return false;
        }
        source = new FundTransactionSource(fund);
        return true;
    }

    private bool TryGetFundDestinations(
        IReadOnlyCollection<CreateFundTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out IReadOnlyCollection<FundTransactionDestination>? destinations)
    {
        List<FundTransactionDestination> resolvedDestinations = [];
        foreach ((int index, CreateFundTransactionDestinationModel model) in models.Index())
        {
            if (!TryGetFundDestination(model.FundId, model.Amount, $"{errorKeyPrefix}[{index}]", errors, out FundTransactionDestination? destination))
            {
                destinations = null;
                return false;
            }
            resolvedDestinations.Add(destination);
        }
        destinations = resolvedDestinations;
        return true;
    }

    private bool TryGetFundDestinations(
        IReadOnlyCollection<UpdateFundTransactionDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out IReadOnlyCollection<FundTransactionDestination>? destinations)
    {
        List<FundTransactionDestination> resolvedDestinations = [];
        foreach ((int index, UpdateFundTransactionDestinationModel model) in models.Index())
        {
            if (!TryGetFundDestination(model.FundId, model.Amount, $"{errorKeyPrefix}[{index}]", errors, out FundTransactionDestination? destination))
            {
                destinations = null;
                return false;
            }
            resolvedDestinations.Add(destination);
        }
        destinations = resolvedDestinations;
        return true;
    }

    private bool TryGetFundDestination(
        Guid fundId,
        decimal amount,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out FundTransactionDestination? destination)
    {
        destination = null;
        if (!TryGetFund(fundId, $"{errorKeyPrefix}.FundId", errors, out Fund? fund))
        {
            return false;
        }
        destination = new FundTransactionDestination(fund, amount);
        return true;
    }
}