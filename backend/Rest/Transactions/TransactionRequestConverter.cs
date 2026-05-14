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
        if (!TryGetAccount(model.DebitAccount.AccountId, nameof(CreateSpendingTransactionModel.DebitAccount) + "." + nameof(CreateTransactionAccountModel.AccountId), errors, out Account? debitAccount))
        {
            return null;
        }
        Account? creditAccount = null;
        if (model.CreditAccount != null && !TryGetAccount(model.CreditAccount.AccountId, nameof(CreateSpendingTransactionModel.CreditAccount) + "." + nameof(CreateTransactionAccountModel.AccountId), errors, out creditAccount))
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
            Location = model.Location,
            Description = model.Description,
            Amount = model.Amount,
            DebitAccount = debitAccount,
            DebitPostedDate = model.DebitAccount.PostedDate,
            CreditAccount = creditAccount,
            CreditPostedDate = model.CreditAccount?.PostedDate,
            FundAssignments = fundAssignments,
        };
    }

    private CreateIncomeTransactionRequest? BuildIncomeCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateIncomeTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        if (!TryGetAccount(model.CreditAccount.AccountId, nameof(CreateIncomeTransactionModel.CreditAccount) + "." + nameof(CreateTransactionAccountModel.AccountId), errors, out Account? creditAccount))
        {
            return null;
        }
        Account? debitAccount = null;
        if (model.DebitAccount != null && !TryGetAccount(model.DebitAccount.AccountId, nameof(CreateIncomeTransactionModel.DebitAccount) + "." + nameof(CreateTransactionAccountModel.AccountId), errors, out debitAccount))
        {
            return null;
        }
        if (!TryGetFundAmounts(model.FundAssignments, nameof(CreateIncomeTransactionModel.FundAssignments), errors, out IReadOnlyCollection<FundAmount>? fundAssignments))
        {
            return null;
        }
        return new CreateIncomeTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Location = model.Location,
            Description = model.Description,
            Amount = model.Amount,
            CreditAccount = creditAccount,
            CreditPostedDate = model.CreditAccount.PostedDate,
            DebitAccount = debitAccount,
            DebitPostedDate = model.DebitAccount?.PostedDate,
            FundAssignments = fundAssignments,
        };
    }

    private CreateAccountTransactionRequest? BuildAccountCreateRequest(
        AccountingPeriod accountingPeriod,
        CreateAccountTransactionModel model,
        Dictionary<string, string[]> errors)
    {
        Account? debitAccount = null;
        if (model.DebitAccount != null && !TryGetAccount(model.DebitAccount.AccountId, nameof(CreateAccountTransactionModel.DebitAccount) + "." + nameof(CreateTransactionAccountModel.AccountId), errors, out debitAccount))
        {
            return null;
        }
        Account? creditAccount = null;
        if (model.CreditAccount != null && !TryGetAccount(model.CreditAccount.AccountId, nameof(CreateAccountTransactionModel.CreditAccount) + "." + nameof(CreateTransactionAccountModel.AccountId), errors, out creditAccount))
        {
            return null;
        }
        return new CreateAccountTransactionRequest
        {
            AccountingPeriodId = accountingPeriod.Id,
            TransactionDate = model.Date,
            Location = model.Location,
            Description = model.Description,
            Amount = model.Amount,
            DebitAccount = debitAccount,
            DebitPostedDate = model.DebitAccount?.PostedDate,
            CreditAccount = creditAccount,
            CreditPostedDate = model.CreditAccount?.PostedDate,
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
            Location = model.Location,
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
            Location = model.Location,
            Description = model.Description,
            Amount = model.Amount,
            DebitPostedDate = model.DebitAccount?.PostedDate,
            CreditPostedDate = model.CreditAccount?.PostedDate,
            FundAssignments = fundAssignments,
        };
    }

    private UpdateIncomeTransactionRequest? BuildIncomeUpdateRequest(UpdateIncomeTransactionModel model, Dictionary<string, string[]> errors)
    {
        if (!TryGetFundAmounts(model.FundAssignments, nameof(UpdateIncomeTransactionModel.FundAssignments), errors, out IReadOnlyCollection<FundAmount>? fundAssignments))
        {
            return null;
        }
        return new UpdateIncomeTransactionRequest
        {
            TransactionDate = model.Date,
            Location = model.Location,
            Description = model.Description,
            Amount = model.Amount,
            DebitPostedDate = model.DebitAccount?.PostedDate,
            CreditPostedDate = model.CreditAccount?.PostedDate,
            FundAssignments = fundAssignments,
        };
    }

    private static UpdateAccountTransactionRequest BuildAccountUpdateRequest(UpdateAccountTransactionModel model) =>
        new()
        {
            TransactionDate = model.Date,
            Location = model.Location,
            Description = model.Description,
            Amount = model.Amount,
            DebitPostedDate = model.DebitAccount?.PostedDate,
            CreditPostedDate = model.CreditAccount?.PostedDate,
        };

    private static UpdateFundTransactionRequest BuildFundUpdateRequest(UpdateFundTransactionModel model) =>
        new()
        {
            TransactionDate = model.Date,
            Location = model.Location,
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
}