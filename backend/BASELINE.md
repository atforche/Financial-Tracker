# Backend cleanup baseline

Recorded on 2026-09-22, before API or backend structure changes.

## Current verification

- `dotnet test backend/Tests/Tests.csproj --no-restore -m:1 -v:minimal`: 187 passed, 0 failed, 0 skipped (32 seconds of test execution).
- After adding the page characterization and both measurement tests, the full suite passes 216 tests, 0 failed, 0 skipped (37 seconds of test execution).
- The backend currently has 67 controller actions, including 36 GET actions. This is a source count of attributes, not an OpenAPI operation count.
- The frontend has 37 `app/**/page.tsx` routes. Several routes redirect or display forms and share the same backend reads.
- The original suite covers domain and endpoint behaviors; the new `Tests/Pages/` classes group the added characterization assertions by frontend page.

## Page read inventory

These are GET calls visible in the server component that owns each page. Alternative calls for date versus accounting period mode, conditional sections, pagination, and detail state appear together; the list is **not** a count of requests made during one render. The route files for `/accounts`, `/funds`, `/account-goals`, `/transactions`, and several workspace URLs redirect or delegate to these components.

| Page or view | Current GET contracts |
| --- | --- |
| Overview | `/accounting-periods`; `/accounts/date-range` or `/accounts/accounting-period-range`; `/funds/date-range` or `/funds/accounting-period-range` |
| Account workspace | `/accounting-periods`; `/accounts/with-balances`; `/accounts/financial-institutions` |
| Account detail | `/accounts/with-balances`; `/accounts/{accountId}/balance-events`; `/accounts/financial-institutions`; `/accounts/{accountId}/balance-events/totals` |
| Account trends | `/accounting-periods`; `/accounts/date-range` or `/accounts/accounting-period-range` |
| Fund workspace | `/accounting-periods`; `/funds/with-balances` |
| Fund create form | `/accounting-periods/with-balances` |
| Fund onboard form | `/accounting-periods`; `/funds/with-balances` |
| Fund detail | `/funds/with-balances`; `/funds/balance-events/date-range`; `/funds/balance-events/date-range/totals` |
| Fund trends | `/accounting-periods`; `/funds/date-range` or `/funds/accounting-period-range` |
| Accounting period workspace | `/accounting-periods` for the first period; `/accounting-periods/with-balances` for the latest period; `/accounting-periods/with-balances` for the paginated list |
| Accounting period Cash Flow | `/accounting-periods/{accountingPeriodId}`; `/accounting-periods/{accountingPeriodId}/transactions`; `/accounts/accounting-period-range`; `/funds/accounting-period-range` |
| Accounting period Plan | `/accounting-periods/{accountingPeriodId}`; `/fund-goals`; `/fund-goals/progress/{accountingPeriodId}`; `/account-goals`; `/account-goals/progress/{accountingPeriodId}` |
| Accounting period trends | `/accounting-periods`; `/accounting-periods/range`; `/accounts/accounting-period-range`; `/funds/accounting-period-range` |
| Expected income source detail/edit | `/accounting-periods/{accountingPeriodId}` |
| Account goal workspace/trends | `/accounting-periods`; `/account-goals`; `/account-goals/progress/{accountingPeriodId}` |
| Account goal detail | `/accounting-periods`; `/account-goals/account/{accountId}`; `/account-goals/{accountGoalId}/progress/{accountingPeriodId}`; account balance event and date endpoints |
| Fund goal workspace/trends | `/accounting-periods`; `/fund-goals`; `/fund-goals/progress/{accountingPeriodId}` |
| Fund goal detail | `/accounting-periods`; `/fund-goals/fund/{fundId}`; `/fund-goals/{fundGoalId}/progress/{accountingPeriodId}`; fund goal balance event and date endpoints |
| Locations | `/locations` |
| Location detail | `/locations`; `/transactions` filtered by location; `/transactions/date-range` filtered by location |
| Location trends | `/locations`; `/accounting-periods`; `/transactions/date-range` or `/transactions/accounting-period-range` |
| Transaction workspace | `/accounting-periods`; `/accounts/with-balances`; `/funds/with-balances`; `/locations`; conditionally `/fund-goals` and one `/fund-goals/progress/{accountingPeriodId}` per open period; `/transactions`, `/transactions/date-range`, or `/transactions/accounting-period-range` only when a transaction filter is active |
| Transaction create/edit form | Transaction reference reads above; edit also requests `/transactions/{transactionId}` |
| Transaction detail | `/transactions/{transactionId}`; `/accounting-periods/{accountingPeriodId}`; `/funds/with-balances`; `/fund-goals`; `/fund-goals/progress/{accountingPeriodId}` |
| User administration | `/users`; `/user-invitations`; the route also resolves the signed-in user |

Transaction routes and some detail/form routes share server-side data loaders. The rendered trace below captures one state for each route; static import traversal would overcount calls from components that are not rendered. Other filters and page states can change the request shape.

## Route-to-view map

This lists all 37 `app/**/page.tsx` routes. A redirect has no dedicated backend page read. A shared or form route still needs coverage of any unique reference reads and mutation behavior.

| Frontend route | Data-bearing view or route behavior |
| --- | --- |
| `/` | Overview |
| `/login` | Sign-in and redirect; authentication contract |
| `/accounts` | Redirect to account workspace |
| `/accounts/workspace` | Account workspace |
| `/accounts/workspace/[accountId]` | Account detail |
| `/accounts/trends` | Account trends |
| `/funds` | Redirect to fund workspace |
| `/funds/workspace` | Fund workspace |
| `/funds/workspace/[fundId]` | Fund detail |
| `/funds/workspace/create` | Fund create form |
| `/funds/workspace/onboard` | Fund onboard form |
| `/funds/trends` | Fund trends |
| `/accounting-periods` | Redirect to accounting period workspace |
| `/accounting-periods/workspace` | Accounting period workspace |
| `/accounting-periods/workspace/[accountingPeriodId]` | Accounting period Cash Flow |
| `/accounting-periods/workspace/[accountingPeriodId]/plan` | Accounting period Plan |
| `/accounting-periods/workspace/[accountingPeriodId]/expected-income-sources/create` | Expected income source form, using period detail |
| `/accounting-periods/workspace/[accountingPeriodId]/expected-income-sources/[sourceId]` | Expected income source detail, using period detail |
| `/accounting-periods/workspace/[accountingPeriodId]/expected-income-sources/[sourceId]/edit` | Expected income source edit, using period detail |
| `/accounting-periods/trends` | Accounting period trends |
| `/account-goals` | Redirect to account goal workspace |
| `/account-goals/workspace` | Account goal workspace |
| `/account-goals/workspace/[accountId]` | Account goal detail |
| `/account-goals/trends` | Account goal trends |
| `/fund-goals` | Redirect to fund goal workspace |
| `/fund-goals/workspace` | Fund goal workspace |
| `/fund-goals/workspace/[fundId]` | Fund goal detail |
| `/fund-goals/trends` | Fund goal trends |
| `/locations` | Location workspace |
| `/locations/[locationId]` | Location detail |
| `/locations/trends` | Location trends |
| `/transactions` | Redirect to transaction workspace |
| `/transactions/workspace` | Transaction workspace |
| `/transactions/workspace/create` | Transaction create form |
| `/transactions/workspace/[transactionId]` | Transaction detail |
| `/transactions/workspace/[transactionId]/edit` | Transaction edit form |
| `/admin/users` | User administration |

## Fixed-fixture API read measurements

Run `dotnet test backend/Tests/Tests.csproj --no-restore -m:1 --filter FullyQualifiedName~PageReadMeasurementTests --logger 'console;verbosity=detailed'` to reproduce this table. The test uses an isolated SQLite database with one account (opening balance 100), one July 2026 period, one fund, a posted income of 40, a posted spending of 20, and the resulting locations and goals. Each row sends the listed page read group through the in-process REST test host, one request at a time. Bytes are uncompressed response bodies. SQL commands are counted when EF Core initializes them after fixture setup and include authentication and read-model work. The duration is the sum of in-process request times in one run, **not** browser page load time or a latency target. Alternative modes, independent pagination, and conditional sections need their own traces.

| Page read group | Requests | JSON bytes | Initialized SQL commands | Summed ms |
| --- | ---: | ---: | ---: | ---: |
| Overview | 3 | 9,176 | 17 | 134 |
| Account workspace | 3 | 367 | 9 | 37 |
| Account detail | 4 | 1,850 | 36 | 219 |
| Account trends | 2 | 5,714 | 10 | 6 |
| Fund workspace | 2 | 522 | 7 | 31 |
| Fund create | 1 | 441 | 10 | 47 |
| Fund detail | 3 | 3,208 | 38 | 165 |
| Fund trends | 2 | 3,769 | 10 | 4 |
| Accounting period workspace | 3 | 1,009 | 23 | 14 |
| Accounting period Cash Flow | 4 | 8,774 | 84 | 345 |
| Accounting period Plan | 5 | 2,367 | 29 | 100 |
| Accounting period trends | 4 | 2,156 | 41 | 45 |
| Expected income source | 1 | 414 | 9 | 2 |
| Account goal workspace | 3 | 735 | 13 | 3 |
| Account goal trends | 3 | 735 | 13 | 2 |
| Account goal detail | 5 | 2,248 | 42 | 21 |
| Fund goal workspace | 3 | 1,472 | 13 | 3 |
| Fund goal trends | 3 | 1,472 | 13 | 3 |
| Fund goal detail | 5 | 3,269 | 52 | 176 |
| Location workspace | 1 | 152 | 3 | 4 |
| Location trends | 3 | 3,723 | 57 | 308 |
| Transaction workspace, filtered | 7 | 8,650 | 64 | 27 |
| Transaction create | 6 | 2,226 | 24 | 6 |
| Transaction detail | 5 | 5,291 | 61 | 20 |
| Transaction edit | 7 | 5,363 | 62 | 18 |
| Location detail | 3 | 6,780 | 93 | 191 |
| User administration | 3 | 672 | 5 | 37 |

These are controlled API costs for the small fixture. The expanded fixture and rendered-page trace below provide separate scaling and navigation evidence; neither substitutes for production-sized data.

### Expanded fixed fixture

`ExpandedFixtureReadCostsAreRecorded` uses three consecutive periods, three accounts, three funds, and nine posted spending transactions at one location. The fixture stays below the API's 30-write-per-minute limit during setup. It exercises the read groups with the greatest initial cost. The same caveats about in-process timing and uncompressed JSON apply.

| Page read group | Requests | JSON bytes | Initialized SQL commands | Summed ms |
| --- | ---: | ---: | ---: | ---: |
| Overview | 3 | 26,537 | 17 | 148 |
| Account workspace | 3 | 941 | 9 | 35 |
| Account detail | 4 | 2,994 | 34 | 254 |
| Fund detail | 3 | 6,881 | 36 | 253 |
| Accounting period Cash Flow | 4 | 13,462 | 123 | 411 |
| Accounting period Plan | 5 | 4,841 | 29 | 98 |
| Transaction workspace, filtered | 7 | 13,848 | 103 | 49 |
| Location detail | 3 | 44,522 | 173 | 494 |

The growth in location-detail bytes and commands on this fixture is a reason to inspect its queries during redesign. These two fixture sizes are too small to predict production latency.

## Rendered-page request trace

An isolated native Development backend, fresh SQLite database, and production-built Next.js frontend were run on local ports. The browser signed in as the local administrator and navigated each route. Backend request-start and request-finish logs supplied the actual API calls, status, and summed server handling time. The trace includes the shared `/users/me` layout read and any requests caused by Next.js prefetch or redirects. Summed handling time is **not** navigation latency because requests can run in parallel and the browser performs other work. API response bytes and SQL counts are measured by the fixed-fixture tests above.

The fixture contained one account, one July 2026 period, one fund, one posted spending transaction, one expected income source, and the resulting location. All 35 navigations below returned HTTP 200 and all recorded API calls returned 200. The fund onboarding route redirected to the fund workspace because this fixture already had a fund.

| Rendered route or state | API requests | Summed backend ms |
| --- | ---: | ---: |
| `/` date mode | 4 | 270 |
| `/accounts/workspace` | 4 | 20 |
| `/accounts/workspace/[accountId]` | 9 | 436 |
| `/accounts/trends` date mode | 5 | 9 |
| `/funds/workspace` | 3 | 30 |
| `/funds/workspace/[fundId]` | 7 | 320 |
| `/funds/workspace/create` | 2 | 171 |
| `/funds/workspace/onboard` redirect | 6 | 8 |
| `/funds/trends` date mode | 5 | 6 |
| `/accounting-periods/workspace` | 4 | 13 |
| `/accounting-periods/workspace/[accountingPeriodId]` | 6 | 397 |
| `/accounting-periods/workspace/[accountingPeriodId]/plan` | 7 | 113 |
| `/accounting-periods/trends` | 6 | 46 |
| Expected income source create/detail/edit | 2 each | 3–5 |
| `/account-goals/workspace` | 4 | 369 |
| `/account-goals/workspace/[accountId]` | 15 | 565 |
| `/account-goals/trends` | 6 | 11 |
| `/fund-goals/workspace` | 4 | 59 |
| `/fund-goals/workspace/[fundId]` | 9 | 535 |
| `/fund-goals/trends` | 6 | 7 |
| `/locations` | 2 | 16 |
| `/locations/[locationId]` | 4 | 508 |
| `/locations/trends` date mode | 4 | 109 |
| `/transactions/workspace` unfiltered | 7 | 38 |
| `/transactions/workspace/create` | 7 | 9 |
| `/transactions/workspace/[transactionId]` | 6 | 115 |
| `/transactions/workspace/[transactionId]/edit` | 8 | 20 |
| `/admin/users` | 3 | 18 |
| `/` accounting-period mode | 4 | 636 |
| `/accounts/trends` accounting-period mode | 3 | 18 |
| `/funds/trends` accounting-period mode | 3 | 17 |
| `/locations/trends` accounting-period mode | 4 | 1,531 |
| `/transactions/workspace` filtered by period | 8 | 256 |

The trace is reproducible with [the local seeder](../scripts/backend-baseline-seed.mjs) and [browser trace script](../scripts/backend-baseline-trace.mjs). Use a **fresh isolated Development database**. The following commands show the setup; run the backend and frontend in separate terminals after migrating the database:

```sh
mkdir -p /tmp/ft-baseline/logs
set -a
source debug/.env
set +a
export DATABASE_PATH=/tmp/ft-baseline/database.db LOG_DIRECTORY=/tmp/ft-baseline/logs
export ASPNETCORE_HTTP_PORTS=18081 FRONTEND_ORIGIN=http://localhost:13001
dotnet run --project backend/Migrator/Migrator.csproj
env 'Serilog__MinimumLevel__Override__Microsoft.AspNetCore.Hosting.Diagnostics=Information' dotnet run --project backend/Rest/Rest.csproj
```

In the frontend terminal, source `../debug/.env`, set `API_URL=http://127.0.0.1:18081`, `PUBLIC_ORIGIN=http://localhost:13001`, and `AUTH_URL=http://localhost:13001`, then run `npm run start -- -p 13001` from `frontend/` after building the frontend. Seed and trace from the repository root:

```sh
BASELINE_API_URL=http://127.0.0.1:18081 BASELINE_FIXTURE=/tmp/ft-baseline/seed.json node scripts/backend-baseline-seed.mjs
BASELINE_FIXTURE=/tmp/ft-baseline/seed.json BASELINE_API_LOG=/tmp/ft-baseline/logs/api-log-YYYYMMDD.log BASELINE_OUTPUT=/tmp/ft-baseline/trace-0.json BASELINE_ROUTE_START=0 BASELINE_ROUTE_END=16 node scripts/backend-baseline-trace.mjs
```

Replace `YYYYMMDD` with the log date. Keep route batches small enough to stay below the API's 120-read-per-minute limit; the recorded run used `0–16`, `16–30`, and `30–35`, restarting the isolated backend between batches and using a separate output file for each. The seed script refuses a non-loopback API URL or a database that already has accounts or periods. The `debug/.env` file supplies only local development authentication configuration; the overridden `DATABASE_PATH` keeps these reads away from any existing debug database.

## Before changing a page contract

For each page, capture a representative rendered request trace with status, request count, transferred bytes, and duration. Use a fixed seeded database and record the query count for each backend request. Add a page-specific behavior test that asserts the financial or lifecycle values the page uses, including filters and pagination where applicable. Existing endpoint tests may be moved or extended rather than duplicated. Record the old and new measurements against the same fixture.

## Characterization test progress

`Tests/Pages/` now has 27 page read behavior tests and two measurement tests. Redirects have no unique read. The login page uses authentication contracts covered separately; fund onboarding shares the fund workspace reads; expected income source create/detail/edit share one period read; the unfiltered transaction workspace loads reference data covered by its filtered and create-form tests. The 187-test result above is the pre-addition suite and remains the comparison point. Page tests cover representative non-empty states, not every conditional or date/accounting-period mode. Existing endpoint and domain tests retain the deeper lifecycle, validation, and financial edge cases.

| Page read group | Characterization test class |
| --- | --- |
| Overview | `OverviewPageReadTests` |
| Account workspace, detail, trends | `AccountPageReadTests` |
| Fund workspace, create, detail, trends | `FundPageReadTests` |
| Accounting period workspace, Cash Flow, Plan, trends, expected income source | `AccountingPeriodPageReadTests` |
| Account goal workspace, detail, trends | `AccountGoalPageReadTests` |
| Fund goal workspace, detail, trends | `FundGoalPageReadTests` |
| Location workspace, detail, trends | `LocationPageReadTests` |
| Transaction workspace, create, detail, edit | `TransactionPageReadTests` |
| User administration | `UserAdministrationPageReadTests` |

The API measurement groups are maintained in `PageReadMeasurementTests`. They are intentionally separate from the behavioral assertions so changing an implementation cost does not make a behavior test fail.

## Baseline limits for later page migrations

The fixed-fixture tests provide comparable response bytes and SQL counts for representative page read groups. The browser trace provides the requests made during one rendered navigation, including layout reads. Their fixtures differ, so the two tables should not be combined into a single cost estimate. Server handling times vary between runs and are diagnostic only.

Before migrating a page, capture any state absent from this baseline on the same fixture before and after the change: empty onboarding, other filters or pagination, conditional sections, and client interactions after navigation. Record the fields the rendered page uses and its actual transferred bytes and navigation duration at that point. The fund onboarding form needs an empty-fund fixture because the current browser fixture exercises its redirect. This keeps the per-page gate in [CLEANUP_PLAN.md](CLEANUP_PLAN.md) explicit without treating one representative state as exhaustive coverage.
