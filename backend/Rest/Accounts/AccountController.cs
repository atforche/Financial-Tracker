using Data;
using Domain.AccountingPeriods;
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
    AccountingPeriodConverter accountingPeriodConverter,
    AccountService accountService,
    AccountQueryService accountQueryService,
    FinancialRangeQueryService financialRangeQueryService,
    AccountConverter accountConverter) : ControllerBase
{
    /// <summary>
    /// Retrieves the Account that matches the provided ID
    /// </summary>
    [HttpGet("{accountId}")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<AccountModel> GetAsync(Guid accountId)
    {
        Account? account = accountQueryService.GetById(accountId);
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
        Ok(await financialRangeQueryService.GetAccountsAsync(query, cancellationToken));

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
            ? UnprocessableEntity(new ValidationProblemDetails { Title = "Unable to resolve Accounting Period range.", Status = StatusCodes.Status422UnprocessableEntity })
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
        if (!accountingPeriodConverter.TryToDomain(createAccountModel.OpeningAccountingPeriodId, out AccountingPeriod? accountingPeriod))
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
        Account? accountToUpdate = accountQueryService.GetById(accountId);
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
        Account? accountToDelete = accountQueryService.GetById(accountId);
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