# Spending Transaction Plan

## Goal

Enhance the spending transaction domain model to support a single spending transaction crediting more than one destination at a time.

The desired model should:

- preserve the current mental model of spending as money leaving one source
- allow the spent amount to be split across multiple credited destinations
- avoid introducing individual spending lines when they are not needed

## Core Model

Model a spending transaction as:

- one debit source at the transaction level
- one or more spending destinations

Suggested shape:

```csharp
public class SpendingTransaction : Transaction
{
    public AccountId DebitAccountId { get; private set; }
    public DateOnly? DebitPostedDate { get; internal set; }

    public IReadOnlyCollection<SpendingDestination> Destinations => _destinations;
}

public class SpendingDestination
{
    public AccountId? CreditAccountId { get; private set; }
    public string? Location { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly? CreditPostedDate { get; internal set; }
    public IReadOnlyCollection<FundAmount> FundAssignments => _fundAssignments;
}
```

## Intent Of The Model

The transaction-level debit account represents where the money is coming from.

The destination collection represents where the spent money is going.

Each destination can be either:

- a tracked account receiving credit, or
- an external location that is not represented as an account

This keeps the model aligned with the existing concept of spending while allowing the credit side to fan out to multiple targets.

## Invariants

The model should enforce:

- exactly one debit account at the transaction level
- at least one destination
- all destination amounts are positive
- each destination has either `CreditAccountId` or `Location`, but not both
- `Transaction.Amount == sum(Destinations.Amount)`

## Why This Model

This approach:

- solves the main requirement of splitting a spending transaction across multiple credited destinations
- avoids unnecessary complexity from introducing spending lines
- preserves a simple source-to-destinations mental model
- keeps the many-sided part of the transaction focused only where it is needed

## Example

Transaction:

- Debit account: Checking
- Total amount: `1000.00`

Destinations:

- Landlord location: `700.00`
- Utilities account: `200.00`
- Reimbursement receivable account: `100.00`

Validation:

- destination total = `700.00 + 200.00 + 100.00 = 1000.00`
- destination total matches transaction amount

## Destination Modeling

Each `SpendingDestination` should represent one credited target.

Recommended fields:

- optional `CreditAccountId`
- optional external `Location`
- `Amount`
- optional `CreditPostedDate`
- `FundAssignments`

The destination should not allow both an account and a location at the same time.

## Location Design Choice

Do not keep a single transaction-level `DestinationLocation` once multi-destination support is introduced.

Instead, place destination details on each `SpendingDestination`.

This avoids ambiguity when a single spending transaction contains multiple credited targets with different meanings.

## Fund Assignment Direction

Move fund assignments from the transaction level to each destination.

This allows funds to be associated with the specific portion of spending being credited to each destination rather than with the whole transaction.

Each destination should validate that its own fund assignments do not exceed its amount, following the same style used elsewhere in the transaction model.

## Posting Behavior

Posting should operate independently on:

- the debit account at the transaction level
- each credited destination account

That means:

- the debit side keeps a transaction-level posted date
- each destination account keeps its own posted date
- destinations represented only by external locations do not participate in account posting

## Migration Direction

Recommended migration path:

1. Keep the existing debit account on `SpendingTransaction`.
2. Replace the single `CreditAccountId` and transaction-level `DestinationLocation` with a collection of `SpendingDestination`.
3. Move fund assignments from the transaction level to destination-level assignments.
4. Update create and update API contracts to accept destination collections.
5. Update validation so destination totals must equal the transaction amount.
6. Update posting and balance application logic to iterate over destinations.
7. Update the spending transaction UI to allow adding and editing multiple destinations.

## UI Direction

The create and update spending forms should evolve to support:

- one debit/source account selector
- a repeatable list of destinations
- per-destination choice of tracked account or external location
- per-destination amount entry
- per-destination fund assignments
- a running remaining amount indicator until destination totals match the transaction amount

## Open Design Notes

- Keep the transaction-level source side singular unless a real use case appears for multi-source spending.
- Use a repeatable destination editor rather than a pair-style account picker once multiple destinations are supported.
- Keep the destination object small and focused so it remains easy to reason about in balance and posting logic.
