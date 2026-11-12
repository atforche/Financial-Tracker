# Balance adjustments: design notes

Status: implemented on branch balance-adjustments (2026-11-12), uncommitted.

## Problem

Investment, retirement, and debt accounts change value without a matching statement transaction: market movement,
interest, and similar. Roughly weekly, the user reconciles each of these accounts by entering a transaction whose
amount is whatever makes the app's balance match the institution's balance.

Today this is painful in two ways:

1. **Entry needs a calculator.** The user looks up the app's balance, subtracts it from the institution's balance, and
   types the difference.
2. **The data model forces a fake counterparty.** These are entered as Account transactions, which need something on
   the other side, so "mirror" locations exist purely to fill it (a Fidelity location for the Fidelity accounts, a
   location named after the loan for the loan). The account also has to switch sides depending on whether the value
   went up or down. Those locations then show up as places money went on the Locations pages.

## Decisions so far

### A new transaction type: Balance adjustment

A sixth transaction type alongside Spending, Income, Account, Fund, and Refund.

| Field        | Notes                                                                       |
| ------------ | --------------------------------------------------------------------------- |
| Account      | Exactly one account; Investment, Retirement, or Debt only                    |
| Date         | The transaction date, which is also its posted date                         |
| Amount       | Signed change to the displayed balance (see below)                          |
| Description  | Free text, for example "Change in value" or "Interest"                      |
| Counterparty | None: no location, no other account                                         |
| Funds        | None: these account types are untracked, so no fund assignments             |

- **Always posted on its date.** No separate posted date, never pending, no Post or Unpost actions. Changing the date
  moves the posting with it. In the domain, the posted date for its account is `Date`, and `SetPostedDate` and
  `ClearPostedDates` have nothing to do.
- **The amount is a signed change to the balance as it is displayed.** +$500 on the Mortgage means $500 more owed, and
  +$500 on Brokerage means $500 more value. It is applied directly to the posted balance, bypassing the debit and
  credit rules for debt accounts. The form's Change is exactly the stored amount: the entered balance minus the posted
  balance. This is the first signed amount in the app. The list shows it like other amounts (a minus for decreases, no plus), and the form and detail page show the change with its sign. (Rejected:
  a positive amount plus an increase/decrease flag, a second field that every consumer would have to combine.)
- **Rules.** Like other transactions, an adjustment dated before the account's opening date is rejected, and closed
  accounting periods follow the existing rules. Editing or deleting an older adjustment shifts the balance after it.
  Later adjustments are not recalculated, which follows from storing the change.
- **Stores the change, not the observed balance.** We considered a "balance checkpoint" that would store the
  institution balance and derive the change. We rejected it: the app's balance history is defined by posting dates, and
  an observed balance stops meaning anything once something later posts back before it. An adjustment is one more
  posted event in that history, the same as any other transaction. If something later posts backdated, the next
  reconcile absorbs the difference, as it does today.

### Entry: the normal Add Transaction flow

No separate action. Balance adjustment is one more type in the regular Add Transaction flow, with a form of its own.
Create and edit share that form: you always enter the balance you want, and the app works out the change.

```
Balance adjustment
  Account                  [Brokerage          ]   (Investment, Retirement, Debt only)
  Date                     [Nov 11, 2026      ]
  Balance at institution   [$ 48,213.07       ]
  Description              [Change in value   ]
  ----------------------------------------------
  Posted balance on Nov 11    $47,880.12
  Change                      +$332.95
                                [Cancel] [Save]
```

- As with every Add Transaction link, only the period is preselected. There is no account default.
- The change is the institution balance minus the account's **posted** balance through the chosen date. The posted
  balance comes from the existing balance endpoints (`/accounts/date-range`) and refreshes when the account or date
  changes.
- **Editing** opens the same form. The posted balance excludes the adjustment being edited, and the institution
  balance starts as that posted balance plus the stored change. If something was backdated before the adjustment after
  it was saved, the starting balance differs from what was originally entered. That is expected, because only the
  change is stored.
- Pending transactions on the account are not included in the posted balance, and the form does not call them out.
- The description defaults to the last one used for the chosen account.
- A change of $0.00 disables Save.

### Existing data: migrate and remove the mirror locations

There is a single deployment, so a data migration targeted at this data is acceptable.

Mirror locations can't be identified by a pattern. Real employer and pension contributions also look like
"location to retirement account". So the migration uses an **explicit list of location names**.

The list from the debug database (the latest production backup, via the query below) is **CRI, Equitable, Fidelity,
Voya**, which are converted and deleted. **Rocket Mortgage** is a real counterparty: 3 Escrow to Rocket Mortgage
payments use it. So its 4 Mortgage (Debt) transactions are converted, and the location is kept. **Saline Area Schools**
and **State of Michigan** fit the pattern but are real contributions, so they are excluded entirely.

| Location            | Account(s)                                      | Txns       | Used elsewhere |
| ------------------- | ----------------------------------------------- | ---------- | -------------- |
| CRI                 | Federal Student Loans (Debt)                    | 14         | 0              |
| Equitable           | Equitable 403b                                  | 14         | 0              |
| Fidelity            | Fidelity 401k, Fidelity IRA, Fidelity Personal  | 14, 19, 19 | 0              |
| Rocket Mortgage     | Mortgage (Debt)                                 | 4          | 0              |
| Voya                | Voya 401k, Voya 457                             | 14, 14     | 0              |
| Saline Area Schools | Equitable 403b, Voya 401k, Voya 457             | 1, 2, 2    | 7              |
| State of Michigan   | MI Pension                                      | 1          | 1              |

Planned migration steps:

1. Add the Balance adjustment type to the schema.
2. Convert every Account transaction between a listed location and an Investment, Retirement, or Debt account into a
   Balance adjustment. Keep its id, description, and amount. The sign comes from the effect the transaction had on the displayed balance,
   not from which side the account was on. For Investment and Retirement accounts, location to account is positive and
   account to location is negative. For Debt accounts it is reversed: location to account (for example CRI to Federal
   Student Loans) lowered the amount owed, so it becomes negative. The date becomes the account side's
   posted date.
3. Delete the listed locations, except Rocket Mortgage.
4. **Fail the whole migration, rather than skipping anything, if** a location to be deleted is used by any other
   transaction,
   a matching transaction is still pending, or a matching transaction has more than one destination.

Balances and history are unchanged, because each adjustment hits the same account, on the same posted date, by the same
amount.

Candidate query (read-only). Run against production to confirm the list:

```sql
WITH pairs AS (
  -- Location -> account (increase)
  SELECT t.Id AS TransactionId, t.AccountTransaction_SourceLocationId AS LocationId, d.CreditAccountId AS AccountId
  FROM Transactions t JOIN AccountTransactionDestinations d ON d.AccountTransactionId = t.Id
  WHERE t.AccountTransaction_SourceLocationId IS NOT NULL AND d.CreditAccountId IS NOT NULL
  UNION ALL
  -- Account -> location (decrease)
  SELECT t.Id, d.LocationId, t.AccountTransaction_DebitAccountId
  FROM Transactions t JOIN AccountTransactionDestinations d ON d.AccountTransactionId = t.Id
  WHERE d.LocationId IS NOT NULL AND t.AccountTransaction_DebitAccountId IS NOT NULL
)
SELECT l.Name AS Location, a.Name AS Account, a.Type AS AccountType,
       COUNT(DISTINCT p.TransactionId) AS Transactions,
       (SELECT COUNT(*) FROM Transactions WHERE IncomeTransaction_SourceLocationId = l.Id)
     + (SELECT COUNT(*) FROM RefundTransactionSources WHERE LocationId = l.Id)
     + (SELECT COUNT(*) FROM SpendingTransactionDestinations WHERE LocationId = l.Id) AS OtherUses
FROM pairs p
JOIN Locations l ON l.Id = p.LocationId
JOIN Accounts a ON a.Id = p.AccountId
WHERE a.Type IN ('Investment', 'Retirement', 'Debt')
GROUP BY l.Id, a.Id
ORDER BY l.Name, a.Name;
```

## Resolved questions

- The dev database is the latest production backup, so the location list stands.
- The name is "Balance adjustment".
- Editing uses the same enter-the-balance flow as creating.

### Transaction forms: type dropdown in the details frame

The Spending, Refund, Income, Account, Fund tabs at the top of the create form are replaced. With a sixth type, the
toggle row no longer fits on a phone.

```
Details
  Accounting Period   [Nov 2026        ]
  Date                [Nov 11, 2026    ]
  Description         [Change in value ]
  Type                [Balance adjustment v]
----------------------------------------------
  <frames for the selected type>
```

- **Type is the last field of the details frame**, not a separate control between frames.
- **The shared details are filled in first.** Period, date, and description are already shared state in
  `CreateTransactionForm.tsx`, so they survive a type change. Only the frames below the details change.
- **Spending is the default type**, as with the tabs today.
- **On the edit and detail pages, the type is shown but can't be changed.** Each type has its own update request, so
  changing the type on update isn't supported. Every page has the same layout: details including the type, then the
  frames for that type.
- **This fits Balance adjustment.** Its posted-balance lookup uses the shared date, so the change shows as soon as an
  account is picked.

## Effect on the existing types

- **Account transactions stay as they are.** After the migration, the Account transactions left are:
  - card payments (Standard to Credit card)
  - transfers (Standard to Standard)
  - employer and pension contributions (location to Retirement)
  - escrow disbursements (Escrow to location)

  The last two only touch untracked accounts. Spending must debit a tracked account and Income needs a tracked
  destination, so the one-sided location form of Account transactions is still needed. We considered moving those
  cases into Spending and Income and decided against it.
- **Account and Fund transactions stay separate.** We considered merging them into a single Transfer type. They share
  only the shape "one source, several destinations": Account transactions move account balances, have posted dates,
  allow locations, and enforce the tracked/untracked rules, while Fund transactions only move fund balances and never
  post. A merged type would branch on accounts versus funds in every rule. A single "Transfer" entry in the frontend is
  still possible later without changing the backend.
- Spending, Income, Fund, and Refund transactions don't change.

## Deferred

- **Breaking an account's change into contributions vs. change in value** on the account and period pages. Balance
  adjustments are the natural change-in-value bucket, but that reporting is out of scope here.

## Implementation notes

- **Backend.** Transactions are a single-table hierarchy with a `Type` discriminator
  (`Data/Transactions/TransactionConfiguration.cs`). The new type needs:
  - a `TransactionType` value
  - a domain class with its service and create and update requests, overriding the affected-account, affected-location,
    posted-date, and balance-application hooks on `Transaction`
  - EF mapping and a migration; the database is SQLite, and earlier migrations already include raw-SQL data steps
  - REST models and converters (`Rest/Transactions/TransactionConverter.cs`, `TransactionTypeConverter.cs`)
  - support in the transaction queries, sorts, and type filter
  - tests
- **Frontend.** The new pieces are:
  - one create and edit form for the type, built around entering a balance
  - detail page display and list rendering
  - the transaction type filter
  - replacing the type tabs with a Type field in the details frame (create, edit, detail pages)
  - hiding post and unpost actions for this type
- **Scope.** About one existing transaction-type slice, plus the migration. This work goes on its own branch,
  `balance-adjustments`, separate from `ux-fixes`.
