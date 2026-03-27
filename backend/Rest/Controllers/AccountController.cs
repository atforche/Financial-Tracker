using Data;
using Data.AccountingPeriods;
using Data.Accounts;
using Data.Transactions;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Exceptions;
using Domain.Funds;
using Domain.Transactions;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Accounts;
using Models.Funds;
using Models.Transactions;
using Rest.Mappers;

namespace Rest.Controllers;

/// <summary>
/// Controller class that exposes endpoints related to Accounts
/// </summary>
[ApiController]
[Route("/accounts")]
public sealed class AccountController(
    UnitOfWork unitOfWork,
    AccountingPeriodBalanceHistoryRepository accountingPeriodBalanceHistoryRepository,
    AccountRepository accountRepository,
    TransactionRepository transactionRepository,
    AccountService accountService,
    AccountingPeriodMapper accountingPeriodMapper,
    AccountingPeriodAccountMapper accountingPeriodAccountMapper,
    AccountMapper accountMapper,
    FundAmountMapper fundAmountMapper,
    TransactionMapper transactionMapper) : ControllerBase
{
    /// <summary>
    /// Retrieves the Account that matches the provided ID
    /// </summary>
    [HttpGet("{accountId}")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Get(Guid accountId)
    {
        if (!accountMapper.TryToDomain(accountId, out Account? account))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Account.",
                Errors = { [nameof(accountId)] = new[] { $"Account with ID {accountId} not found." } },
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }
        return Ok(accountMapper.ToModel(account));
    }

    /// <summary>
    /// Gets the Accounts that match the specified criteria
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(CollectionModel<AccountModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult GetMany([FromQuery] AccountQueryParameterModel queryParameters)
    {
        Dictionary<string, string[]> errors = [];

        AccountSortOrder? accountSortOrder = null;
        if (queryParameters.Sort != null && !AccountSortOrderMapper.TryToData(queryParameters.Sort.Value, out accountSortOrder))
        {
            errors.Add(nameof(queryParameters.Sort), [$"Unrecognized Sort Order: {queryParameters.Sort.Value}"]);
        }
        if (errors.Count > 0)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounts.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity,
            });
        }

        List<AccountModel> results = [];
        PaginatedCollection<Account> paginatedResults = accountRepository.GetMany(new GetAccountsRequest
        {
            Search = queryParameters.Search,
            Sort = accountSortOrder,
            Limit = queryParameters.Limit,
            Offset = queryParameters.Offset,
        });
        return Ok(new CollectionModel<AccountModel>
        {
            Items = paginatedResults.Items.Select(accountMapper.ToModel).ToList(),
            TotalCount = paginatedResults.TotalCount
        });
    }

    /// <summary>
    /// Retrieves the Account as it appeared in the provided Accounting Period
    /// </summary>
    [HttpGet("{accountId}/{accountingPeriodId}")]
    [ProducesResponseType(typeof(AccountingPeriodAccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetAccountingPeriodAccount(Guid accountId, Guid accountingPeriodId)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountMapper.TryToDomain(accountId, out Account? account))
        {
            errors.Add(nameof(accountId), new[] { $"Account with ID {accountId} not found." });
        }
        if (!accountingPeriodMapper.TryToDomain(accountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(accountingPeriodId), new[] { $"Accounting Period with ID {accountingPeriodId} not found." });
        }
        if (errors.Count > 0 || account == null || accountingPeriod == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounting Period Fund.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        AccountingPeriodBalanceHistory? accountingPeriodBalanceHistory = accountingPeriodBalanceHistoryRepository.GetForAccountingPeriod(accountingPeriod.Id);
        AccountAccountingPeriodBalanceHistory? accountBalanceHistory = accountingPeriodBalanceHistory?.AccountBalances
            .FirstOrDefault(accountHistory => accountHistory.Account.Id == account.Id);
        if (accountBalanceHistory == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Accounting Period Fund.",
                Errors = new Dictionary<string, string[]>
                {
                    { nameof(accountId), new[] { $"Account with ID {accountId} not found for Accounting Period with ID {accountingPeriodId}." } }
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        return Ok(accountingPeriodAccountMapper.ToModel(accountBalanceHistory));
    }

    /// <summary>
    /// Retrieves the Transactions for the Account that matches the provided ID
    /// </summary>
    [HttpGet("{accountId}/transactions")]
    [ProducesResponseType(typeof(CollectionModel<TransactionModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetTransactions(Guid accountId, [FromQuery] AccountTransactionQueryParameterModel queryParameters)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountMapper.TryToDomain(accountId, out Account? account))
        {
            errors.Add(nameof(accountId), [$"Account with ID {accountId} was not found."]);
        }
        AccountTransactionSortOrder? accountTransactionSortOrder = null;
        if (queryParameters.Sort != null && !AccountTransactionSortOrderMapper.TryToData(queryParameters.Sort.Value, out accountTransactionSortOrder))
        {
            errors.Add(nameof(queryParameters.Sort), [$"Unrecognized Sort Order: {queryParameters.Sort.Value}"]);
        }
        if (errors.Count > 0 || account == null)
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to retrieve Account Transactions.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }

        PaginatedCollection<Transaction> paginatedResults = transactionRepository.GetManyByAccount(account, new GetAccountTransactionsRequest
        {
            Search = queryParameters.Search,
            Sort = accountTransactionSortOrder,
            Limit = queryParameters.Limit,
            Offset = queryParameters.Offset
        });
        return Ok(new CollectionModel<TransactionModel>
        {
            Items = paginatedResults.Items.Select(transactionMapper.ToModel).ToList(),
            TotalCount = paginatedResults.TotalCount
        });
    }

    /// <summary>
    /// Creates a new Account with the provided properties
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAsync(CreateAccountModel createAccountModel)
    {
        Dictionary<string, string[]> errors = [];
        if (!accountingPeriodMapper.TryToDomain(createAccountModel.AccountingPeriodId, out AccountingPeriod? accountingPeriod))
        {
            errors.Add(nameof(createAccountModel.AccountingPeriodId), [$"Accounting Period with ID {createAccountModel.AccountingPeriodId} was not found."]);
        }
        if (!AccountTypeMapper.TryToDomain(createAccountModel.Type, out AccountType? accountType))
        {
            errors.Add(nameof(createAccountModel.Type), [$"Unrecognized Account Type: {createAccountModel.Type}"]);
        }
        List<FundAmount> fundAmounts = [];
        foreach ((int index, CreateFundAmountModel fundAmountModel) in createAccountModel.InitialFundAmounts.Index())
        {
            if (!fundAmountMapper.TryToDomain(fundAmountModel, out FundAmount? fundAmount))
            {
                errors.Add($"{nameof(createAccountModel.InitialFundAmounts)}[{index}]", [$"Fund with ID {fundAmountModel.FundId} was not found."]);
            }
            else
            {
                fundAmounts.Add(fundAmount);
            }
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
                AccountingPeriod = accountingPeriod,
                AddDate = createAccountModel.AddDate,
                InitialFundAmounts = fundAmounts
            },
            out Account? newAccount,
            out Transaction? initialTransaction,
            out IEnumerable<Exception> exceptions))
        {
            errors = exceptions.GroupBy(e => e switch
            {
                InvalidNameException => nameof(createAccountModel.Name),
                InvalidAccountingPeriodException => nameof(createAccountModel.AccountingPeriodId),
                InvalidDateException => nameof(createAccountModel.AddDate),
                _ => string.Empty
            }).ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to create Account.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        if (initialTransaction != null)
        {
            transactionRepository.Add(initialTransaction);
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(accountMapper.ToModel(newAccount));
    }

    /// <summary>
    /// Updates the provided Account with the provided properties
    /// </summary>
    [HttpPost("{accountId}")]
    [ProducesResponseType(typeof(AccountModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAsync(Guid accountId, UpdateAccountModel updateAccountModel)
    {
        if (!accountMapper.TryToDomain(accountId, out Account? accountToUpdate))
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
        if (!accountService.TryUpdate(accountToUpdate, updateAccountModel.Name, out IEnumerable<Exception> exceptions))
        {
            var errors = exceptions.GroupBy(e => e switch
            {
                InvalidNameException => nameof(updateAccountModel.Name),
                _ => string.Empty
            }).ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to update Account.",
                Errors = errors,
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok(accountMapper.ToModel(accountToUpdate));
    }

    /// <summary>
    /// Deletes the Account with the provided ID
    /// </summary>
    [HttpDelete("{accountId}")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteAsync(Guid accountId)
    {
        if (!accountMapper.TryToDomain(accountId, out Account? accountToDelete))
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
        if (!accountService.TryDelete(accountToDelete, out IEnumerable<Exception> exceptions))
        {
            return new UnprocessableEntityObjectResult(new ValidationProblemDetails
            {
                Title = "Unable to delete Account.",
                Errors = {
                    { string.Empty, exceptions.Select(e => e.Message).ToArray() }
                },
                Status = StatusCodes.Status422UnprocessableEntity
            });
        }
        await unitOfWork.SaveChangesAsync();
        return Ok();
    }
}