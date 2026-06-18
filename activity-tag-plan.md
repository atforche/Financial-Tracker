# Activity Tag Plan

## Goal

Introduce a first-class tag concept that groups transaction activity for reporting and analysis without tracking balances.

This concept should:

- replace freeform transaction location fields
- support consistent grouping across income and spending activity
- allow users to analyze activity such as store spending, 401k contributions, employer match income, insurance deductions, and similar patterns
- remain balance-neutral and independent of posting logic

## Core Idea

Use a reusable domain entity called `ActivityTag` as the primary classification concept for analyzable transaction parts.

Instead of using freeform fields like `SourceLocation` or `DestinationLocation`, the new transaction model should reference a single required tag on each meaningful item.

Recommended examples:

- each `IncomeLine` has one required tag
- each `IncomeDeduction` has one required tag
- each `SpendingDestination` has one required tag

This makes the tag the canonical attribution for that line or destination.

## Core Model

Suggested shape:

```csharp
public class ActivityTag : Entity<ActivityTagId>
{
    public string Name { get; internal set; } = "";
    public ActivityTagType Type { get; internal set; }
    public string? Description { get; internal set; }
    public bool IsSystem { get; internal set; }
}

public enum ActivityTagType
{
    Location,
    Income,
    Deduction,
    Employer,
    Merchant,
    Contribution,
    Custom
}
```

Suggested usage in transaction objects:

```csharp
public class IncomeLine
{
    public IncomeLineType Type { get; private set; }
    public ActivityTagId TagId { get; private set; }
    public decimal Amount { get; private set; }
}

public class IncomeDeduction
{
    public IncomeDeductionType Type { get; private set; }
    public ActivityTagId TagId { get; private set; }
    public decimal Amount { get; private set; }
}

public class SpendingDestination
{
    public AccountId? CreditAccountId { get; private set; }
    public ActivityTagId TagId { get; private set; }
    public decimal Amount { get; private set; }
}
```

## Why Tags Should Replace Location Strings

Replacing freeform locations with tags provides:

- consistent naming
- cleaner filtering
- better long-term reporting
- support for both place-based and concept-based attribution

Examples of problems avoided:

- `Target` versus `target` versus `Target Store`
- mixing location names with conceptual labels such as `401k Contribution`
- duplicated spelling variants that break analysis

## Tag Meaning

An activity tag is not a balance bucket and not a posting target.

It is a classification mechanism that answers questions such as:

- where was this spent?
- what kind of payroll deduction was this?
- what kind of income was this?
- which employer or merchant was involved?

Tags should remain:

- reusable
- user-manageable
- analytics-focused
- independent of balance calculations

## Required Tagging Rules

Recommended validation rules:

- each `IncomeLine` must have exactly one tag
- each `IncomeDeduction` must have exactly one tag
- each `SpendingDestination` must have exactly one tag
- tag names must be unique
- referenced tags must exist

Optional future expansion:

- allow secondary tags later if needed
- allow transaction-level tags for broad context

The initial implementation should stay simple and use one required primary tag per analyzable item.

## Examples

### Income

Income lines:

- Salary, tag `Employer Payroll`
- Employer Match, tag `Employer Match`

Deductions:

- Retirement Contribution, tag `401k Contribution`
- Insurance, tag `Health Insurance`

### Spending

Spending destinations:

- Credit card payment for store purchase, tag `Target`
- External destination, tag `Landlord`
- Utilities account credit, tag `Utilities`

## Relationship To Existing Models

The current model uses:

- `SourceLocation` on income transactions
- `DestinationLocation` on spending transactions

The planned direction is:

- remove freeform location strings from the future split-based models
- use `ActivityTagId` instead on the new income and spending child objects

This change should happen alongside the broader transaction redesign:

- income transactions with `IncomeLines`, `Deposits`, and `Deductions`
- spending transactions with a collection of `SpendingDestination`

## Reporting And UI Value

This model enables:

- tag detail pages
- tag-based transaction history
- tag trends over time
- merchant/store analysis
- 401k contribution analysis
- employer match analysis
- insurance deduction analysis

Useful summaries for a tag:

- total amount in a date range
- transaction count
- first and last activity
- income versus spending totals
- activity by accounting period

## API Direction

Recommended API changes:

- add tag CRUD models and endpoints
- add `ActivityTagId` to the relevant income and spending create/update request objects
- include tag details in transaction response models where line/destination data is returned
- support filtering transaction queries by tag

## Domain And Persistence Direction

Recommended implementation steps:

1. Add `ActivityTag` domain entity, repository, service, and REST models.
2. Add database tables for tags.
3. Add `ActivityTagId` references to:
   - `IncomeLine`
   - `IncomeDeduction`
   - `SpendingDestination`
4. Update transaction creation and update validation to require tags.
5. Add query support for retrieving activity by tag.

## Migration Strategy

Recommended migration path:

1. Introduce `ActivityTag` before removing any existing location fields.
2. Create tags for existing known locations or concepts as needed.
3. Update the new transaction forms to use required tag pickers.
4. Migrate users away from freeform location entry.
5. Remove or deprecate `SourceLocation` and `DestinationLocation` once the new models are in place.

## Naming Recommendation

Recommended name: `ActivityTag`

Why:

- broader than `Location`
- clearer than plain `Tag`
- flexible enough for stores, employers, deductions, and contribution types
- communicates that the concept exists for transaction activity analysis

## Open Design Notes

- Do not let tags influence balances, funds, or posting behavior.
- Keep tags required only where they are the primary attribution mechanism.
- Avoid modeling too many tag types up front; the enum can start small and expand later.
- Consider allowing system tags later for built-in categories if that becomes useful.
