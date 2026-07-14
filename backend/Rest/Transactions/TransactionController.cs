using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Transactions;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Read;
using Models.Transactions.Update;
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
    CurrentTransactionsGetter currentTransactionsGetter,
    TransactionTrendsGetter transactionTrendsGetter,
    TransactionGetter transactionGetter,
    TransactionConverter transactionConverter,
    TransactionDispatcherService transactionDispatcherService,
    TransactionRequestConverter transactionRequestConverter) : ControllerBase
{
    /// <summary>
    /// Retrieves the Transactions that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<TransactionModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetMany([FromQuery] TransactionQueryParameterModel queryParameters)
    {
        if (!transactionGetter.TryGet(queryParameters, out CollectionModel<TransactionModel>? transactions, out Dictionary<string, string[]> errors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Transactions.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        return Ok(transactions);
    }

    /// <summary>
    /// Retrieves current snapshot data for Transactions.
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(CurrentTransactionsModel), StatusCodes.Status200OK)]
    public IActionResult GetCurrent([FromQuery] CurrentTransactionsQueryParameterModel queryParameters) =>
        Ok(currentTransactionsGetter.Get(queryParameters));

    /// <summary>
    /// Retrieves trends data for Transactions across a range of Accounting Periods or dates.
    /// </summary>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(TransactionTrendsModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetTrends([FromQuery] TransactionTrendsQueryParameterModel queryParameters)
    {
        if (!transactionTrendsGetter.TryGet(queryParameters, out TransactionTrendsModel? trends, out Dictionary<string, string[]> errors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Transaction trends.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        return Ok(trends);
    }

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
        if (!transactionDispatcherService.TryCreate(createRequest, out Transaction? newTransaction, out IEnumerable<ValidationError> exceptions))
        {
            MergeErrors(errors, GroupValidationErrors(exceptions));
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
        if (!transactionDispatcherService.TryUpdate(transaction, updateRequest, out IEnumerable<ValidationError> exceptions))
        {
            MergeErrors(errors, GroupValidationErrors(exceptions));
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
                new PostTransactionRequest
                {
                    AccountId = account.Id,
                    PostedDate = postTransactionModel.Date,
                },
                out IEnumerable<ValidationError> exceptions))
        {
            errors = GroupValidationErrors(exceptions);
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
        if (!transactionDispatcherService.TryUnpost(transaction, out IEnumerable<ValidationError> exceptions))
        {
            MergeErrors(errors, GroupValidationErrors(exceptions));
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
        if (!transactionDispatcherService.TryDelete(transaction, out IEnumerable<ValidationError> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Transaction.",
                Errors = GroupValidationErrors(exceptions),
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    private static Dictionary<string, string[]> GroupValidationErrors(IEnumerable<ValidationError> errors) =>
        errors
            .GroupBy(error => error.Path.Value switch
            {
                nameof(CreateTransactionRequest.TransactionDate) => nameof(CreateTransactionModel.Date),
                nameof(PostTransactionRequest.PostedDate) => nameof(PostTransactionModel.Date),
                nameof(PostTransactionRequest.AccountId) => nameof(PostTransactionModel.AccountId),
                _ => error.Path.Value
            })
            .ToDictionary(group => group.Key, group => group.Select(error => error.Message).ToArray());

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