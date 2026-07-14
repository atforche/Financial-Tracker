# Data Query Service Design

## Purpose

Introduce a read-only query service layer in the `Data` project that supports the data retrieval needs of the REST API.

The query layer will:

- Accept the query parameter models already defined in the `Models` project.
- Return the response models already defined in the `Models` project.
- Build efficient EF Core queries that filter, sort, aggregate, and paginate in the database where possible.
- Keep domain repositories focused on loading and persisting aggregates for command workflows.
- Reduce REST controllers to HTTP concerns such as model binding, status codes, and validation-problem responses.

The initial implementation should focus on `Rest.AccountingPeriods`, but its conventions should be suitable for the other REST namespaces.

## Current State

Read behavior for Accounting Periods currently lives primarily in the REST project:

- `AccountingPeriodGetter` loads all Accounting Period domain entities, converts all of them, and then filters, sorts, and pages the resulting models in memory.
- `AccountingPeriodConverter` queries the balance-history repository for every converted Accounting Period. Converting a collection therefore produces an N+1 query pattern.
- `CurrentAccountingPeriodGetter` loads all transactions for the current period, calculates totals, converts the transactions, sorts them, and pages them in memory.
- `AccountingPeriodTrendsGetter` resolves and traverses a range of periods, loads transactions period by period, calculates aggregates, converts results, sorts them, and pages them.
- `AccountingPeriodController.GetAllOpen` executes a database-backed read directly from the controller.

These classes collectively act like a query layer, but they are located in REST and operate primarily on materialized domain objects rather than database projections.

There is also currently a mismatch between `Rest.AccountingPeriods` and `Models.AccountingPeriods`. REST still references types and properties such as:

- `CurrentAccountingPeriodModel`
- `CurrentAccountingPeriodQueryParameterModel`
- `AccountingPeriodTrendsModel`
- `AccountingPeriodTrendsQueryParameterModel`
- `AccountingPeriodSortOrderModel`
- `AccountingPeriodQueryParameterModel.Years`
- `AccountingPeriodQueryParameterModel.Months`

The current Models project instead exposes newer contracts built around filters, ranges, and the `AccountingPeriodWithBalanceModel` and `AccountingPeriodWithTransactionsModel` types. The Models contracts should be treated as the intended API during this redesign.

## Dependency Direction

The intended dependency structure is:

```text
REST
 |-- Models
 |-- Data query services --> Models
 `-- Domain services and repositories for commands

Data
 |-- Domain
 |-- Models
 `-- EF Core
```

`Data.csproj` should add a project reference to `Models.csproj`:

```xml
<ProjectReference Include="../Models/Models.csproj" />
```

This dependency is intentional. The query layer's responsibility is to produce the application's shared read models from persisted data.

The Models project must remain independent. It should not reference Data, Domain, EF Core, or ASP.NET.

## Repository and Query Service Responsibilities

Domain repositories and query services serve different use cases.

### Domain repositories

Repositories should continue to:

- Load domain aggregates needed by domain services.
- Track aggregates that will be changed.
- Add and remove aggregates.
- Support command-side business rules.

Repositories should return domain objects rather than API models.

### Query services

Query services should:

- Be read-only.
- Use `AsNoTracking()` by default.
- Accept shared Models query types.
- Return shared Models response types.
- Project directly from EF queries into read models.
- Apply filtering, sorting, counting, and paging before materialization.
- Calculate aggregate values independently of result paging.
- Avoid returning domain entities to REST.

Query services should not return `IQueryable` to REST. The query should be composed and executed inside Data so that EF-specific behavior does not leak across the project boundary.

## Accounting Period Query Service

Start with a feature-oriented concrete service:

```csharp
namespace Data.AccountingPeriods;

public sealed class AccountingPeriodQueryService(DatabaseContext databaseContext)
{
    public Task<CollectionModel<AccountingPeriodModel>> GetAsync(
        AccountingPeriodQueryParameterModel query,
        CancellationToken cancellationToken = default);

    public Task<CollectionModel<AccountingPeriodWithBalanceModel>> GetWithBalancesAsync(
        AccountingPeriodWithBalanceQueryParameterModel query,
        CancellationToken cancellationToken = default);

    public Task<AccountingPeriodWithBalanceModel?> GetByIdAsync(
        Guid accountingPeriodId,
        CancellationToken cancellationToken = default);

    public Task<AccountingPeriodWithTransactionsModel?> GetWithTransactionsAsync(
        Guid accountingPeriodId,
        AccountingPeriodWithTransactionsQueryParameterModel query,
        CancellationToken cancellationToken = default);

    public Task<AccountingPeriodsInRangeQueryResult> GetRangeAsync(
        AccountingPeriodsInRangeQueryParameterModel query,
        CancellationToken cancellationToken = default);
}
```

An `IAccountingPeriodQueryService` interface is not initially necessary. The existing application already injects concrete Data services, and an interface with only one EF-backed implementation would not create a meaningful boundary. An interface can be introduced later if a real substitution requirement emerges.

Register the service in `Data.ServiceManager`:

```csharp
_ = serviceCollection.AddScoped<AccountingPeriodQueryService>();
```

## Basic Accounting Period Query

The list query should remain as an `IQueryable` until the count and requested page are retrieved:

```csharp
public async Task<CollectionModel<AccountingPeriodModel>> GetAsync(
    AccountingPeriodQueryParameterModel request,
    CancellationToken cancellationToken = default)
{
    IQueryable<AccountingPeriod> query = databaseContext.AccountingPeriods
        .AsNoTracking();

    if (request.Filter?.Years is { Count: > 0 } years)
    {
        query = query.Where(period => years.Contains(period.Year));
    }

    if (request.Filter?.Months is { Count: > 0 } months)
    {
        query = query.Where(period => months.Contains(period.Month));
    }

    query = request.Sort switch
    {
        AccountingPeriodSortModel.Date =>
            query.OrderBy(period => period.Year)
                .ThenBy(period => period.Month),

        AccountingPeriodSortModel.IsOpen =>
            query.OrderBy(period => period.IsOpen)
                .ThenByDescending(period => period.Year)
                .ThenByDescending(period => period.Month),

        AccountingPeriodSortModel.IsOpenDescending =>
            query.OrderByDescending(period => period.IsOpen)
                .ThenByDescending(period => period.Year)
                .ThenByDescending(period => period.Month),

        _ =>
            query.OrderByDescending(period => period.Year)
                .ThenByDescending(period => period.Month),
    };

    int totalCount = await query.CountAsync(cancellationToken);

    List<AccountingPeriodModel> items = await query
        .Skip(request.Offset ?? 0)
        .Take(request.Limit ?? int.MaxValue)
        .Select(period => new AccountingPeriodModel
        {
            Id = period.Id.Value,
            Name = period.Name,
            Year = period.Year,
            Month = period.Month,
            IsOpen = period.IsOpen,
        })
        .ToListAsync(cancellationToken);

    return new CollectionModel<AccountingPeriodModel>
    {
        Items = items,
        TotalCount = totalCount,
    };
}
```

This ensures that:

- Filters execute in SQL.
- Sorting executes in SQL.
- `TotalCount` describes all matching results before pagination.
- Only the requested page is materialized.
- The result is projected directly into the shared model.

The translation of value-object properties such as `period.Id.Value` must be verified against the SQLite EF provider. If EF cannot translate it, the projection should use a supported expression based on the configured value conversion. An in-memory fallback should not be used for an otherwise database-translatable list query.

## Queries with Balances

`AccountingPeriodWithBalanceModel` should be produced using a database join rather than by calling `AccountingPeriodConverter` once per period:

```csharp
private IQueryable<AccountingPeriodWithBalanceModel> QueryWithBalances() =>
    from period in databaseContext.AccountingPeriods.AsNoTracking()
    join history in databaseContext.AccountingPeriodBalanceHistories.AsNoTracking()
        on period.Id equals history.AccountingPeriod.Id
    select new AccountingPeriodWithBalanceModel
    {
        Id = period.Id.Value,
        Name = period.Name,
        Year = period.Year,
        Month = period.Month,
        IsOpen = period.IsOpen,
        OpeningBalance = history.OpeningBalance,
        ClosingBalance = history.ClosingBalance,
    };
```

Filtering, balance-aware sorting, counting, and paging can then operate over this projection.

The relationship is conceptually required: every persisted Accounting Period should have a corresponding balance history. An inner join is therefore appropriate. If the database can legally contain a period without a balance history, that invariant should be explicitly decided before choosing a left join and fallback balance values.

Small, strongly typed filter and sort methods are preferable to a generic query framework:

```csharp
private static IQueryable<AccountingPeriodWithBalanceModel> ApplySort(
    IQueryable<AccountingPeriodWithBalanceModel> query,
    AccountingPeriodWithBalanceSortModel? sort);
```

This keeps every supported API sort visible and aligned with its Models enum.

Every ordering must include a stable tie-breaker. For example, a balance sort should then order by year, month, and, if necessary, ID so that offset pagination remains deterministic.

## Single-Period Queries

`GetByIdAsync` should return `null` when the requested period does not exist. The REST controller decides whether that maps to `404 Not Found` or another API-specific response.

This method should return an `AccountingPeriodWithBalanceModel` because command responses currently expose balance information through `AccountingPeriodConverter`. After create, close, or reopen operations, REST can query the saved read model by ID instead of converting a tracked domain entity and issuing a separate balance-history repository call.

Domain lookup must remain separate. Command endpoints should use `AccountingPeriodRepository` to load an aggregate for mutation rather than asking a model converter to resolve a domain entity.

## Range Queries

`AccountingPeriodsInRangeQueryParameterModel` provides start and end Accounting Period IDs. The range query should:

1. Resolve both endpoint IDs in one database query.
2. Report which endpoint was not found when either is absent.
3. Compare the endpoint `(Year, Month)` values and reject a reversed range.
4. Select every period between those values in one SQL query.
5. Verify after materialization that the returned months are contiguous.
6. Query total income and spending for the complete range.
7. Sort and page the Accounting Period collection separately.

Range totals must describe the complete requested range, not only the Accounting Periods included in the returned page.

Database-dependent failures should be represented by a Data-owned result type rather than ASP.NET types:

```csharp
public enum AccountingPeriodRangeQueryFailure
{
    StartNotFound,
    EndNotFound,
    Reversed,
    NotContiguous,
}

public sealed record AccountingPeriodsInRangeQueryResult(
    AccountingPeriodsInRangeModel? Model,
    AccountingPeriodRangeQueryFailure? Failure);
```

REST maps this outcome to `ValidationProblemDetails`. This allows Data to own validation that requires persisted state without coupling Data to HTTP response classes.

If both endpoint IDs are missing, the result must be capable of representing both failures. The final implementation can therefore use a flags enum or a collection of typed failures rather than restricting the result to a single enum value.

## Transaction Queries

`AccountingPeriodWithTransactionsModel` is more complex because `TransactionModel` is polymorphic and contains nested source, destination, account, fund, income-line, deduction, and assignment data.

Transaction read behavior should not be moved into `AccountingPeriodQueryService` as duplicated private logic. Introduce a reusable Data service:

```csharp
namespace Data.Transactions;

public sealed class TransactionQueryService(DatabaseContext databaseContext)
{
    public Task<CollectionModel<TransactionModel>> GetForAccountingPeriodAsync(
        Guid accountingPeriodId,
        AccountingPeriodWithTransactionsQueryParameterModel query,
        CancellationToken cancellationToken = default);

    public Task<IncomeAmountModel> GetIncomeTotalsAsync(
        IReadOnlyCollection<Guid> accountingPeriodIds,
        CancellationToken cancellationToken = default);

    public Task<decimal> GetSpendingTotalAsync(
        IReadOnlyCollection<Guid> accountingPeriodIds,
        CancellationToken cancellationToken = default);
}
```

The exact public contract can be refined as transaction endpoints move to the query layer. The important boundary is that transaction selection, sorting, paging, projection, and aggregation live in Data and are reusable across Accounting Period and Transaction endpoints.

The initial implementation may need to:

1. Build a filtered transaction query.
2. Apply any SQL-translatable sort.
3. Count all matching transactions.
4. Page transaction IDs or transaction roots.
5. Load only the requested page with the required owned data.
6. Map the page to the appropriate polymorphic transaction models inside Data.

Frequently used projections can be pushed further into SQL as EF translation behavior is established. The implementation should avoid loading every transaction in a period merely to return one page.

Aggregate totals must be calculated independently of transaction pagination.

The existing REST code also uses inconsistent definitions for tracked and untracked income. `CurrentAccountingPeriodGetter` classifies the entire transaction amount from the source account, while `AccountingPeriodTrendsGetter` classifies destination amounts. A single transaction query implementation should establish one definition and use it consistently. Based on the data model, destination amounts appear to be the more precise source for tracked-versus-untracked income, but this is a business rule that should be confirmed during implementation.

## REST Responsibilities After Migration

Read endpoints should become thin asynchronous controller actions:

```csharp
[HttpGet]
[ProducesResponseType(typeof(CollectionModel<AccountingPeriodModel>), StatusCodes.Status200OK)]
public async Task<ActionResult<CollectionModel<AccountingPeriodModel>>> GetManyAsync(
    [FromQuery] AccountingPeriodQueryParameterModel query,
    CancellationToken cancellationToken) =>
    Ok(await accountingPeriodQueryService.GetAsync(query, cancellationToken));
```

REST remains responsible for:

- Binding route, query, and body models.
- Translating query outcomes into HTTP status codes.
- Producing `ValidationProblemDetails`.
- Supplying the request cancellation token.
- Calling domain services for command workflows.
- Saving command-side changes through `UnitOfWork`.

REST should not:

- Compose EF queries.
- Load all domain entities for a read request.
- Sort or page read collections in memory when SQL can perform the operation.
- Calculate reusable financial aggregates.
- Use converters as domain repositories.

Create, close, reopen, and delete commands should continue to use `AccountingPeriodService`, `AccountingPeriodRepository`, and `UnitOfWork`. After a successful create, close, or reopen command, the controller can retrieve the response model through `AccountingPeriodQueryService.GetByIdAsync`.

## Proposed Endpoint Alignment

The current Models contracts support the following endpoint shape:

| Endpoint | Result |
| --- | --- |
| `GET /accounting-periods` | `CollectionModel<AccountingPeriodModel>` |
| `GET /accounting-periods/with-balances` | `CollectionModel<AccountingPeriodWithBalanceModel>` |
| `GET /accounting-periods/{id}` | `AccountingPeriodWithBalanceModel` |
| `GET /accounting-periods/{id}/transactions` | `AccountingPeriodWithTransactionsModel` |
| `GET /accounting-periods/range` | `AccountingPeriodsInRangeModel` |

Route ordering must ensure that literal routes such as `range` and `with-balances` are not treated as `{id}` values.

The existing `/current`, `/trends`, and `/open` endpoints can be retained as convenience routes backed by the same query services. Alternatively, they can be removed if the newer Models contracts intentionally supersede them. This is an API compatibility decision rather than a Data-layer design decision.

If retained:

- `/open` should call the normal list query with an `IsOpen` filter. The current `AccountingPeriodFilterModel` does not expose this filter, so it would need to be added or represented by a dedicated query method.
- `/current` should resolve the latest Accounting Period in Data and compose the period-with-transactions query.
- `/trends` should become the range endpoint or a thin alias for it.

## Pagination and Sorting Rules

All query services should follow the same rules:

- `TotalCount` is calculated after filtering and before paging.
- Aggregate totals are calculated over the entire filtered set, not the page.
- A missing `Offset` means zero.
- A missing `Limit` means no application-level limit, although introducing a REST default and maximum should be considered separately.
- Negative offsets and limits should be rejected through model validation rather than passed to EF.
- Every sort includes deterministic secondary keys.
- The default sort is explicit and covered by tests.
- Sorting and paging occur before materialization whenever possible.

Offset pagination matches the existing Models contracts. Cursor pagination is not necessary for this initial redesign.

## Naming and Organization

Suggested Data structure:

```text
backend/Data/
 |-- AccountingPeriods/
 |    |-- AccountingPeriodQueryService.cs
 |    |-- AccountingPeriodRangeQueryResult.cs
 |    |-- AccountingPeriodRepository.cs
 |    `-- ...
 |-- Transactions/
 |    |-- TransactionQueryService.cs
 |    |-- TransactionRepository.cs
 |    `-- ...
 `-- ServiceManager.cs
```

Use `QueryService` consistently for API-shaped read services. Continue using `Repository` for domain persistence abstractions. The distinction should remain visible in class names and constructor dependencies.

## Testing Strategy

Query services should be covered primarily with integration tests against SQLite because EF translation and relational behavior are central to their correctness.

Important cases include:

- Empty database results.
- Filtering by years, months, and combinations of both.
- Every supported sort direction.
- Stable ordering when primary sort values are equal.
- `TotalCount` before pagination.
- Zero, partial, and out-of-range pages.
- Joined balance projections.
- Missing range endpoints.
- Reversed ranges.
- Gaps within a requested range.
- Single-period ranges.
- Range totals remaining unchanged across different pages.
- Transaction polymorphic projection.
- Tracked and untracked income classification.
- Cancellation-token propagation.
- Generated SQL or query-count checks for preventing N+1 behavior where practical.

Controller tests should focus on model binding and mapping query outcomes to HTTP responses rather than repeating query behavior tests.

## Implementation Sequence

1. Add the Models project reference to Data.
2. Add and register `AccountingPeriodQueryService`.
3. Implement the plain Accounting Period list projection.
4. Replace `AccountingPeriodGetter` and the read logic in `GetAllOpen` where the available filter contract permits it.
5. Implement the joined balance projection and `GetByIdAsync`.
6. Update successful command responses to retrieve their response model through the query service.
7. Implement the typed range outcome and range query.
8. Introduce and register `TransactionQueryService`.
9. Implement transaction paging, projection, and shared aggregate calculations.
10. Replace the current-period and trends REST getters with query-service calls.
11. Remove obsolete REST getters and the read-side responsibilities of `AccountingPeriodConverter`.
12. Align Accounting Period routes and controller signatures with the current Models contracts.
13. Verify the generated OpenAPI document.
14. Run targeted Data and REST builds and the query integration tests.

## Result

This design creates a focused read side without introducing another domain layer:

- Domain repositories support business behavior and mutation.
- Data query services support API-shaped, read-only projections.
- Models define the shared input and output contracts.
- REST handles HTTP translation.

It also provides a gradual migration path. Accounting Period queries can move first, followed by reusable Transaction queries and then the other REST namespaces, without requiring a single large rewrite.
