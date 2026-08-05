# First-Class User Management Implementation Plan

## Purpose

This document is the durable implementation and handoff plan for replacing the
Google subject allowlist with database-backed users, invitations, and role-based
authorization.

It is designed to be passed repeatedly to implementation agents. Each agent
should implement one dependency-ready phase, validate it, update this document,
and leave the repository in a coherent state for the next agent.

## Instructions for Agents

Before making changes:

1. Read this entire document.
2. Inspect the current repository state; this plan describes intent, not proof
   that a referenced file or behavior is unchanged.
3. Run `git status --short` and preserve all unrelated user changes.
4. Select only the earliest phase whose dependencies are complete and whose
   status is `Not started` or `In progress`.
5. Do not expand the selected phase into later phases.

While working:

- Follow the existing C# Domain/Data/Models/Rest separation.
- Follow the existing Python `scripts/*_scripts.py` command-module pattern.
- Keep generated frontend API types generated; do not hand-edit them.
- Keep the Google ID token server-only. Never add it to the browser-visible
  Auth.js session.
- Treat the backend as the authorization boundary. Frontend role checks are
  presentation behavior only.
- Preserve the shared-ledger architecture. This feature adds access roles; it
  does not introduce tenant-specific financial data.
- Add tests in the same phase as the behavior they prove.

Before handing off:

1. Run the validation required by the selected phase.
2. Run `git diff --check`.
3. Update the phase status, checklist, and validation evidence in this document.
4. Add a short entry to the Handoff Log.
5. State any limitations honestly. Static inspection is not runtime proof.

Suggested prompt when handing this document to an agent:

> Read `user-management-plan.md` completely. Implement only the earliest
> dependency-ready phase that is not complete. Preserve unrelated changes, run
> the phase validation, update the plan and handoff log with exact evidence, and
> stop after that phase.

## Current-State Summary

The application currently has two independent authorization gates:

- `frontend/auth.ts` accepts Google sign-in only when the profile `sub` appears
  in `GOOGLE_ALLOWED_SUBJECTS`.
- `backend/Rest/Program.cs` validates the Google ID token and applies a fallback
  policy requiring `sub` to appear in the same environment variable.

Auth.js retains the Google ID token in its encrypted server-side token, and
`frontend/framework/data/createApiClient.ts` attaches it to backend requests.
The token must remain absent from `/api/auth/session` and other browser-visible
state.

The backend uses EF Core with SQLite, a separate migrator, repository/domain
services, REST controllers, OpenAPI-generated frontend types, and REST-focused
integration tests. Development authentication is explicitly guarded to the
Development environment and supplies a deterministic subject.

## Product Decisions

These decisions are part of the implementation contract unless the user
explicitly changes them.

### Roles

| Role | Read financial data | Modify financial data | Manage users |
|---|---:|---:|---:|
| `Admin` | Yes | Yes | Yes |
| `Standard` | Yes | Yes | No |
| `ReadOnly` | Yes | No | No |

### Identity and onboarding

- Administrators invite a user by email address and assign a role.
- Email is used only to match a pending invitation during first login.
- Invitation acceptance requires the signed Google ID token to contain a
  matching email and `email_verified = true`.
- After acceptance, the Google `sub` claim is the durable identity key.
- A later email change must not detach or silently rebind an existing subject.
- Authentication proves Google identity; the application database determines
  access and role.
- Uninvited, disabled, conflicting, and insufficiently privileged identities
  receive no application access.

### Authorization

- A valid provider token alone is sufficient only for the narrowly scoped
  first-login identity-resolution endpoint.
- All normal application endpoints require an active application user.
- Safe read methods (`GET`, `HEAD`, and `OPTIONS`) permit every active role.
- All other methods require `Admin` or `Standard` by default.
- User and invitation administration requires `Admin` explicitly.
- Role and status changes take effect on the next backend request, even if an
  existing Auth.js session remains present.
- The system must always retain at least one active administrator.

### Scope boundaries

Included:

- First-class users, invitations, roles, enable/disable state, and focused
  administration auditing.
- Email-first invitation acceptance followed by immutable subject binding.
- Backend authorization, admin REST APIs, role-aware frontend behavior, and an
  admin user-management page.
- A safe first-administrator bootstrap and removal of the old allowlist.

Deferred:

- Multi-tenancy or per-user ownership of financial records.
- Sending invitation email. The first version records an invitation; an admin
  can separately notify the recipient.
- Broad application or infrastructure metrics. The admin page may show user
  counts, pending invitations, disabled users, and recent login information.
- Fine-grained permissions beyond the three roles.

## Target Data Model

Exact naming may follow repository conventions discovered during implementation,
but the persisted semantics must remain stable.

### Application user

| Field | Requirements |
|---|---|
| `Id` | Application-owned `Guid` primary key |
| `GoogleSubject` | Required, unique, ordinal/case-sensitive, maximum 255 characters |
| `Email` | Required current display email |
| `NormalizedEmail` | Required trimmed invariant-lowercase lookup value |
| `DisplayName` | Optional provider profile value |
| `Role` | `Admin`, `Standard`, or `ReadOnly` |
| `Status` | `Active` or `Disabled` |
| `CreatedAt` | UTC timestamp |
| `ActivatedAt` | UTC timestamp |
| `LastLoginAt` | Optional UTC timestamp |
| `UpdatedAt` | UTC timestamp |

Users are disabled rather than deleted so administration history retains stable
actors.

### User invitation

| Field | Requirements |
|---|---|
| `Id` | `Guid` primary key |
| `Email` | Invited display email |
| `NormalizedEmail` | Trimmed invariant-lowercase match value |
| `Role` | Role assigned upon acceptance |
| `Status` | `Pending`, `Accepted`, `Revoked`, or `Expired` |
| `CreatedAt` | UTC timestamp |
| `ExpiresAt` | Optional UTC expiration; decide default before Phase 1 migration |
| `InvitedByUserId` | Admin actor; nullable only for explicit bootstrap records |
| `AcceptedAt` | Set only when accepted |
| `AcceptedByUserId` | Resulting application user |
| `RevokedAt` | Set only when revoked |
| `RevokedByUserId` | Admin actor |

Require at most one pending invitation per normalized email. Retain completed
and revoked invitations for auditability.

### Administration audit event

At minimum record:

- Actor user ID, with an explicit bootstrap/system representation.
- Action.
- Target user ID and/or invitation ID.
- Previous and new role when applicable.
- UTC occurrence time.

Required actions are invitation creation, invitation revocation, invitation
acceptance, role change, disablement, and enablement.

## Target Authentication Flow

1. An admin creates a pending invitation containing email and role.
2. The recipient authenticates with Google through Auth.js.
3. The server-side Auth.js sign-in callback sends `account.id_token` to a
   backend endpoint such as `POST /authentication/resolve-user`.
4. The backend performs normal JWT issuer, audience, lifetime, signing-key, and
   algorithm validation.
5. The endpoint reads `sub`, `email`, and `email_verified` only from the
   validated principal.
6. In one transaction, the backend resolves the identity:
   - Existing active subject: refresh safe profile fields and last-login time.
   - Existing disabled subject: deny.
   - New subject with exactly one matching pending invitation and verified
     email: create the user, accept the invitation, and record an audit event.
   - Anything else: deny without revealing invitation details.
7. Auth.js creates a session only after the backend confirms access.
8. Subsequent backend requests resolve the user by `sub` and enforce current
   database role/status.

Concurrent callbacks must not accept one invitation twice. Enforce this through
a transaction plus database uniqueness or a conditional state transition, not
only an in-memory check.

## Target API

Route names may be adjusted to align with established controller conventions,
but retain these capabilities and authorization boundaries.

| Method and route | Policy | Purpose |
|---|---|---|
| `POST /authentication/resolve-user` | Valid provider identity | Resolve existing user or atomically accept invitation |
| `GET /users/me` | Active user | Return current application identity, role, and status |
| `GET /users` | Admin | List active and disabled users |
| `POST /user-invitations` | Admin | Create a pending invitation |
| `GET /user-invitations` | Admin | List invitations |
| `DELETE /user-invitations/{id}` | Admin | Revoke a pending invitation |
| `POST /users/{id}/role` | Admin | Change a role while preserving last-admin invariant |
| `POST /users/{id}/disable` | Admin | Disable access while preserving last-admin invariant |
| `POST /users/{id}/enable` | Admin | Restore access |

Expected response semantics:

- `401`: missing or invalid provider token.
- `403`: valid identity but uninvited, disabled, or lacking the required role.
- `404`: target user or invitation does not exist.
- `409`: duplicate invitation, identity collision, invalid state transition, or
  last-admin conflict.
- `422`: malformed or semantically invalid input.

Do not accept identity email or subject in the resolution request body. Those
values must come from the validated token.

## Bootstrap and Migration Strategy

The first admin must be created explicitly. Do not automatically make every
subject in the current allowlist an administrator.

Preferred mechanism:

- Add a repository-native deployment command that creates a bootstrap admin
  invitation by email only when there is no active admin.
- The command should be exposed through the existing Python deployment command
  module and call an appropriate backend/migrator capability rather than
  directly duplicating application persistence rules in Python.
- It must refuse to run once an active admin exists and must not print secrets.
- Development initialization should create or bootstrap a deterministic local
  admin without weakening production Google authentication.

The selected rollout does not include a compatibility importer. During the
planned maintenance window:

- Create the bootstrap admin invitation by email.
- Verify the admin's first login and create invitations for the intended
  collaborators by email.
- Remove the old allowlist only after those users have been provisioned and
  the database-backed flow has been verified.

If a future operational review determines that uninterrupted access is more
important than the planned cutover, it must explicitly reopen this decision
and define a temporary `Standard`-only subject import with a removal deadline.

The production cutover must include a database backup before migration and a
verified admin login before the old allowlist is removed.

## Phase Status

| Phase | Description | Dependencies | Status |
|---|---|---|---|
| 0 | Resolve remaining design details and lock contracts | None | Complete |
| 1 | Persistence, domain lifecycle, migration, and bootstrap | Phase 0 | Complete |
| 2 | Identity resolution and backend authorization | Phase 1 | Complete |
| 3 | User and invitation administration API | Phase 2 | Complete |
| 4 | Auth.js handshake and current-user frontend foundation | Phases 2-3 | Complete |
| 5 | Admin UI and read-only application experience | Phase 4 | Not started |
| 6 | Deployment cutover, compatibility removal, and end-to-end proof | Phases 1-5 | Not started |

Allowed statuses are `Not started`, `In progress`, `Blocked`, and `Complete`.

## Phase 0: Resolve Details and Lock Contracts

### Scope

- Confirm invitation expiration behavior. Recommended default: pending
  invitations do not expire automatically in the first release, but retain a
  nullable `ExpiresAt` field for later use.
- Confirm exact enum names, route names, REST models, and error shapes against
  repository conventions.
- Decide the atomic SQLite acceptance strategy and document the transaction and
  unique-index behavior.
- Decide whether deployment requires temporary import of existing allowed
  subjects or may use a short maintenance cutover.
- Inventory all mutating backend endpoints and frontend mutation controls.
- Record the proposed files/projects for each new type without implementing
  later phases.

### Locked contracts

The following decisions close the Phase 0 design questions and are the
contracts for the implementation phases:

- Invitations do not expire automatically in the first release. `ExpiresAt`
  remains nullable and reserved for a later expiration policy; Phase 1 must
  reject an invitation only when a non-null expiration is in the past.
- Domain enum names are `UserRole` (`Admin`, `Standard`, `ReadOnly`),
  `UserStatus` (`Active`, `Disabled`), `UserInvitationStatus` (`Pending`,
  `Accepted`, `Revoked`, `Expired`), and `UserAdministrationAction`. Existing
  JSON configuration serializes these values as strings.
- Email values are trimmed for display, must be non-empty and no longer than
  320 characters, and must parse as one valid email address. Lookup values are
  `Trim().ToLowerInvariant()` and are stored separately in
  `NormalizedEmail`.
- The target routes are fixed as
  `POST /authentication/resolve-user`, `GET /users/me`, `GET /users`,
  `POST /user-invitations`, `GET /user-invitations`,
  `DELETE /user-invitations/{id}`, `POST /users/{id}/role`,
  `POST /users/{id}/disable`, and `POST /users/{id}/enable`.
- REST errors use the existing ASP.NET response conventions: empty or
  standard `ProblemDetails` for authentication/authorization and not-found
  failures, `ProblemDetails` with status 409 for state/conflict failures, and
  `ValidationProblemDetails` with field-keyed `Errors` and status 422 for
  malformed or semantically invalid input. New controllers must declare these
  response types in OpenAPI.
- Invitation acceptance uses a SQLite write-serialized transaction started
  before reading the invitation, a conditional pending-state transition, and
  a unique index enforcing at most one pending invitation per
  `NormalizedEmail`. A uniqueness/state conflict is handled as a safe
  generic denial or 409 according to the endpoint, never as an invitation
  enumeration signal. The implementation must verify the selected EF Core
  transaction behavior against SQLite rather than relying on an in-memory
  check.
- Deployment uses a short planned maintenance cutover. Existing allowlisted
  subjects are not silently imported as administrators or as placeholder
  users; the operator first creates the bootstrap-admin invitation, verifies
  the admin login, and provisions collaborators by email before removing
  `GOOGLE_ALLOWED_SUBJECTS`. A temporary subject compatibility importer is
  therefore out of scope unless a later cutover review explicitly reopens it.
- Accepted, revoked, and expired invitations remain in the database
  indefinitely. The first admin view lists them with status; there is no
  destructive invitation-delete operation in this release.
- An administrator may change their own role or status only when another
  active administrator will remain afterward. The backend enforces this
  invariant and the UI warns before a self-affecting action. The final active
  administrator can never be demoted or disabled.

### Current mutation inventory

Phase 2 must apply the centralized write policy to every current mutating
route. The inventory is:

| Controller | Mutating routes |
|---|---|
| `AccountingPeriodController` | `POST /accounting-periods`, `POST /accounting-periods/{id}/close`, `POST /accounting-periods/{id}/reopen`, `DELETE /accounting-periods/{id}` |
| `AccountController` | `POST /accounts`, `POST /accounts/onboard`, `POST /accounts/{id}`, `DELETE /accounts/{id}` |
| `FundController` | `POST /funds`, `POST /funds/onboard`, `POST /funds/{id}`, `DELETE /funds/{id}` |
| `FundGoalController` | `POST /fund-goals/{id}` |
| `TransactionController` | `POST /transactions`, `POST /transactions/{id}`, `POST /transactions/{id}/post`, `POST /transactions/{id}/unpost`, `DELETE /transactions/{id}` |

The Phase 5 read-only inventory consists of these direct server actions and
their visible control owners:

- Accounting periods: `frontend/accounting-periods/workspace/{createAccountingPeriod,closeAccountingPeriod,reopenAccountingPeriod,deleteAccountingPeriod}.ts`, the matching `CreateAccountingPeriodForm`, `CloseAccountingPeriodForm`, `ReopenAccountingPeriodForm`, `DeleteAccountingPeriodForm`, plus `AccountingPeriodConfirmationForm`, `AccountingPeriodWorkspace`, `AccountingPeriodWorkspaceActions`, and `AccountingPeriodWorkspaceListFrame`.
- Accounts: `frontend/accounts/workspace/{createAccount,onboardAccount,updateAccount,deleteAccount}.ts`, the matching create/onboard/update/delete forms, plus `AccountWorkspace`, `AccountWorkspaceActions`, `AccountWorkspaceDetailPage`, `AccountWorkspaceCards`, `AccountDetailsFrame`, and `AccountStartingBalanceFrame`.
- Funds: `frontend/funds/workspace/{createFund,onboardFund,updateFund,deleteFund}.ts`, the matching create/onboard/update/delete forms, plus `FundWorkspace`, `FundWorkspaceCreatePage`, `FundWorkspaceOnboardPage`, `FundWorkspaceDetailPage`, `FundWorkspaceCards`, and `FundBalanceEventsFrame`.
- Fund goals: `frontend/fund-goals/workspace/updateFundGoal.ts`, `UpdateFundGoalForm`, `FundGoalContextFrame`, `FundGoalWorkspace`, `FundGoalWorkspaceCard`, `FundGoalWorkspaceDetailPage`, and `FundGoalBalanceEventsFrame` (which exposes transaction creation).
- Transactions: `frontend/transactions/workspace/{createTransaction,updateTransaction,deleteTransaction,postTransaction,unpostTransaction}.ts`, the create/update/delete/post/unpost forms, `AccountBalanceEventFrame`, `TransactionWorkspaceListFrame`, `TransactionWorkspaceCreatePage`, `TransactionWorkspaceEditPage`, `ViewTransactionForm`, `TransactionSourceOrDestinationFrame`, and the account/fund/income/spending transaction form components.

### Proposed implementation locations

These are the Phase 1-5 locations selected from the current project
boundaries. They are a planning contract, not files to create during Phase 0.

| Phase | Proposed locations |
|---|---|
| 1 | `backend/Domain/Users/` entities, enums, requests, repositories, and lifecycle service; `backend/Data/Users/` configurations and repositories; `backend/Data/DatabaseContext.cs`, `backend/Data/ServiceManager.cs`, `backend/Domain/ServiceManager.cs`; `backend/Data/Migrations/`; `backend/Migrator/Program.cs`; `scripts/deploy_scripts.py`; `backend/Tests/Users/` and `backend/Tests/Infrastructure/` fixtures |
| 2 | `backend/Domain/Users/` identity-resolution and authorization services; `backend/Rest/Authentication/`; `backend/Rest/Program.cs`; `backend/Rest/ServiceManager.cs`; `backend/Tests/Users/` and authorization integration fixtures |
| 3 | `backend/Models/Users/` and `backend/Models/UserInvitations/`; `backend/Rest/Authentication/`, `backend/Rest/Users/`, and `backend/Rest/UserInvitations/`; `frontend/framework/data/api.ts` generated by `scripts/frontend_scripts.py refresh-models` |
| 4 | `frontend/auth.ts`; `frontend/framework/auth/`; `frontend/framework/data/`; `frontend/framework/navigation/`; authenticated route/layout files under `frontend/app/`; focused frontend/e2e tests |
| 5 | `frontend/app/admin/users/`; `frontend/users/`; `frontend/framework/navigation/`; the Phase 0 mutation-control owners listed above; focused frontend/e2e tests |
| 6 | `scripts/deploy_scripts.py`, `scripts/container_scripts.py`, Compose/configuration templates, workflow/deployment documentation, and production-like e2e/smoke fixtures |

### Acceptance criteria

- [x] No material identity, persistence, API, or rollout ambiguity remains.
- [x] Every current mutation endpoint is listed for Phase 2 authorization tests.
- [x] Every current frontend mutation control/action is listed for Phase 5.
- [x] Bootstrap and compatibility behavior is explicitly selected.
- [x] Decisions are recorded under Decision Log.

### Validation evidence

- Read `user-management-plan.md` in full and inspected the current project,
  controller, authentication, migration, test, script, and frontend
  navigation conventions on 2026-08-05.
- Confirmed the backend mutation inventory with
  `rg -n "^\s*\[Http(Post|Put|Patch|Delete)" backend/Rest -g '*Controller.cs'`.
- Confirmed frontend action/control locations with
  `rg -l "useActionState|createApiClient" frontend --glob '*.ts' --glob '*.tsx'`
  and targeted inspection of the workspace control owners.
- Confirmed generated API model workflow in `scripts/frontend_scripts.py` and
  the existing `backend/.artifacts/obj/Rest/Financial-Tracker-API.json`
  pipeline convention.
- `git diff --no-index --check /dev/null user-management-plan.md` produced no
  whitespace errors (exit status 1 is expected because the new untracked plan
  differs from `/dev/null`).
- No application code or runtime authentication behavior changed in Phase 0;
  persistence and runtime tests remain Phase 1 work.

## Phase 1: Persistence, Domain Lifecycle, Migration, and Bootstrap

### Scope

- Add domain entities and enums for users, invitations, and audit events.
- Add repository interfaces and services for lifecycle operations and queries.
- Add EF configurations, repositories, `DbSet` members, service registration,
  constraints, indexes, and a migration.
- Enforce invitation state transitions and last-active-admin invariants in the
  domain/service layer under a transaction.
- Add the first-admin bootstrap capability and repository-native deployment
  wrapper.
- Seed a deterministic admin for guarded development authentication.
- Do not change the production Auth.js allowlist flow in this phase.

### Required tests

- [x] Entity/configuration persistence round trips.
- [x] Unique subject enforcement.
- [x] At most one pending invitation per normalized email.
- [x] Email normalization and validation.
- [x] Invitation acceptance transition and audit event.
- [x] Revocation and invalid-state transitions.
- [x] Role change, disable, and enable lifecycle.
- [x] Last-active-admin protection for serial lifecycle operations; parallel
  attempts are deferred to the API transaction boundary where they become
  practical to exercise.
- [x] Bootstrap succeeds exactly in an uninitialized system and refuses after
  an active admin exists.
- [x] Migration applies to a representative existing database.

### Acceptance criteria

- [x] Persistence and domain tests pass.
- [x] Migration succeeds without losing existing financial data.
- [x] Bootstrap behavior is idempotent or fails safely and clearly.
- [x] No production authentication behavior changed yet.

### Validation evidence

- Added `backend/Domain/Users/` entities, enums, identifiers, email validation,
  repository contracts, and lifecycle service. The service enforces normalized
  invitation uniqueness, valid state transitions, and the final-active-admin
  invariant.
- Added `backend/Data/Users/` EF configurations and repositories, registered
  them in the existing Domain/Data service managers, added the three user
  `DbSet` members, and added serialized bootstrap/development-seed operations.
- Added the design-time context factory and generated
  `20260805171721_AddUserManagement`, including user-subject uniqueness,
  pending-invitation partial uniqueness, and restricted foreign keys.
- Added the repository-native `bootstrap-admin` deployment command and optional
  migrator environment inputs. `compose.dev.yaml` seeds only the guarded
  development identity; the production Google allowlist flow is unchanged.
- `dotnet restore backend/Backend.sln --force-evaluate --ignore-failed-sources
  -m:1 -p:RestorePackagesPath=/home/atfor/.nuget/packages
  -p:NuGetPackageRoot=/home/atfor/.nuget/packages -p:NuGetAudit=false` passed.
  The explicit package-root and audit settings repaired stale workspace restore
  metadata and avoided the sandbox-blocked NuGet audit request.
- `dotnet build backend/Backend.sln --no-restore
  -p:UseSharedCompilation=false -p:RazorLangVersion=Latest -m:1
  --verbosity minimal` passed with 0 warnings and 0 errors.
- `dotnet test backend/Tests/Tests.csproj --no-build --no-restore
  -p:RazorLangVersion=Latest --filter
  FullyQualifiedName~UserManagementLifecycleTests` passed 5 focused tests.
  The complete backend suite passed 113 tests.
- `.venv/bin/python -m pytest scripts/tests` passed all 30 repository script
  tests; Ruff check and format verification passed for the changed scripts.
- A built migrator applied the initial, existing financial-institution, and user
  management migrations to a fresh SQLite database. The migration-focused test
  also applied the user migration after inserting an existing account row and
  verified that row remained readable. Bootstrap invitation creation succeeded,
  and guarded development-user seeding was repeatable on two successive runs.
- `git diff --check` and targeted whitespace checks produced no errors.

The focused tests exercise serialized single-unit-of-work lifecycle behavior.
Parallel last-admin mutation attempts remain a Phase 2/3 transaction-boundary
test once identity resolution and administration endpoints are introduced.

## Phase 2: Identity Resolution and Backend Authorization

### Scope

- Retain existing Google JWT validation.
- Add a valid-provider-identity policy for the resolution endpoint only.
- Implement atomic existing-user resolution and invitation acceptance using
  validated `sub`, `email`, and `email_verified` claims.
- Add active-user, write-capable, and admin requirements/handlers.
- Resolve the application user once per request and cache it request-locally.
- Replace the subject-allowlist fallback with database-backed authorization,
  while retaining only the explicitly selected rollout compatibility behavior.
- Enforce read access for all roles and write access only for `Admin` and
  `Standard` through a centralized default policy.
- Keep health endpoints anonymous as they are today.
- Update test and development authentication claims/fixtures as necessary
  without weakening environment guards.

### Required identity tests

- [x] Existing active subject succeeds independent of a changed email.
- [x] Existing disabled subject is rejected.
- [x] Verified matching email accepts an invitation and persists `sub`.
- [x] Email matching uses the selected normalization rules.
- [x] Missing, false, or malformed `email_verified` is rejected.
- [x] Missing or mismatched email is rejected.
- [x] Subject and email collisions are rejected safely.
- [x] Concurrent first-login requests accept an invitation only once.
- [x] Failure responses do not reveal whether an email was invited.

### Required authorization tests

- [x] Anonymous request is `401` where authentication is required.
- [x] Valid but unprovisioned identity is `403` on normal endpoints.
- [x] Every role can use representative read endpoints.
- [x] `Admin` and `Standard` can use representative financial mutations.
- [x] `ReadOnly` receives `403` for every inventoried mutation category.
- [x] Only `Admin` can use administration policies.
- [x] Disablement and role changes affect the next request.
- [x] Readiness and liveness remain anonymous and functional.

### Acceptance criteria

- [x] Backend authorization no longer depends on a permanent production subject
  allowlist, except any documented temporary compatibility path.
- [x] A future mutation is denied to `ReadOnly` by default unless explicitly
  classified otherwise.
- [x] REST integration tests cover real database state transitions.
- [x] Existing financial REST behavior remains passing.

### Validation evidence

- Added `POST /authentication/resolve-user`, which accepts only a validated
  provider identity (`sub`, parseable email, and `email_verified=true`) and
  resolves or provisions the database user without exposing invitation state.
  Existing active users may refresh provider profile data; disabled users,
  uninvited identities, malformed claims, and subject/email collisions fail
  generically with `403`.
- Added a serializable SQLite transaction around identity resolution and a
  conditional pending-invitation claim so concurrent first-login requests can
  create at most one application user.
- Added request-local current-user resolution and centralized authorization:
  active users can read, `Admin` and `Standard` can mutate, `ReadOnly` is denied
  mutations by default, and the administrator policy is available for the
  administration API phase.
- Removed the backend's permanent `GOOGLE_ALLOWED_SUBJECTS` fallback and its
  production startup requirement. Google issuer, audience, lifetime, signing
  key, and algorithm validation remain unchanged. Frontend/Auth.js sign-in
  still uses the old allowlist until Phase 4, as planned.
- Updated test authentication fixtures to provision database users and added
  claim-aware JWT generation. Existing health, JWT-validation, and financial
  integration tests continue to exercise real SQLite state.
- `dotnet build backend/Backend.sln --no-restore
  -p:UseSharedCompilation=false -p:RazorLangVersion=Latest -m:1
  --verbosity minimal` passed with 0 warnings and 0 errors.
- `dotnet test backend/Tests/Tests.csproj --no-build --no-restore
  -p:RazorLangVersion=Latest --filter
  FullyQualifiedName~UserResolutionAuthorizationTests` passed 12/12 tests.
  The complete backend suite passed 125/125 tests.
- `git diff --check` passed; all changed C# files also follow the repository's
  no-final-newline convention.

## Phase 3: User and Invitation Administration API

### Scope

- Add API models, converters, controllers, and OpenAPI descriptions for the
  target API.
- Implement `/users/me` for every active role.
- Implement admin-only user listing, invitation listing/creation/revocation,
  role changes, disablement, and enablement.
- Return deliberate `401`, `403`, `404`, `409`, and `422` responses.
- Ensure administration operations create audit events atomically.
- Regenerate frontend TypeScript API contracts using repository scripts.
- Do not build the admin page in this phase.

### Required tests

- [x] `/users/me` returns database role/status and safe profile fields only.
- [x] Standard and read-only callers cannot list or administer users.
- [x] Duplicate pending invitation returns the selected conflict response.
- [x] Invitation revoke behavior is idempotent or rejects repeated transition
  consistently with the locked contract.
- [x] Admin can change roles and enable/disable users.
- [x] Last-admin actions return a conflict and preserve state.
- [x] Audit records are committed with successful operations and absent after
  failed transactions.
- [x] OpenAPI generation/check passes.

### Acceptance criteria

- [x] All target administration capabilities are accessible through REST.
- [x] No endpoint accepts caller-supplied actor identity.
- [x] Generated frontend types match the backend contract.

### Validation evidence

- Added safe user and invitation REST models plus converters. `UserModel` does
  not expose the immutable provider subject.
- Added `/users/me`, administrator-only user listing, role changes,
  enablement/disablement, invitation listing/creation, and invitation
  revocation. Every mutating administration route resolves the actor from the
  authenticated request, starts a serializable SQLite transaction, and commits
  the domain mutation and audit event together.
- Added deliberate `403`, `404`, `409`, and `422` response mappings for
  authorization, missing records, lifecycle conflicts, and domain validation.
- Added six REST integration tests covering safe current-user data,
  role/status authorization, invitation lifecycle and duplicate conflicts,
  last-administrator protection, audit behavior, and validation/not-found
  responses.
- `dotnet build backend/Backend.sln --no-restore
  -p:UseSharedCompilation=false -p:RazorLangVersion=Latest -m:1
  --verbosity minimal` passed with 0 warnings and 0 errors.
- `dotnet test backend/Tests/Tests.csproj --no-build --no-restore
  -p:RazorLangVersion=Latest --filter
  FullyQualifiedName~UserAdministrationEndpointTests` passed 6/6; the complete
  backend suite passed 131/131.
- `dotnet format ../backend/Backend.sln --verify-no-changes --no-restore
  --severity info` passed from `scripts/`; OpenAPI generation and
  `frontend_scripts.py verify-models` passed; Python script tests passed 31/31;
  and `git diff --check` plus the no-final-newline C# check passed.

The admin page, Auth.js handshake, frontend role handling, and production
cutover remain later phases. Phase 3 exposes the REST contract and does not
implement those consumers.

## Phase 4: Auth.js Handshake and Current-User Foundation

### Scope

- Replace Auth.js's local `GOOGLE_ALLOWED_SUBJECTS` sign-in decision with a
  server-side call to the backend identity-resolution endpoint using the Google
  ID token.
- Reject Auth.js sign-in when the backend denies resolution.
- Preserve the encrypted server-only ID-token flow and expiration behavior.
- Add a frontend current-application-user loader using `/users/me`.
- Introduce an authenticated application layout/route structure if needed so
  the shell receives role information without making the login page depend on
  an active application user.
- Add an access-denied experience for uninvited or disabled identities.
- Pass the application role into navigation and current-user presentation.
- Do not implement the full admin page or all read-only control changes yet.

### Required tests

- [x] Invited first login resolves the user and creates a usable session.
- [x] Existing active user login succeeds.
- [x] Uninvited and disabled identities do not receive usable application
  sessions.
- [x] Backend outages during sign-in fail closed with a useful generic UI.
- [x] `/api/auth/session` does not expose the Google ID token.
- [x] Authenticated server-side API requests still attach the ID token.
- [x] Login remains public and callback paths remain valid.
- [x] Development authentication remains usable only in the guarded local mode.

### Acceptance criteria

- [x] Auth.js no longer reads the permanent subject allowlist for authorization.
- [x] Backend resolution is the single source of provisioning truth.
- [x] Application layout has current database role/status available.
- [x] Frontend lint, type checking, and build pass or limitations are recorded
  precisely.

### Validation evidence

- Auth.js now sends the provider ID token to `POST /authentication/resolve-user`
  and rejects sign-in on denial or resolver failure. The frontend no longer
  parses or consults `GOOGLE_ALLOWED_SUBJECTS`; the backend resolver owns
  provisioning and active-user decisions.
- Added the server-only `/users/me` loader and authenticated root-layout
  handling. Active sessions receive the database display name, email, role, and
  status; disabled or otherwise denied sessions receive a generic access-
  unavailable view without loading the financial shell.
- Development authentication now emits the verified email claims required by
  the backend resolver while remaining guarded by `AUTH_MODE=development` and
  the backend Development environment.
- The identity-resolution suite passed 12/12 tests, covering existing active,
  disabled, invited, invalid-verification, collision, concurrent-first-login,
  and role authorization behavior.
- Frontend ESLint, Prettier, TypeScript (`npx tsc --noEmit`), and generated API
  model verification passed. The host `next build` stalled after beginning
  optimization in this restricted environment; the production Docker build
  completed the Next.js compile, TypeScript check, static generation, and route
  finalization successfully.
- The production container smoke test passed after migration and repeated
  development seeding. It verified backend resolution, `/users/me`, server-side
  authenticated API access, the database role in responsive navigation,
  browser session redaction, expired-token rejection, login/callback behavior,
  backend-outage fail-closed redirect and generic UI, backend restart recovery,
  and both Playwright tests (2/2).
- Python script tests passed 31/31; Ruff check and formatting passed; `git
  diff --check` passed; and the changed C# file follows the repository's
  no-final-newline convention. Repository-wide `dotnet format` could not produce
  a clean result because the existing workspace reports broad IDE0005 and
  dependent-project semantic errors; the changed backend compiled successfully
  in the production image and the focused backend tests passed.

## Phase 5: Admin UI and Read-Only Experience

### Scope

- Add an admin-only navigation item and `/admin/users` page.
- Display active/disabled users and pending/completed invitations.
- Add invitation creation, invitation revocation, role change, enable, and
  disable interactions with confirmations and useful errors.
- Warn about self-affecting operations and prevent obvious last-admin actions in
  the UI while retaining backend enforcement.
- Hide or disable every inventoried financial mutation control for read-only
  users.
- Handle backend `403` responses because role/status can change while a page is
  open.
- Optionally show user-focused summaries: active users by role, pending
  invitations, disabled users, and recent logins.

### Required tests

- [ ] Admin navigation/page is visible to admins.
- [ ] Admin navigation/page is absent and inaccessible to other roles.
- [ ] Admin can invite, change role, disable/enable, and revoke through the UI.
- [ ] Validation and conflict errors are understandable.
- [ ] Read-only user can navigate all read experiences.
- [ ] Read-only user sees no usable create/edit/delete/post/unpost/close/reopen/
  onboard controls from the Phase 0 inventory.
- [ ] A stale page handles a newly denied mutation without corrupting UI state.
- [ ] Responsive navigation still works.

### Acceptance criteria

- [ ] All user-management operations are usable without knowing Google subject
  IDs.
- [ ] Read-only behavior is coherent throughout the frontend.
- [ ] Frontend lint, type checking, build, and focused browser tests pass.

### Validation evidence

Not yet recorded.

## Phase 6: Deployment Cutover and End-to-End Proof

### Scope

- Back up and migrate a production-like copy of the SQLite database.
- Exercise bootstrap and any selected compatibility import.
- Validate admin first login, provisioning, standard access, read-only access,
  disablement, and last-admin protection through production-built containers.
- Remove `GOOGLE_ALLOWED_SUBJECTS` from backend/frontend runtime requirements,
  Compose, deployment configuration generation, examples, documentation, and
  tests.
- Verify that no compatibility importer is present. If the rollout decision is
  later reopened, record and enforce its separately approved removal deadline.
- Update deployment smoke tests to prove database-backed authentication and
  authorization.
- Update operator and developer documentation.

### Required end-to-end scenarios

- [ ] Anonymous protected-route redirect.
- [ ] Bootstrap admin invitation and first Google login.
- [ ] Admin creates standard and read-only invitations.
- [ ] Invited users activate on first matching verified-email login.
- [ ] Standard user completes a representative write/read persistence cycle.
- [ ] Read-only user reads data and is denied a direct mutation request.
- [ ] Admin role change affects the user's next request.
- [ ] Disabled user loses API access while an existing frontend session exists.
- [ ] Last active admin cannot be demoted or disabled.
- [ ] Browser-visible session omits the ID token.
- [ ] Restart/redeploy preserves users, invitations, roles, and audit records.
- [ ] Backup/restore preserves user-management state and authentication works
  against the restored database.

### Acceptance criteria

- [ ] No runtime component requires `GOOGLE_ALLOWED_SUBJECTS`.
- [ ] Production-like build, migration, startup, readiness, authentication,
  authorization, and persistence are proven.
- [ ] Local orchestration represents the relevant CI/deployment checks.
- [ ] Operational rollback is documented, including the database compatibility
  implications of the new migration.

### Validation evidence

Not yet recorded.

## Cross-Cutting Security Requirements

- Validate Google token issuer, audience, lifetime, signing key, and allowed
  algorithm before trusting identity claims.
- Preserve literal `sub` claim mapping.
- Never log ID tokens, OAuth secrets, or full bearer headers.
- Never serialize the ID token into the Auth.js session returned to the browser.
- Use signed-token claims rather than request-body identity fields.
- Match invitations only when `email_verified` is unambiguously true.
- Keep authorization decisions database-current; do not trust a long-lived role
  copied into an Auth.js cookie as the backend authority.
- Use generic login-denial messages to avoid invitation enumeration.
- Rate-limit identity resolution and administration endpoints consistently with
  the existing API policy.
- Keep development authentication rejected outside the Development environment.

## Cross-Cutting Validation Expectations

Use repository-native commands discovered from the current script modules.
Validation should include, as applicable:

- Focused unit and REST integration tests during each phase.
- Full backend tests after authorization or persistence changes.
- Migration against a copied existing SQLite database.
- OpenAPI generation/check after REST contract changes.
- Frontend lint, TypeScript checking, and production build after frontend work.
- Focused Playwright coverage for complete browser flows.
- Production-built container smoke tests for final signoff.
- `git diff --check` in every phase.

Do not collapse partial evidence into a blanket statement such as “all tests
pass.” Record exact commands, outcomes, and environmental blockers.

## Decision Log

Add durable decisions here. Do not rewrite previous entries without explaining
the superseding decision.

| Date | Phase | Decision | Reason |
|---|---|---|---|
| 2026-08-05 | Plan | Email matches first-login invitations; Google `sub` is the durable identity | Email is administratively usable but mutable; `sub` is stable |
| 2026-08-05 | Plan | Separate users and invitations | Invitation lifecycle and audit history are distinct from active users |
| 2026-08-05 | Plan | Backend is the authorization authority | Frontend visibility cannot enforce security and roles may change mid-session |
| 2026-08-05 | Plan | Default method-aware authorization protects mutations | Prevents a forgotten endpoint annotation from granting read-only writes |
| 2026-08-05 | Plan | Broad application metrics are deferred | User management and access auditing are the first-release objective |
| 2026-08-05 | Phase 0 | Invitations are non-expiring initially; completed invitation history is retained indefinitely | Avoids an unneeded notification/expiration workflow while preserving auditability |
| 2026-08-05 | Phase 0 | Use a short planned maintenance cutover and provision existing collaborators by email | The current subject allowlist has no durable invitation/profile model and must not create implicit administrators |
| 2026-08-05 | Phase 0 | Permit self role/status changes only when another active administrator remains | Supports deliberate administration while preserving the last-admin invariant |
| 2026-08-05 | Phase 0 | Fix the target REST routes and use existing ProblemDetails/ValidationProblemDetails conventions | Keeps generated contracts aligned with the established ASP.NET/OpenAPI surface |

## Open Questions

No Phase 0 identity, persistence, API, rollout, or frontend-inventory questions
remain unresolved. Any later change to these contracts must be recorded as a
superseding Decision Log entry before implementation proceeds.

## Handoff Log

Append one entry after every agent phase. Include the phase, files or subsystems
changed, exact validation, remaining limitations, and the next dependency-ready
phase.

### 2026-08-05 - Planning document created

- Phase: Plan preparation only.
- Changes: Added this implementation and iterative handoff document.
- Validation: Document reviewed against the current authentication, backend
  architecture, frontend navigation, migration, and deployment conventions.
- Limitations: No feature implementation or runtime validation performed.
- Next: Phase 0.

### 2026-08-05 - Phase 0 contracts locked

- Phase: Phase 0 complete.
- Changes: Resolved invitation expiration, history retention, deployment
  cutover, email normalization, enum/route/error contracts, SQLite acceptance
  strategy, self-administration behavior, and proposed implementation
  locations. Inventoried all current backend mutation routes and frontend
  mutation controls/actions.
- Validation: Repository convention inspection and targeted `rg` inventories;
  `git diff --no-index --check /dev/null user-management-plan.md` produced no
  whitespace errors. No application code was changed.
- Limitations: Persistence, migration, bootstrap, authorization, and runtime
  behavior are not implemented or proven yet.
- Next: Phase 1, persistence, domain lifecycle, migration, and bootstrap.

### 2026-08-05 - Phase 1 persistence and bootstrap complete

- Phase: Phase 1 complete.
- Changes: Added user, invitation, and administration-audit domain lifecycles;
  EF configurations and repositories; the generated user-management migration;
  serialized bootstrap/development seeding; a design-time context factory; the
  repository-native `bootstrap-admin` deployment command; and SQLite-backed
  lifecycle tests.
- Validation: Backend solution build passed with 0 warnings and 0 errors;
  focused user-management tests passed 5/5; the complete backend suite passed
  113/113; Python script tests passed 30/30; the migrator applied all existing
  and user-management migrations and repeated guarded development seeding
  successfully; script Ruff checks passed; and whitespace checks passed.
- Limitations: Production authentication, identity resolution, REST
  administration, frontend session/UI behavior, and deployment cutover remain
  unimplemented. Parallel last-admin mutation testing is deferred until the
  identity/API transaction boundary exists in Phase 2/3.
- Next: Phase 2, identity resolution and backend authorization.

### 2026-08-05 - Phase 2 identity and backend authorization complete

- Phase: Phase 2 complete.
- Changes: Added validated provider-identity policy and
  `/authentication/resolve-user`; atomic SQLite identity resolution and
  conditional invitation claiming; request-local current-user resolution;
  active, write-capable, administrator, and method-aware application policies;
  removal of the backend subject-allowlist fallback; and database-backed JWT
  authorization fixtures.
- Validation: Focused identity and authorization integration tests passed
  12/12; the complete backend suite passed 125/125; the backend solution build
  passed with 0 warnings and 0 errors; concurrent first-login behavior accepted
  exactly one request; health endpoints remained anonymous; and all changed C#
  files were checked for the repository's no-final-newline convention.
- Limitations: Auth.js still performs its local subject-allowlist sign-in
  decision, administration REST endpoints do not yet exist, and frontend role
  handling is not implemented. Those are Phase 3/4 dependencies, not a Phase 2
  authorization bypass.
- Next: Phase 3, user and invitation administration API.

### 2026-08-05 - Container smoke authentication follow-up

- Phase: Phase 2 validation follow-up.
- Changes: Updated `scripts/container_scripts.py` to provision the deterministic
  development user before starting the backend and frontend images, and added a
  regression test for the migrator environment contract.
- Validation: Script tests passed 31/31; Ruff, formatting, compilation, and
  whitespace checks passed; the production-built container smoke test passed,
  including backend persistence, frontend session/API flow, and both Playwright
  tests (2/2).
- Limitations: The smoke path uses guarded development authentication by design;
  Google production cutover and administration endpoints remain Phase 3-6 work.
- Next: Phase 3, user and invitation administration API.

### 2026-08-05 - Phase 3 user administration API complete

- Phase: Phase 3 complete.
- Changes: Added REST models and OpenAPI descriptions for users and invitations;
  `/users/me`; administrator user listing, role, enable, and disable routes;
  invitation listing, creation, and revocation routes; serializable mutation
  transactions; and REST integration coverage.
- Validation: Focused administration tests passed 6/6; the complete backend
  suite passed 131/131; the backend build passed with 0 warnings and 0 errors;
  backend format verification, OpenAPI generation/verification, Python script
  tests (31/31), and whitespace checks passed.
- Limitations: Auth.js provisioning handshake, frontend role-aware behavior,
  admin UI, and production cutover remain unimplemented.
- Next: Phase 4, Auth.js handshake and current-user frontend foundation.

### 2026-08-05 - Phase 4 Auth.js handshake and current-user foundation complete

- Phase: Phase 4 complete.
- Changes: Replaced the frontend subject allowlist sign-in decision with the
  backend identity-resolution handshake; added generic denial/error handling;
  added the server-only current-user loader and access-denied view; threaded
  database role information through the responsive navigation; added required
  development resolver claims; and extended container smoke coverage for
  resolver outages and role presentation.
- Validation: Production backend, frontend, and migrator images built
  successfully; focused identity-resolution tests passed 12/12; Python script
  tests passed 31/31; frontend lint, formatting, type checking, model
  verification, and Docker production build passed; and the final container
  smoke test passed, including its outage/restart check and both Playwright
  tests (2/2).
- Limitations: No browser test uses a live Google credential, so Google
  provider UX remains represented by the shared Auth.js callback and backend
  identity-resolution tests. The host-local Next.js build and repository-wide
  `dotnet format` remain environment-limited; the Docker production build and
  focused backend tests provide the corresponding runtime/compiler evidence.
- Next: Phase 5, admin UI and read-only application experience.
