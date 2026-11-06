using System.Diagnostics.CodeAnalysis;
using Data;
using Domain.AccountingPeriods;
using Domain.AccountingPeriods.Queries;
using Domain.Transactions.Income;
using Domain.Validation;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.AccountingPeriods;

namespace Rest.AccountingPeriods;

/// <summary>
/// Controller class that exposes endpoints related to Accounting Periods
/// </summary>
[ApiController]
[Route("/accounting-periods")]
public sealed class AccountingPeriodController(UnitOfWork unitOfWork,
    AccountingPeriodConverter accountingPeriodConverter,
    AccountingPeriodQueryConverter accountingPeriodQueryConverter,
    AccountingPeriodQueryService accountingPeriodQueryService,
    PeriodEntityQueryService periodEntityQueryService,
    PeriodEntityConverter periodEntityConverter,
    AccountingPeriodService accountingPeriodService,
    IAccountingPeriodRepository accountingPeriodRepository) : ControllerBase
{
    /// <summary>
    /// Retrieves the Accounting Periods that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<AccountingPeriodModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<AccountingPeriodModel>>> GetManyAsync(
        [FromQuery] AccountingPeriodQueryParameterModel queryParameters,
        CancellationToken cancellationToken) =>
        Ok(accountingPeriodQueryConverter.ToModel(await accountingPeriodQueryService.GetAsync(
            accountingPeriodQueryConverter.ToDomain(queryParameters),
            cancellationToken)));

    /// <summary>
    /// Retrieves everything shown for one Account in an Accounting Period
    /// </summary>
    [HttpGet("{accountingPeriodId:guid}/accounts/{accountId:guid}")]
    [ProducesResponseType(typeof(PeriodAccountDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PeriodAccountDetailModel>> GetAccountDetailAsync(
        Guid accountingPeriodId,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        PeriodAccountDetail? detail = await periodEntityQueryService.GetAccountAsync(accountingPeriodId, accountId, cancellationToken);
        return detail == null ? NotFound() : Ok(periodEntityConverter.ToModel(detail));
    }

    /// <summary>
    /// Retrieves everything shown for one Fund in an Accounting Period
    /// </summary>
    [HttpGet("{accountingPeriodId:guid}/funds/{fundId:guid}")]
    [ProducesResponseType(typeof(PeriodFundDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PeriodFundDetailModel>> GetFundDetailAsync(
        Guid accountingPeriodId,
        Guid fundId,
        CancellationToken cancellationToken)
    {
        PeriodFundDetail? detail = await periodEntityQueryService.GetFundAsync(accountingPeriodId, fundId, cancellationToken);
        return detail == null ? NotFound() : Ok(periodEntityConverter.ToModel(detail));
    }

    /// <summary>
    /// Retrieves the earliest and latest Accounting Periods
    /// </summary>
    [HttpGet("bounds")]
    [ProducesResponseType(typeof(AccountingPeriodBoundsModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountingPeriodBoundsModel>> GetBoundsAsync(CancellationToken cancellationToken) =>
        Ok(accountingPeriodQueryConverter.ToModel(await accountingPeriodQueryService.GetBoundsAsync(cancellationToken)));

    /// <summary>
    /// Retrieves snapshot data for the current Accounting Period.
    /// </summary>
    [HttpGet("with-balances")]
    [ProducesResponseType(typeof(CollectionModel<AccountingPeriodWithBalanceModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CollectionModel<AccountingPeriodWithBalanceModel>>> GetWithBalancesAsync(
        [FromQuery] AccountingPeriodWithBalanceQueryParameterModel queryParameters,
        CancellationToken cancellationToken) =>
        Ok(accountingPeriodQueryConverter.ToModel(await accountingPeriodQueryService.GetWithBalancesAsync(
            accountingPeriodQueryConverter.ToDomain(queryParameters),
            cancellationToken)));

    /// <summary>
    /// Retrieves the Accounting Period with the provided ID.
    /// </summary>
    [HttpGet("{accountingPeriodId:guid}")]
    [ProducesResponseType(typeof(AccountingPeriodWithBalanceModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountingPeriodWithBalanceModel>> GetByIdAsync(
        Guid accountingPeriodId,
        CancellationToken cancellationToken)
    {
        AccountingPeriodBalance? balance = await accountingPeriodQueryService.GetBalanceByIdAsync(accountingPeriodId, cancellationToken);
        return balance == null ? NotFound() : Ok(accountingPeriodQueryConverter.ToModel(balance));
    }

    /// <summary>
    /// Retrieves an Accounting Period with its totals, neighbors, and opening and closing balances.
    /// </summary>
    [HttpGet("{accountingPeriodId:guid}/detail")]
    [ProducesResponseType(typeof(AccountingPeriodDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountingPeriodDetailModel>> GetDetailAsync(
        Guid accountingPeriodId,
        CancellationToken cancellationToken)
    {
        AccountingPeriodDetail? detail = await accountingPeriodQueryService.GetDetailAsync(accountingPeriodId, cancellationToken);
        return detail == null ? NotFound() : Ok(accountingPeriodQueryConverter.ToModel(detail));
    }

    /// <summary>
    /// Retrieves the provided Accounting Periods in date order with their Expected Income Sources. Accounting Periods
    /// that do not exist are left out.
    /// </summary>
    [HttpGet("expected-income-sources")]
    [ProducesResponseType(typeof(IReadOnlyCollection<AccountingPeriodWithExpectedIncomeSourcesModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AccountingPeriodWithExpectedIncomeSourcesModel>>> GetExpectedIncomeSourcesAsync(
        [FromQuery] IReadOnlyCollection<Guid> accountingPeriodIds,
        CancellationToken cancellationToken) =>
        Ok((await accountingPeriodQueryService.GetExpectedIncomeAsync(accountingPeriodIds, cancellationToken))
            .Select(accountingPeriodQueryConverter.ToModel)
            .ToList());

    /// <summary>
    /// Retrieves an Expected Income Source with its Accounting Period and the Income Transactions linked to it.
    /// </summary>
    [HttpGet("{accountingPeriodId:guid}/expected-income-sources/{expectedIncomeSourceId:guid}")]
    [ProducesResponseType(typeof(ExpectedIncomeSourceDetailModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExpectedIncomeSourceDetailModel>> GetExpectedIncomeSourceAsync(
        Guid accountingPeriodId,
        Guid expectedIncomeSourceId,
        CancellationToken cancellationToken)
    {
        ExpectedIncomeSourceDetail? detail = await accountingPeriodQueryService.GetExpectedIncomeSourceAsync(
            accountingPeriodId,
            expectedIncomeSourceId,
            cancellationToken);
        return detail == null ? NotFound() : Ok(accountingPeriodQueryConverter.ToModel(detail));
    }

    /// <summary>
    /// Retrieves Accounting Periods and totals for a contiguous range.
    /// </summary>
    [HttpGet("range")]
    [ProducesResponseType(typeof(AccountingPeriodsInRangeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AccountingPeriodsInRangeModel>> GetRangeAsync(
        [FromQuery] AccountingPeriodsInRangeQueryParameterModel query,
        CancellationToken cancellationToken)
    {
        AccountingPeriodRangeQueryResult<AccountingPeriodRange> result = await accountingPeriodQueryService.GetRangeAsync(
            accountingPeriodQueryConverter.ToDomain(query),
            cancellationToken);
        if (result.Range != null)
        {
            return Ok(accountingPeriodQueryConverter.ToModel(result.Range));
        }
        return UnprocessableEntity(AccountingPeriodRangeValidationProblem.Create(
            result.Failure,
            query.Range.Start,
            query.Range.End,
            "Unable to retrieve Accounting Period range."));
    }

    /// <summary>
    /// Creates a new Accounting Period with the provided properties
    /// </summary>
    /// <param name="createAccountingPeriodModel">Request to create an Accounting Period</param>
    [HttpPost("")]
    [ProducesResponseType(typeof(AccountingPeriodModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateAsync(CreateAccountingPeriodModel createAccountingPeriodModel)
    {
        if (!accountingPeriodService.TryCreate(
                new CreateAccountingPeriodRequest
                {
                    Year = createAccountingPeriodModel.Year,
                    Month = createAccountingPeriodModel.Month,
                },
                out AccountingPeriod? newAccountingPeriod,
                out IEnumerable<ValidationError> validationErrors))
        {
            return Unprocessable("Unable to create Accounting Period.", validationErrors);
        }
        accountingPeriodRepository.Add(newAccountingPeriod);
        await unitOfWork.SaveChangesAsync();
        return Ok(accountingPeriodConverter.ToModel(newAccountingPeriod));
    }

    /// <summary>
    /// Adds an expected income source to an open Accounting Period.
    /// </summary>
    [HttpPost("{accountingPeriodId:guid}/expected-income-sources")]
    [ProducesResponseType(typeof(ExpectedIncomeSourceModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddExpectedIncomeSourceAsync(
        Guid accountingPeriodId,
        [FromBody] ExpectedIncomeSourceRequestModel source)
    {
        if (!accountingPeriodRepository.TryGetById(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            return NotFound();
        }
        if (!AccountingPeriodService.TryAddExpectedIncomeSource(
                accountingPeriod,
                ToRequest(source),
                out ExpectedIncomeSource? expectedIncomeSource,
                out IEnumerable<ValidationError> validationErrors))
        {
            return Unprocessable("Unable to add expected income source.", validationErrors);
        }
        await unitOfWork.SaveChangesAsync();
        // A new source has no income linked to it yet
        return Ok(accountingPeriodConverter.ToModel(expectedIncomeSource, new ReceivedIncome(0, 0)));
    }

    /// <summary>
    /// Updates one expected income source in an open Accounting Period.
    /// </summary>
    [HttpPost("{accountingPeriodId:guid}/expected-income-sources/{expectedIncomeSourceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateExpectedIncomeSourceAsync(
        Guid accountingPeriodId,
        Guid expectedIncomeSourceId,
        [FromBody] ExpectedIncomeSourceRequestModel source)
    {
        if (!TryGetExpectedIncomeSource(accountingPeriodId, expectedIncomeSourceId, out AccountingPeriod? accountingPeriod, out ExpectedIncomeSource? expectedIncomeSource))
        {
            return NotFound();
        }
        if (!AccountingPeriodService.TryUpdateExpectedIncomeSource(
                accountingPeriod,
                expectedIncomeSource,
                ToRequest(source),
                out IEnumerable<ValidationError> validationErrors))
        {
            return Unprocessable("Unable to update expected income source.", validationErrors);
        }
        await unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Deletes one expected income source from an open Accounting Period.
    /// </summary>
    [HttpDelete("{accountingPeriodId:guid}/expected-income-sources/{expectedIncomeSourceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteExpectedIncomeSourceAsync(
        Guid accountingPeriodId,
        Guid expectedIncomeSourceId)
    {
        if (!TryGetExpectedIncomeSource(accountingPeriodId, expectedIncomeSourceId, out AccountingPeriod? accountingPeriod, out ExpectedIncomeSource? expectedIncomeSource))
        {
            return NotFound();
        }
        if (!AccountingPeriodService.TryDeleteExpectedIncomeSource(
                accountingPeriod,
                expectedIncomeSource,
                out IEnumerable<ValidationError> validationErrors))
        {
            return Unprocessable("Unable to delete expected income source.", validationErrors);
        }
        await unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Closes the Accounting Period with the provided ID
    /// </summary>
    [HttpPost("{accountingPeriodId:guid}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> CloseAsync(Guid accountingPeriodId) =>
        ChangeAsync(accountingPeriodId, "Unable to close Accounting Period.", accountingPeriodService.TryClose);

    /// <summary>
    /// Reopens the Accounting Period with the provided ID
    /// </summary>
    [HttpPost("{accountingPeriodId:guid}/reopen")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> ReopenAsync(Guid accountingPeriodId) =>
        ChangeAsync(accountingPeriodId, "Unable to reopen Accounting Period.", accountingPeriodService.TryReopen);

    /// <summary>
    /// Deletes the Accounting Period with the provided ID
    /// </summary>
    /// <param name="accountingPeriodId">ID of the Accounting Period to delete</param>
    [HttpDelete("{accountingPeriodId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public Task<IActionResult> DeleteAsync(Guid accountingPeriodId) =>
        ChangeAsync(accountingPeriodId, "Unable to delete Accounting Period.", accountingPeriodService.TryDelete);

    /// <summary>
    /// A change to an Accounting Period that reports why it could not be made.
    /// </summary>
    private delegate bool AccountingPeriodChange(AccountingPeriod accountingPeriod, out IEnumerable<ValidationError> validationErrors);

    /// <summary>
    /// Applies a change to the Accounting Period with the provided ID and saves it.
    /// </summary>
    private async Task<IActionResult> ChangeAsync(Guid accountingPeriodId, string failureTitle, AccountingPeriodChange change)
    {
        if (!accountingPeriodRepository.TryGetById(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            return NotFound();
        }
        if (!change(accountingPeriod, out IEnumerable<ValidationError> validationErrors))
        {
            return Unprocessable(failureTitle, validationErrors);
        }
        await unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Gets an Accounting Period and one of its expected income sources, or false when either does not exist.
    /// </summary>
    private bool TryGetExpectedIncomeSource(
        Guid accountingPeriodId,
        Guid expectedIncomeSourceId,
        [NotNullWhen(true)] out AccountingPeriod? accountingPeriod,
        [NotNullWhen(true)] out ExpectedIncomeSource? expectedIncomeSource)
    {
        expectedIncomeSource = accountingPeriodRepository.TryGetById(accountingPeriodId, out accountingPeriod)
            ? accountingPeriod.ExpectedIncomeSources.SingleOrDefault(item => item.Id.Value == expectedIncomeSourceId)
            : null;
        return expectedIncomeSource != null && accountingPeriod != null;
    }

    /// <summary>
    /// Builds the response for a change that failed validation.
    /// </summary>
    private static UnprocessableEntityObjectResult Unprocessable(string title, IEnumerable<ValidationError> validationErrors) =>
        new(new ValidationProblemDetails
        {
            Title = title,
            Errors = ValidationErrorHelper.GroupValidationErrors(validationErrors),
            Status = StatusCodes.Status422UnprocessableEntity,
        });

    /// <summary>
    /// Converts expected-income API models to domain requests.
    /// </summary>
    private static ExpectedIncomeSourceRequest ToRequest(ExpectedIncomeSourceRequestModel source) => new()
    {
        Name = source.Name,
        IncomeLines = source.IncomeLines.Select(line => new IncomeLine(line.Description, line.Amount)).ToList(),
        IncomeDeductions = source.IncomeDeductions.Select(deduction => new IncomeDeduction(deduction.Description, deduction.Amount)).ToList(),
        UntrackedTransfers = source.UntrackedTransfers.Select(transfer => new ExpectedUntrackedIncomeTransfer(transfer.Description, transfer.Amount)).ToList(),
        ExpectedDates = source.ExpectedDates,
    };
}
