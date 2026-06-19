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
            (AccountTransaction, UpdateAccountTransactionModel account) => BuildAccountUpdateRequest(account),
            (FundTransaction, UpdateFundTransactionModel fund) => BuildFundUpdateRequest(fund),
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
        if (!TryGetAccount(model.DebitAccountId, nameof(CreateSpendingTransactionModel.DebitAccountId), errors, out Account? debitAccount))
        {
            return null;
        }
        Account? creditAccount = null;
        if (model.CreditAccountId != null && !TryGetAccount(model.CreditAccountId.Value, nameof(CreateSpendingTransactionModel.CreditAccountId), errors, out creditAccount))
        {
            return null;
        }
        if (!TryGetFundAmounts(model.FundAssignments, nameof(CreateSpendingTransactionModel.FundAssignments), errors, out IReadOnlyCollection<FundAmount>? fundAssignments))
        {
            return null;
        }
        return new CreateSpendingTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            DebitAccount = debitAccount,
            CreditAccount = creditAccount,
            DestinationLocation = model.DestinationLocation,
            FundAssignments = fundAssignments,
        };
    }

    private CreateIncomeTransactionRequest? BuildIncomeCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateIncomeTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        Account? sourceAccount = null;
        if (model.SourceAccountId != null && !TryGetAccount(model.SourceAccountId.Value, nameof(CreateIncomeTransactionModel.SourceAccountId), errors, out sourceAccount))
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
        if (!TryGetIncomeDestinations(model.IncomeDestinations, nameof(CreateIncomeTransactionModel.IncomeDestinations), errors, out IReadOnlyCollection<IncomeDestination>? incomeDestinations))
        {
            return null;
        }
        return new CreateIncomeTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            SourceAccount = sourceAccount,
            SourceLocation = model.SourceLocation,
            IncomeLines = incomeLines,
            IncomeDeductions = incomeDeductions,
            IncomeDestinations = incomeDestinations,
        };
    }

    private CreateAccountTransactionRequest? BuildAccountCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateAccountTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        Account? debitAccount = null;
        if (model.DebitAccountId != null && !TryGetAccount(model.DebitAccountId.Value, nameof(CreateAccountTransactionModel.DebitAccountId), errors, out debitAccount))
        {
            return null;
        }
        Account? creditAccount = null;
        if (model.CreditAccountId != null && !TryGetAccount(model.CreditAccountId.Value, nameof(CreateAccountTransactionModel.CreditAccountId), errors, out creditAccount))
        {
            return null;
        }
        return new CreateAccountTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            DebitAccount = debitAccount,
            CreditAccount = creditAccount,
            GeneratedByAccountId = null,
        };
    }

    private CreateFundTransactionRequest? BuildFundCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateFundTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        bool hasDebitFund = TryGetFund(model.DebitFundId, nameof(CreateFundTransactionModel.DebitFundId), errors, out Fund? debitFund);
        bool hasCreditFund = TryGetFund(model.CreditFundId, nameof(CreateFundTransactionModel.CreditFundId), errors, out Fund? creditFund);

        if (!hasDebitFund || !hasCreditFund || debitFund == null || creditFund == null)
        {
            return null;
        }
        return new CreateFundTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            DebitFund = debitFund,
            CreditFund = creditFund,
        };
    }

    private UpdateSpendingTransactionRequest? BuildSpendingUpdateRequest(UpdateSpendingTransactionModel model, Dictionary<string, string[]> errors)
    {
        if (!TryGetFundAmounts(model.FundAssignments, nameof(UpdateSpendingTransactionModel.FundAssignments), errors, out IReadOnlyCollection<FundAmount>? fundAssignments))
        {
            return null;
        }
        return new UpdateSpendingTransactionRequest
        {
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            FundAssignments = fundAssignments,
        };
    }

    private UpdateIncomeTransactionRequest? BuildIncomeUpdateRequest(UpdateIncomeTransactionModel model, Dictionary<string, string[]> errors)
    {
        if (!TryGetIncomeLines(model.IncomeLines, out IReadOnlyCollection<IncomeLine>? incomeLines))
        {
            return null;
        }
        if (!TryGetIncomeDeductions(model.IncomeDeductions, out IReadOnlyCollection<IncomeDeduction>? incomeDeductions))
        {
            return null;
        }
        if (!TryGetIncomeDestinations(model.IncomeDestinations, nameof(UpdateIncomeTransactionModel.IncomeDestinations), errors, out IReadOnlyCollection<IncomeDestination>? incomeDestinations))
        {
            return null;
        }
        return new UpdateIncomeTransactionRequest
        {
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
            IncomeLines = incomeLines,
            IncomeDeductions = incomeDeductions,
            IncomeDestinations = incomeDestinations,
        };
    }

    private static UpdateAccountTransactionRequest BuildAccountUpdateRequest(UpdateAccountTransactionModel model) =>
        new()
        {
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
        };

    private static UpdateFundTransactionRequest BuildFundUpdateRequest(UpdateFundTransactionModel model) =>
        new()
        {
            TransactionDate = model.Date,
            Description = model.Description,
            Amount = model.Amount,
        };

    private bool TryGetAccount(
        Guid accountId,
        string errorKey,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out Account? account)
    {
        if (accountConverter.TryToDomain(accountId, out account))
        {
            return true;
        }

        errors.Add(errorKey, [$"Account with ID {accountId} was not found."]);
        return false;
    }

    private bool TryGetFund(
        Guid fundId,
        string errorKey,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out Fund? fund)
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

    private static bool TryGetIncomeLines(
        IReadOnlyCollection<CreateIncomeLineModel> models,
        [NotNullWhen(true)] out IReadOnlyCollection<IncomeLine>? incomeLines)
    {
        incomeLines = models.Select(model => new IncomeLine(model.Description, model.Amount)).ToList();
        return true;
    }

    private static bool TryGetIncomeLines(
        IReadOnlyCollection<UpdateIncomeLineModel> models,
        [NotNullWhen(true)] out IReadOnlyCollection<IncomeLine>? incomeLines)
    {
        incomeLines = models.Select(model => new IncomeLine(model.Description, model.Amount)).ToList();
        return true;
    }

    private static bool TryGetIncomeDeductions(
        IReadOnlyCollection<CreateIncomeDeductionModel> models,
        [NotNullWhen(true)] out IReadOnlyCollection<IncomeDeduction>? incomeDeductions)
    {
        incomeDeductions = models.Select(model => new IncomeDeduction(model.Description, model.Amount)).ToList();
        return true;
    }

    private static bool TryGetIncomeDeductions(
        IReadOnlyCollection<UpdateIncomeDeductionModel> models,
        [NotNullWhen(true)] out IReadOnlyCollection<IncomeDeduction>? incomeDeductions)
    {
        incomeDeductions = models.Select(model => new IncomeDeduction(model.Description, model.Amount)).ToList();
        return true;
    }

    private bool TryGetIncomeDestinations(
        IReadOnlyCollection<CreateIncomeDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out IReadOnlyCollection<IncomeDestination>? incomeDestinations)
    {
        List<IncomeDestination> resolvedDestinations = [];

        foreach ((int index, CreateIncomeDestinationModel model) in models.Index())
        {
            if (!TryGetIncomeDestination(model.AccountId, model.Amount, model.FundAssignments, $"{errorKeyPrefix}[{index}]", errors, out IncomeDestination? destination))
            {
                incomeDestinations = null;
                return false;
            }
            resolvedDestinations.Add(destination);
        }

        incomeDestinations = resolvedDestinations;
        return true;
    }

    private bool TryGetIncomeDestinations(
        IReadOnlyCollection<UpdateIncomeDestinationModel> models,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out IReadOnlyCollection<IncomeDestination>? incomeDestinations)
    {
        List<IncomeDestination> resolvedDestinations = [];

        foreach ((int index, UpdateIncomeDestinationModel model) in models.Index())
        {
            if (!TryGetIncomeDestination(model.AccountId, model.Amount, model.FundAssignments, $"{errorKeyPrefix}[{index}]", errors, out IncomeDestination? destination))
            {
                incomeDestinations = null;
                return false;
            }
            resolvedDestinations.Add(destination);
        }

        incomeDestinations = resolvedDestinations;
        return true;
    }

    private bool TryGetIncomeDestination(
        Guid accountId,
        decimal amount,
        IReadOnlyCollection<CreateFundAmountModel> fundAssignmentModels,
        string errorKeyPrefix,
        Dictionary<string, string[]> errors,
        [NotNullWhen(true)] out IncomeDestination? destination)
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

        destination = new IncomeDestination(account, amount, null, fundAssignments);
        return true;
    }
}
