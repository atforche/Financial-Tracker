using Data;
using Data.Transactions;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Transactions;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Types;
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
    TransactionRepository transactionRepository,
    TransactionQueryService transactionQueryService,
    TransactionDispatcherService transactionDispatcherService,
    TransactionRequestConverter transactionRequestConverter) : ControllerBase
{
    /// <summary>
    /// Retrieves Transactions matching the specified criteria.
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<TransactionModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<TransactionModel>>> GetManyAsync(
        [FromQuery] TransactionQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(await transactionQueryService.GetAsync(query, cancellationToken));

    /// <summary>
    /// Retrieves a Transaction by ID.
    /// </summary>
    [HttpGet("{transactionId:guid}")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionModel>> GetAsync(Guid transactionId, CancellationToken cancellationToken)
    {
        TransactionModel? model = await transactionQueryService.GetByIdAsync(transactionId, cancellationToken);
        return model == null ? NotFound() : Ok(model);
    }

    /// <summary>
    /// Retrieves Transactions in a date range.
    /// </summary>
    [HttpGet("date-range")]
    [ProducesResponseType(typeof(TransactionsInDateRangeModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<TransactionsInDateRangeModel>> GetDateRangeAsync(
        [FromQuery] TransactionsInDateRangeQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(await transactionQueryService.GetInDateRangeAsync(query, cancellationToken));

    /// <summary>
    /// Retrieves Transactions in an Accounting Period range.
    /// </summary>
    [HttpGet("accounting-period-range")]
    [ProducesResponseType(typeof(TransactionsInAccountingPeriodRangeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TransactionsInAccountingPeriodRangeModel>> GetAccountingPeriodRangeAsync(
        [FromQuery] TransactionsInAccountingPeriodRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        TransactionsInAccountingPeriodRangeModel? model = await transactionQueryService.GetInAccountingPeriodRangeAsync(query, cancellationToken);
        return model == null
            ? UnprocessableEntity(new ValidationProblemDetails { Title = "Unable to resolve Accounting Period range.", Status = StatusCodes.Status422UnprocessableEntity })
            : Ok(model);
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
            MergeErrors(errors, ValidationErrorHelper.GroupValidationErrors(exceptions, ResolveValidationErrorPath));
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(await transactionQueryService.GetByIdAsync(newTransaction.Id.Value));
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
        if (!transactionRepository.TryGetById(transactionId, out Transaction? transaction))
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
            MergeErrors(errors, ValidationErrorHelper.GroupValidationErrors(exceptions, ResolveValidationErrorPath));
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(await transactionQueryService.GetByIdAsync(transactionId));
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
        if (!transactionRepository.TryGetById(transactionId, out Transaction? transaction))
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
            errors = ValidationErrorHelper.GroupValidationErrors(exceptions, ResolveValidationErrorPath);
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to post Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(await transactionQueryService.GetByIdAsync(transactionId));
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
        if (!transactionRepository.TryGetById(transactionId, out Transaction? transaction))
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
            MergeErrors(errors, ValidationErrorHelper.GroupValidationErrors(exceptions, ResolveValidationErrorPath));
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to unpost Transaction.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(await transactionQueryService.GetByIdAsync(transactionId));
    }

    /// <summary>
    /// Deletes the Transaction with the provided ID
    /// </summary>
    [HttpDelete("{transactionId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid transactionId)
    {
        Dictionary<string, string[]> errors = [];
        if (!transactionRepository.TryGetById(transactionId, out Transaction? transaction))
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
                Errors = ValidationErrorHelper.GroupValidationErrors(exceptions, ResolveValidationErrorPath),
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    private static string ResolveValidationErrorPath(string path) => path switch
    {
        nameof(CreateTransactionRequest.TransactionDate) => nameof(CreateTransactionModel.Date),
        nameof(PostTransactionRequest.PostedDate) => nameof(PostTransactionModel.Date),
        nameof(PostTransactionRequest.AccountId) => nameof(PostTransactionModel.AccountId),
        _ => path
    };

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