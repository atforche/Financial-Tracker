using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Transactions;
using Microsoft.AspNetCore.Mvc;
using Models.Transactions;
using Rest.AccountingPeriods;
using Rest.Accounts;

namespace Rest.Transactions;

/// <summary>
/// Controller class that exposes endpoints related to Transactions
/// </summary>
[ApiController]
[Route("/transactions")]
public sealed class TransactionController(
    UnitOfWork unitOfWork,
    AccountConverter accountConverter,
    AccountingPeriodConverter accountingPeriodConverter,
    TransactionConverter transactionConverter,
    TransactionDispatcherService transactionDispatcherService,
    TransactionRequestConverter transactionRequestConverter) : ControllerBase
{
    /// <summary>
    /// Retrieves the Transaction with the provided ID
    /// </summary>
    [HttpGet("{transactionId}")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Get(Guid transactionId)
    {
        if (!transactionConverter.TryToDomain(transactionId, out Transaction? transaction))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Transaction.",
                Errors = {
                    { nameof(transactionId), new[] { $"Transaction with ID {transactionId} was not found." } }
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        return Ok(transactionConverter.ToModel(transaction));
    }

    /// <summary>
    /// Creates a new Transaction with the provided properties
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateTransactionModel createTransactionModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountingPeriodConverter.TryToDomain(createTransactionModel.AccountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(createTransactionModel.AccountingPeriodId), [$"Accounting Period with ID {createTransactionModel.AccountingPeriodId} was not found."]);
        }
        if (errors.Count > 0 || accountingPeriod == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!transactionRequestConverter.TryToCreateRequest(accountingPeriod, createTransactionModel, out CreateTransactionRequest? createRequest, out Dictionary<string, string[]> mappingErrors))
        {
            MergeErrors(errors, mappingErrors);
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!transactionDispatcherService.TryCreate(createRequest, out Transaction? newTransaction, out IEnumerable<Exception> exceptions))
        {
            MergeErrors(errors, GroupCreateExceptions(createTransactionModel, exceptions));
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(transactionConverter.ToModel(newTransaction));
    }

    /// <summary>
    /// Updates the provided Transaction with the provided properties
    /// </summary>
    [HttpPost("{transactionId}")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAsync(Guid transactionId, UpdateTransactionModel updateTransactionModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!transactionConverter.TryToDomain(transactionId, out Transaction? transaction))
        {
            errors.Add(nameof(transactionId), [$"Transaction with ID {transactionId} was not found."]);
        }
        if (errors.Count > 0 || transaction == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!transactionRequestConverter.TryToUpdateRequest(transaction, updateTransactionModel, out UpdateTransactionRequest? updateRequest, out Dictionary<string, string[]> mappingErrors))
        {
            MergeErrors(errors, mappingErrors);
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!transactionDispatcherService.TryUpdate(transaction, updateRequest, out IEnumerable<Exception> exceptions))
        {
            MergeErrors(errors, GroupUpdateExceptions(updateTransactionModel, exceptions));
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(transactionConverter.ToModel(transaction));
    }

    /// <summary>
    /// Posts the provided Transaction
    /// </summary>
    [HttpPost("{transactionId}/post")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PostAsync(Guid transactionId, PostTransactionModel postTransactionModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!transactionConverter.TryToDomain(transactionId, out Transaction? transaction))
        {
            errors.Add(nameof(transactionId), [$"Transaction with ID {transactionId} was not found."]);
        }
        if (!accountConverter.TryToDomain(postTransactionModel.AccountId, out Account? account))
        {
            errors.Add(nameof(postTransactionModel.AccountId), new[] { $"Account with ID {postTransactionModel.AccountId} was not found." });
        }
        if (errors.Count > 0 || transaction == null || account == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to post Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!transactionDispatcherService.TryPost(
                transaction,
                account.Id,
                postTransactionModel.Date,
                out IEnumerable<Exception> exceptions))
        {
            errors = exceptions.GroupBy(exception => exception switch
            {
                InvalidDateException => nameof(postTransactionModel.Date),
                InvalidAccountException => nameof(postTransactionModel.AccountId),
                _ => string.Empty
            }).ToDictionary(group => group.Key, group => group.Select(exception => exception.Message).ToArray());
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to post Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(transactionConverter.ToModel(transaction));
    }

    /// <summary>
    /// Unposts the provided Transaction
    /// </summary>
    [HttpPost("{transactionId}/unpost")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UnpostAsync(Guid transactionId)
    {
        Dictionary<string, string[]> errors = [];
        if (!transactionConverter.TryToDomain(transactionId, out Transaction? transaction))
        {
            errors.Add(nameof(transactionId), [$"Transaction with ID {transactionId} was not found."]);
        }
        if (errors.Count > 0 || transaction == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to unpost Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!transactionDispatcherService.TryUnpost(transaction, null, out IEnumerable<Exception> exceptions))
        {
            errors.Add("", exceptions.Select(exception => exception.Message).ToArray());
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to unpost Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(transactionConverter.ToModel(transaction));
    }

    /// <summary>
    /// Deletes the Transaction with the provided ID
    /// </summary>
    [HttpDelete("{transactionId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid transactionId)
    {
        Dictionary<string, string[]> errors = [];
        if (!transactionConverter.TryToDomain(transactionId, out Transaction? transaction))
        {
            errors.Add(nameof(transactionId), [$"Transaction with ID {transactionId} was not found."]);
        }
        if (errors.Count > 0 || transaction == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!transactionDispatcherService.TryDelete(transaction, null, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Transaction.",
                Errors = {
                    { string.Empty, exceptions.Select(e => e.Message).ToArray() }
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    private static Dictionary<string, string[]> GroupCreateExceptions(CreateTransactionModel model, IEnumerable<Exception> exceptions) =>
        GroupExceptions(exceptions, exception => exception switch
        {
            InvalidAccountingPeriodException => nameof(CreateTransactionModel.AccountingPeriodId),
            InvalidDateException => nameof(CreateTransactionModel.Date),
            InvalidAmountException => nameof(CreateTransactionModel.Amount),
            InvalidFundAmountException => model switch
            {
                CreateSpendingTransactionModel => nameof(CreateSpendingTransactionModel.FundAssignments),
                CreateIncomeTransactionModel => nameof(CreateIncomeTransactionModel.FundAssignments),
                _ => string.Empty
            },
            InvalidFundException invalidFundException when invalidFundException.Message.Contains("debit", StringComparison.InvariantCultureIgnoreCase) => nameof(CreateFundTransactionModel.DebitFundId),
            InvalidFundException invalidFundException when invalidFundException.Message.Contains("credit", StringComparison.InvariantCultureIgnoreCase) => nameof(CreateFundTransactionModel.CreditFundId),
            InvalidAccountException invalidAccountException when invalidAccountException.Message.Contains("debit", StringComparison.InvariantCultureIgnoreCase) => "DebitAccount",
            InvalidAccountException invalidAccountException when invalidAccountException.Message.Contains("credit", StringComparison.InvariantCultureIgnoreCase) => "CreditAccount",
            _ => string.Empty
        });

    private static Dictionary<string, string[]> GroupUpdateExceptions(UpdateTransactionModel model, IEnumerable<Exception> exceptions) =>
        GroupExceptions(exceptions, exception => exception switch
        {
            InvalidDateException => nameof(UpdateTransactionModel.Date),
            InvalidAmountException => nameof(UpdateTransactionModel.Amount),
            InvalidFundAmountException => model switch
            {
                UpdateSpendingTransactionModel => nameof(UpdateSpendingTransactionModel.FundAssignments),
                UpdateIncomeTransactionModel => nameof(UpdateIncomeTransactionModel.FundAssignments),
                _ => string.Empty
            },
            InvalidAccountException invalidAccountException when invalidAccountException.Message.Contains("debit", StringComparison.InvariantCultureIgnoreCase) => "DebitAccount",
            InvalidAccountException invalidAccountException when invalidAccountException.Message.Contains("credit", StringComparison.InvariantCultureIgnoreCase) => "CreditAccount",
            _ => string.Empty
        });

    private static Dictionary<string, string[]> GroupExceptions(IEnumerable<Exception> exceptions, Func<Exception, string> keySelector) =>
        exceptions
            .GroupBy(keySelector)
            .ToDictionary(group => group.Key, group => group.Select(exception => exception.Message).ToArray());

    private static void MergeErrors(Dictionary<string, string[]> target, Dictionary<string, string[]> source)
    {
        foreach ((string key, string[] value) in source)
        {
            if (target.TryGetValue(key, out string[]? existing))
            {
                target[key] = existing.Concat(value).ToArray();
            }
            else
            {
                target.Add(key, value);
            }
        }
    }
}