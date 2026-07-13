using Data;
using Domain.AccountingPeriods;
using Domain.Accounts;
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
    AccountBalanceEventGetter accountBalanceEventGetter,
    AccountTrendsGetter accountTrendsGetter,
    AccountGetter accountGetter,
    AccountSummaryGetter accountSummaryGetter,
    AccountConverter accountConverter) : ControllerBase
{
    /// <summary>
    /// Retrieves the Account that matches the provided ID
    /// </summary>
    [HttpGet("{accountId}")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Get(Guid accountId)
    {
        if (!accountConverter.TryToDomain(accountId, out Account? account))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Account.",
                Errors = { [nameof(accountId)] = new[] { $"Account with ID {accountId} not found." } },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(accountConverter.ToModel(account));
    }

    /// <summary>
    /// Gets the Accounts that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<AccountModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult GetMany([FromQuery] AccountQueryParameterModel queryParameters) =>
        Ok(accountGetter.Get(queryParameters));

    /// <summary>
    /// Retrieves summary balances for Accounts
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(AccountSummaryModel), StatusCodes.Status200OK)]
    public IActionResult GetSummary() => Ok(accountSummaryGetter.Get());

    /// <summary>
    /// Retrieves balance events for a single Account workspace.
    /// </summary>
    [HttpGet("{accountId}/balance-events")]
    [ProducesResponseType(typeof(CollectionModel<AccountWorkspaceBalanceEventModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetBalanceEvents(Guid accountId, [FromQuery] AccountBalanceEventQueryParameterModel queryParameters)
    {
        if (!accountConverter.TryToDomain(accountId, out Account? account))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Account balance events.",
                Errors = { [nameof(accountId)] = [$"Account with ID {accountId} not found."] },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(accountBalanceEventGetter.Get(account, queryParameters));
    }

    /// <summary>
    /// Retrieves trends data for Accounts across a range of Accounting Periods.
    /// </summary>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(AccountTrendsModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetTrends([FromQuery] AccountTrendsQueryParameterModel queryParameters)
    {
        if (!accountTrendsGetter.TryGet(queryParameters, out AccountTrendsModel? trends, out Dictionary<string, string[]> errors))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Account trends.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(trends);
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
            errors = GroupValidationErrors(validationErrors, path => path == nameof(CreateAccountRequest.OpeningAccountingPeriod)
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
            errors = GroupValidationErrors(validationErrors);
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
        if (!accountConverter.TryToDomain(accountId, out Account? accountToUpdate))
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
            Dictionary<string, string[]> errors = GroupValidationErrors(validationErrors);
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
        if (!accountConverter.TryToDomain(accountId, out Account? accountToDelete))
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
                Errors = GroupValidationErrors(validationErrors),
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }

    private static Dictionary<string, string[]> GroupValidationErrors(
        IEnumerable<ValidationError> validationErrors,
        Func<string, string>? resolvePath = null) =>
        validationErrors
            .GroupBy(error => resolvePath?.Invoke(error.Path.Value) ?? error.Path.Value)
            .ToDictionary(grouping => grouping.Key, grouping => grouping.Select(error => error.Message).ToArray());
}