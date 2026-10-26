using Data;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Accounts;
using Domain.Transactions;
using Domain.Transactions.Queries;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Models;
using Models.Transactions;
using Models.Transactions.Create;
using Models.Transactions.Types;
using Models.Transactions.Update;
using Rest.AccountingPeriods;

namespace Rest.Transactions;

/// <summary>
/// Controller class that exposes endpoints related to Transactions
/// </summary>
[ApiController]
[Route("/transactions")]
public sealed class TransactionController(
    UnitOfWork unitOfWork,
    IAccountRepository accountRepository,
    IAccountingPeriodRepository accountingPeriodRepository,
    ITransactionRepository transactionRepository,
    TransactionQueryService transactionQueryService,
    TransactionDispatcherService transactionDispatcherService,
    TransactionRequestConverter transactionRequestConverter,
    TransactionConverter transactionConverter) : ControllerBase
{
    /// <summary>
    /// Retrieves Transactions matching the specified criteria.
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<TransactionModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<TransactionModel>>> GetManyAsync(
        [FromQuery] TransactionQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(transactionConverter.ToModel(await transactionQueryService.GetAsync(
            transactionConverter.ToDomain(query),
            cancellationToken)));

    /// <summary>
    /// Retrieves a Transaction by ID.
    /// </summary>
    [HttpGet("{transactionId:guid}")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionModel>> GetAsync(Guid transactionId, CancellationToken cancellationToken)
    {
        TransactionModel? model = await GetModelAsync(transactionId, cancellationToken);
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
        Ok(transactionConverter.ToModel(await transactionQueryService.GetDateRangeAsync(
            transactionConverter.ToDomain(query),
            cancellationToken)));

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
        AccountingPeriodRangeQueryResult<TransactionAccountingPeriodRange> result =
            await transactionQueryService.GetAccountingPeriodRangeAsync(
                transactionConverter.ToDomain(query),
                cancellationToken);
        return result.Range == null
            ? UnprocessableEntity(AccountingPeriodRangeValidationProblem.Create(result.Failure, query.Range.Start, query.Range.End, "Unable to retrieve Transaction range."))
            : Ok(transactionConverter.ToModel(result.Range));
    }

    /// <summary>
    /// Creates a new Transaction with the provided properties
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(typeof(CreateTransactionResultModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateTransactionModel createTransactionModel)
    {
        TransactionChangeResult result = await TryCreateAsync(createTransactionModel, "Unable to create Transaction.");
        if (result.Problem != null)
        {
            return new UnprocessableEntityObjectResult(result.Problem);
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(new CreateTransactionResultModel { Id = result.Transaction.Id.Value });
    }

    /// <summary>
    /// Previews the Transaction that would be created with the provided properties, without saving it.
    /// Its balance events are reported as if every Account were posted on the Transaction's date.
    /// </summary>
    [HttpPost("preview")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PreviewCreateAsync(
        CreateTransactionModel createTransactionModel,
        CancellationToken cancellationToken) =>
        await PreviewAsync(
            () => TryCreateAsync(createTransactionModel, "Unable to preview Transaction."),
            postOnTransactionDate: true,
            cancellationToken);

    /// <summary>
    /// Updates the provided Transaction with the provided properties
    /// </summary>
    [HttpPost("{transactionId}")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAsync(Guid transactionId, UpdateTransactionModel updateTransactionModel)
    {
        TransactionChangeResult result = await TryUpdateAsync(transactionId, updateTransactionModel, "Unable to update Transaction.");
        if (result.Problem != null)
        {
            return new UnprocessableEntityObjectResult(result.Problem);
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(await GetModelAsync(transactionId));
    }

    /// <summary>
    /// Previews the provided Transaction as it would be after the update, without saving it. Its balance
    /// events are reported as if every Account were posted on the Transaction's date.
    /// </summary>
    [HttpPost("{transactionId}/preview")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PreviewUpdateAsync(
        Guid transactionId,
        UpdateTransactionModel updateTransactionModel,
        CancellationToken cancellationToken) =>
        await PreviewAsync(
            () => TryUpdateAsync(transactionId, updateTransactionModel, "Unable to preview Transaction."),
            postOnTransactionDate: true,
            cancellationToken);

    /// <summary>
    /// Posts the provided Transaction
    /// </summary>
    [HttpPost("{transactionId}/post")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PostAsync(Guid transactionId, PostTransactionModel postTransactionModel)
    {
        TransactionChangeResult result = TryPost(transactionId, postTransactionModel, "Unable to post Transaction.");
        if (result.Problem != null)
        {
            return new UnprocessableEntityObjectResult(result.Problem);
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(await GetModelAsync(transactionId));
    }

    /// <summary>
    /// Previews the provided Transaction as it would be immediately after posting it, including its
    /// balance events, without saving the posting
    /// </summary>
    [HttpPost("{transactionId}/post/preview")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PreviewPostAsync(
        Guid transactionId,
        PostTransactionModel postTransactionModel,
        CancellationToken cancellationToken) =>
        await PreviewAsync(
            () => Task.FromResult(TryPost(transactionId, postTransactionModel, "Unable to preview posting Transaction.")),
            postOnTransactionDate: false,
            cancellationToken);

    /// <summary>
    /// Unposts the provided Transaction
    /// </summary>
    [HttpPost("{transactionId}/unpost")]
    [ProducesResponseType(typeof(TransactionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UnpostAsync(Guid transactionId)
    {
        const string title = "Unable to unpost Transaction.";
        if (!transactionRepository.TryGetById(transactionId, out Transaction? transaction))
        {
            return new UnprocessableEntityObjectResult(TransactionChangeResult.NotFound(title, transactionId).Problem);
        }
        if (!transactionDispatcherService.TryUnpost(transaction, out IEnumerable<ValidationError> exceptions))
        {
            return new UnprocessableEntityObjectResult(TransactionChangeResult.Failure(
                title,
                ValidationErrorHelper.GroupValidationErrors(exceptions, ResolveValidationErrorPath)).Problem);
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(await GetModelAsync(transactionId));
    }

    /// <summary>
    /// Deletes the Transaction with the provided ID
    /// </summary>
    [HttpDelete("{transactionId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid transactionId)
    {
        const string title = "Unable to delete Transaction.";
        if (!transactionRepository.TryGetById(transactionId, out Transaction? transaction))
        {
            return new UnprocessableEntityObjectResult(TransactionChangeResult.NotFound(title, transactionId).Problem);
        }
        if (!transactionDispatcherService.TryDelete(transaction, out IEnumerable<ValidationError> exceptions))
        {
            return new UnprocessableEntityObjectResult(TransactionChangeResult.Failure(
                title,
                ValidationErrorHelper.GroupValidationErrors(exceptions, ResolveValidationErrorPath)).Problem);
        }
        await unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Retrieves and converts interpreted Transaction details.
    /// </summary>
    private async Task<TransactionModel?> GetModelAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        TransactionDetails? details = await transactionQueryService.GetDetailsByIdAsync(
            transactionId,
            cancellationToken);
        return details == null ? null : transactionConverter.ToModel(details);
    }

    /// <summary>
    /// Maps and validates a create request, applying it to the tracked data without saving.
    /// </summary>
    private async Task<TransactionChangeResult> TryCreateAsync(CreateTransactionModel createTransactionModel, string title)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountingPeriodRepository.TryGetById(createTransactionModel.AccountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(createTransactionModel.AccountingPeriodId), [$"Accounting Period with ID {createTransactionModel.AccountingPeriodId} was not found."]);
        }
        if (errors.Count > 0 || accountingPeriod == null)
        {
            return TransactionChangeResult.Failure(title, errors);
        }
        Dictionary<string, string[]> mappingErrors = [];
        CreateTransactionRequest? createRequest = transactionRequestConverter.ToCreateRequest(accountingPeriod, createTransactionModel, mappingErrors);
        if (createRequest == null)
        {
            MergeErrors(errors, mappingErrors);
            return TransactionChangeResult.Failure(title, errors);
        }
        if (!transactionDispatcherService.TryCreate(createRequest, out Transaction? newTransaction, out IEnumerable<ValidationError> exceptions))
        {
            MergeErrors(errors, ValidationErrorHelper.GroupValidationErrors(exceptions, ResolveValidationErrorPath));
            return TransactionChangeResult.Failure(title, errors);
        }
        return TransactionChangeResult.Success(newTransaction);
    }

    /// <summary>
    /// Maps and validates an update request, applying it to the tracked data without saving.
    /// </summary>
    private async Task<TransactionChangeResult> TryUpdateAsync(Guid transactionId, UpdateTransactionModel updateTransactionModel, string title)
    {
        Dictionary<string, string[]> errors = [];
        if (!transactionRepository.TryGetById(transactionId, out Transaction? transaction))
        {
            errors.Add(nameof(transactionId), [$"Transaction with ID {transactionId} was not found."]);
        }
        if (errors.Count > 0 || transaction == null)
        {
            return TransactionChangeResult.Failure(title, errors);
        }
        Dictionary<string, string[]> mappingErrors = [];
        UpdateTransactionRequest? updateRequest = transactionRequestConverter.ToUpdateRequest(transaction, updateTransactionModel, mappingErrors);
        if (updateRequest == null)
        {
            MergeErrors(errors, mappingErrors);
            return TransactionChangeResult.Failure(title, errors);
        }
        if (!transactionDispatcherService.TryUpdate(transaction, updateRequest, out IEnumerable<ValidationError> exceptions))
        {
            MergeErrors(errors, ValidationErrorHelper.GroupValidationErrors(exceptions, ResolveValidationErrorPath));
            return TransactionChangeResult.Failure(title, errors);
        }
        return TransactionChangeResult.Success(transaction);
    }

    /// <summary>
    /// Posts each unposted Account of the Transaction on the Transaction's date, saving after each posting so
    /// the next one builds on the updated balance histories. Accounts that cannot be posted stay unposted.
    /// </summary>
    private async Task PostUnpostedAccountsOnTransactionDateAsync(Transaction transaction)
    {
        foreach (AccountId accountId in transaction.GetAllAffectedAccountIds().Distinct()
            .Where(accountId => transaction.GetPostedDateForAccount(accountId) == null)
            .ToList())
        {
            if (transactionDispatcherService.TryPost(
                    transaction,
                    new PostTransactionRequest { AccountId = accountId, PostedDate = transaction.Date },
                    out _))
            {
                await unitOfWork.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Maps and validates a post request, applying it to the tracked data without saving.
    /// </summary>
    private TransactionChangeResult TryPost(Guid transactionId, PostTransactionModel postTransactionModel, string title)
    {
        Dictionary<string, string[]> errors = [];
        if (!transactionRepository.TryGetById(transactionId, out Transaction? transaction))
        {
            errors.Add(nameof(transactionId), [$"Transaction with ID {transactionId} was not found."]);
        }
        if (!accountRepository.TryGetById(postTransactionModel.AccountId, out Account? account))
        {
            errors.Add(nameof(postTransactionModel.AccountId), [$"Account with ID {postTransactionModel.AccountId} was not found."]);
        }
        if (errors.Count > 0 || transaction == null || account == null)
        {
            return TransactionChangeResult.Failure(title, errors);
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
            return TransactionChangeResult.Failure(title, ValidationErrorHelper.GroupValidationErrors(exceptions, ResolveValidationErrorPath));
        }
        return TransactionChangeResult.Success(transaction);
    }

    /// <summary>
    /// Applies a change and returns the resulting Transaction without keeping the change. Saving rewrites the
    /// balance histories, so the change is saved inside a database transaction, read back through this
    /// request's context, and then rolled back. Unposted Transactions have no balances, so when requested,
    /// each unposted Account is first posted on the Transaction's date.
    /// </summary>
    private async Task<IActionResult> PreviewAsync(
        Func<Task<TransactionChangeResult>> applyChange,
        bool postOnTransactionDate,
        CancellationToken cancellationToken)
    {
        await using IDbContextTransaction databaseTransaction = await unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);
        TransactionChangeResult result = await applyChange();
        if (result.Problem != null)
        {
            return new UnprocessableEntityObjectResult(result.Problem);
        }
        await unitOfWork.SaveChangesAsync();
        if (postOnTransactionDate)
        {
            await PostUnpostedAccountsOnTransactionDateAsync(result.Transaction);
        }
        TransactionDetails details = await transactionQueryService.GetDetailsByIdInCurrentContextAsync(
                result.Transaction.Id.Value,
                cancellationToken)
            ?? throw new InvalidOperationException("The previewed Transaction could not be read back.");
        await databaseTransaction.RollbackAsync(cancellationToken);
        return Ok(transactionConverter.ToModel(details));
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

    /// <summary>
    /// Outcome of applying a create or update request: the changed Transaction, or the problem preventing it.
    /// </summary>
    private sealed record TransactionChangeResult(Transaction? ChangedTransaction, ValidationProblemDetails? Problem)
    {
        /// <summary>
        /// Gets the changed Transaction. Only valid when there is no problem.
        /// </summary>
        public Transaction Transaction => ChangedTransaction ?? throw new InvalidOperationException("The Transaction change failed.");

        public static TransactionChangeResult Success(Transaction transaction) => new(transaction, null);

        public static TransactionChangeResult NotFound(string title, Guid transactionId) => Failure(
            title,
            new Dictionary<string, string[]> { [nameof(transactionId)] = [$"Transaction with ID {transactionId} was not found."] });

        public static TransactionChangeResult Failure(string title, Dictionary<string, string[]> errors) => new(null, new ValidationProblemDetails
        {
            Title = title,
            Errors = errors,
            Status = StatusCodes.Status422UnprocessableEntity
        });
    }
}
