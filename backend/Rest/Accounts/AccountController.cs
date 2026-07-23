using Data;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Accounts;
using Domain.Accounts.Queries;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Accounts;
using Rest.AccountingPeriods;

namespace Rest.Accounts;

/// <summary>
/// Controller class that exposes endpoints related to Accounts
/// </summary>
[ApiController]
[Route("/accounts")]
public sealed class AccountController(
    UnitOfWork unitOfWork,
    AccountingPeriodQueryService accountingPeriodQueryService,
    AccountService accountService,
    AccountQueryService accountQueryService,
    AccountConverter accountConverter,
    AccountBalanceEventQueryService accountBalanceEventQueryService,
    AccountBalanceEventConverter accountBalanceEventConverter) : ControllerBase
{
    /// <summary>
    /// Retrieves balance events for the specified Account.
    /// </summary>
    [HttpGet("{accountId}/balance-events")]
    [ProducesResponseType(typeof(CollectionModel<AccountBalanceEventModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CollectionModel<AccountBalanceEventModel>>> GetBalanceEventsAsync(
        Guid accountId,
        [FromQuery] AccountBalanceEventsQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        if (await accountQueryService.GetByIdAsync(accountId, cancellationToken) == null)
        {
            return NotFound();
        }
        return Ok(accountBalanceEventConverter.ToModel(await accountBalanceEventQueryService.GetAsync(
            accountBalanceEventConverter.ToDomain(accountId, query),
            cancellationToken)));
    }

    /// <summary>
    /// Retrieves Account Balance Events in a date range.
    /// </summary>
    [HttpGet("balance-events/date-range")]
    [ProducesResponseType(typeof(CollectionModel<AccountBalanceEventModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<AccountBalanceEventModel>>> GetBalanceEventsAsync(
        [FromQuery] AccountBalanceEventsInDateRangeQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(accountBalanceEventConverter.ToModel(await accountBalanceEventQueryService.GetAsync(
            accountBalanceEventConverter.ToDomain(query),
            cancellationToken)));

    /// <summary>
    /// Retrieves Account Balance Events in an Accounting Period range.
    /// </summary>
    [HttpGet("balance-events/accounting-period-range")]
    [ProducesResponseType(typeof(CollectionModel<AccountBalanceEventModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CollectionModel<AccountBalanceEventModel>>> GetBalanceEventsAsync(
        [FromQuery] AccountBalanceEventsInAccountingPeriodRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        AccountBalanceEventAccountingPeriodRangeQueryResult result = await accountBalanceEventQueryService.GetAsync(
            accountBalanceEventConverter.ToDomain(query),
            cancellationToken);
        return result.Page == null
            ? UnprocessableEntity(AccountingPeriodRangeValidationProblem.Create(result.Failure, query.Range.Start, query.Range.End, "Unable to retrieve Account balance events."))
            : Ok(accountBalanceEventConverter.ToModel(result.Page));
    }

    /// <summary>
    /// Retrieves the Account that matches the provided ID
    /// </summary>
    [HttpGet("{accountId}")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AccountModel>> GetAsync(Guid accountId, CancellationToken cancellationToken)
    {
        Account? account = await accountQueryService.GetByIdAsync(accountId, cancellationToken);
        return account == null ? NotFound() : Ok(accountConverter.ToModel(account));
    }

    /// <summary>
    /// Gets the Accounts that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<AccountModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionModel<AccountModel>>> GetManyAsync(
        [FromQuery] AccountQueryParameterModel queryParameters,
        CancellationToken cancellationToken) => Ok(accountConverter.ToModel(
            await accountQueryService.GetAsync(accountConverter.ToDomain(queryParameters), cancellationToken)));

    /// <summary>
    /// Retrieves Accounts with current balances.
    /// </summary>
    [HttpGet("with-balances")]
    [ProducesResponseType(typeof(CollectionModel<AccountWithBalanceModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<AccountWithBalanceModel>>> GetWithBalancesAsync(
        [FromQuery] AccountWithBalanceQueryParameterModel query,
        CancellationToken cancellationToken) => Ok(accountConverter.ToModel(
            await accountQueryService.GetWithBalancesAsync(accountConverter.ToDomain(query), cancellationToken)));

    /// <summary>
    /// Retrieves Account balances over a date range.
    /// </summary>
    [HttpGet("date-range")]
    [ProducesResponseType(typeof(AccountsInDateRangeModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountsInDateRangeModel>> GetDateRangeAsync(
        [FromQuery] AccountsInDateRangeQueryParameterModel query,
        CancellationToken cancellationToken) =>
        Ok(accountConverter.ToModel(await accountQueryService.GetDateRangeAsync(
            accountConverter.ToDomain(query),
            cancellationToken)));

    /// <summary>
    /// Retrieves Account balances over an Accounting Period range.
    /// </summary>
    [HttpGet("accounting-period-range")]
    [ProducesResponseType(typeof(AccountsInAccountingPeriodRangeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AccountsInAccountingPeriodRangeModel>> GetAccountingPeriodRangeAsync(
        [FromQuery] AccountsInAccountingPeriodRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        AccountAccountingPeriodRangeQueryResult result = await accountQueryService.GetAccountingPeriodRangeAsync(
            accountConverter.ToDomain(query),
            cancellationToken);
        return result.Range == null
            ? UnprocessableEntity(AccountingPeriodRangeValidationProblem.Create(result.Failure, query.Range.Start, query.Range.End, "Unable to retrieve Account range."))
            : Ok(accountConverter.ToModel(result.Range));
    }

    /// <summary>
    /// Creates a new Account with the provided properties
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateAccountModel createAccountModel)
    {
        Dictionary<string, string[]> errors = [];
        AccountingPeriod? accountingPeriod = await accountingPeriodQueryService.GetByIdAsync(createAccountModel.OpeningAccountingPeriodId);
        if (accountingPeriod == null)
        {
            errors.Add(nameof(createAccountModel.OpeningAccountingPeriodId), [$"Accounting Period with ID {createAccountModel.OpeningAccountingPeriodId} was not found."]);
        }
        if (!AccountTypeConverter.TryToDomain(createAccountModel.Type, out AccountType? accountType))
        {
            errors.Add(nameof(createAccountModel.Type), [$"Unrecognized Account Type: {createAccountModel.Type}"]);
        }
        if (errors.Count > 0 || accountingPeriod == null || accountType == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Account.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }

        if (!accountService.TryCreate(
            new CreateAccountRequest
            {
                Name = createAccountModel.Name,
                Type = accountType.Value,
                OpeningAccountingPeriod = accountingPeriod,
                DateOpened = createAccountModel.DateOpened
            },
            out Account? newAccount,
            out IEnumerable<ValidationError> validationErrors))
        {
            errors = ValidationErrorHelper.GroupValidationErrors(validationErrors, path => path == nameof(CreateAccountRequest.OpeningAccountingPeriod)
                ? nameof(CreateAccountModel.OpeningAccountingPeriodId)
                : path);
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Account.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(accountConverter.ToModel(newAccount));
    }

    /// <summary>
    /// Onboards a new Account with the provided properties
    /// </summary>
    [HttpPost("onboard")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> OnboardAsync(OnboardAccountModel onboardAccountModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!AccountTypeConverter.TryToDomain(onboardAccountModel.Type, out AccountType? accountType))
        {
            errors.Add(nameof(onboardAccountModel.Type), [$"Unrecognized Account Type: {onboardAccountModel.Type}"]);
        }
        if (errors.Count > 0 || accountType == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to onboard Account.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!accountService.TryOnboard(
            new OnboardAccountRequest
            {
                Name = onboardAccountModel.Name,
                Type = accountType.Value,
                OnboardedBalance = onboardAccountModel.OnboardedBalance
            },
            out Account? newAccount,
            out IEnumerable<ValidationError> validationErrors))
        {
            errors = ValidationErrorHelper.GroupValidationErrors(validationErrors);
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to onboard Account.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(accountConverter.ToModel(newAccount));
    }

    /// <summary>
    /// Updates the provided Account with the provided properties
    /// </summary>
    [HttpPost("{accountId}")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateAsync(Guid accountId, UpdateAccountModel updateAccountModel)
    {
        Account? accountToUpdate = await accountQueryService.GetByIdAsync(accountId);
        if (accountToUpdate == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Account.",
                Errors = {
                    { nameof(accountId), [$"Account with ID {accountId} was not found."]}
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!accountService.TryUpdate(
                accountToUpdate,
                new UpdateAccountRequest { Name = updateAccountModel.Name },
                out IEnumerable<ValidationError> validationErrors))
        {
            Dictionary<string, string[]> errors = ValidationErrorHelper.GroupValidationErrors(validationErrors);
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Account.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(accountConverter.ToModel(accountToUpdate));
    }

    /// <summary>
    /// Deletes the Account with the provided ID
    /// </summary>
    [HttpDelete("{accountId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid accountId)
    {
        Account? accountToDelete = await accountQueryService.GetByIdAsync(accountId);
        if (accountToDelete == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Account.",
                Errors = {
                    { nameof(accountId), [$"Account with ID {accountId} was not found."]}
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (!accountService.TryDelete(accountToDelete, out IEnumerable<ValidationError> validationErrors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Account.",
                Errors = ValidationErrorHelper.GroupValidationErrors(validationErrors),
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }
}