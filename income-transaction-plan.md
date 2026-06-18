# Income Transaction Plan

## Goal

Enhance the income transaction domain model to support paycheck-style entries where:

- gross income can come from multiple income components
- net amounts can be deposited into multiple tracked accounts
- deductions such as taxes and insurance can be captured without modeling them as persistent accounts

## Core Model

Model an income transaction as three collections:

1. `IncomeLines`
2. `Deposits`
3. `Deductions`

Suggested shape:

```csharp
public class IncomeTransaction : Transaction
{
    public AccountId? DebitAccountId { get; private set; }
    public string? SourceLocation { get; private set; }

    public IReadOnlyCollection<IncomeLine> IncomeLines => _incomeLines;
    public IReadOnlyCollection<IncomeDeposit> Deposits => _deposits;
    public IReadOnlyCollection<IncomeDeduction> Deductions => _deductions;

    public decimal GrossAmount => _incomeLines.Sum(x => x.Amount);
    public decimal NetAmount => _deposits.Sum(x => x.Amount);
}

public class IncomeLine
{
    public IncomeLineType Type { get; private set; }
    public string Label { get; private set; } = "";
    public decimal Amount { get; private set; }
}

public class IncomeDeposit
{
    public AccountId AccountId { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly? PostedDate { get; internal set; }
    public IReadOnlyCollection<FundAmount> FundAssignments => _fundAssignments;
}

public class IncomeDeduction
{
    public IncomeDeductionType Type { get; private set; }
    public string Label { get; private set; } = "";
    public decimal Amount { get; private set; }
}
```

## Intent Of Each Collection

### Income Lines

Income lines describe why income exists.

Examples:

- salary
- bonus
- commission
- reimbursement
- employer 401k match
- other earnings

This allows the gross amount of a paycheck to be broken out into meaningful earning categories for reporting and analysis.

### Deposits

Deposits describe where income actually lands in tracked accounts.

Examples:

- checking account deposit
- savings account deposit
- retirement account contribution

Only deposits should affect account balances and posting workflows.

### Deductions

Deductions describe amounts withheld or diverted from gross pay that should not require persistent account balances.

Examples:

- federal tax withholding
- state tax withholding
- health insurance premium
- dental insurance premium
- employee retirement contribution

Deductions are classification and reporting data, not tracked ledger balances.

## Invariants

The model should enforce:

- at least one income line
- at least one deposit
- all amounts are positive
- `GrossAmount == sum(IncomeLines.Amount)`
- `GrossAmount == sum(Deposits.Amount) + sum(Deductions.Amount)`

## Why This Model

This approach:

- supports full paycheck entry without forcing taxes or insurance into fake accounts
- preserves real account tracking only for balances the user actually cares about
- distinguishes earnings from allocations
- supports multi-account income deposits cleanly
- allows employer match income to be represented separately from salary

## Example Paycheck

Income lines:

- Salary: `2500.00`
- Employer 401k Match: `150.00`

Deposits:

- Checking: `2200.00`
- Retirement Account: `300.00`

Deductions:

- Federal Tax: `350.00`
- Health Insurance: `150.00`

Validation:

- Gross amount = `2500.00 + 150.00 = 2650.00`
- Deposits + deductions = `2200.00 + 300.00 + 350.00 + 150.00 = 3000.00`

Note: the example above is intentionally illustrative; real entered values must satisfy the balancing rules exactly.

## API Direction

The current income transaction API uses:

- a single `CreditAccountId`
- an optional `DebitAccountId`
- transaction-level `FundAssignments`

The planned direction is to evolve that into:

- `IncomeLines`
- `Deposits`
- `Deductions`
- deposit-level `FundAssignments`

This keeps funds attached to the specific deposited account amount rather than to the income transaction as a whole.

## Posting Behavior

Posting should operate only on deposits.

That means:

- each deposit has its own `PostedDate`
- posting an income transaction may need to occur per deposit account
- deductions do not participate in posting
- income lines do not participate in posting

## Migration Direction

Recommended migration path:

1. Replace single-account credit modeling in `IncomeTransaction` with a collection of deposits.
2. Introduce `IncomeLine` and `IncomeDeduction` value objects/entities.
3. Move fund assignments from the transaction level to the deposit level.
4. Update create and update API contracts to accept income lines, deposits, and deductions.
5. Update validation and balance application logic so only deposits affect account balances.
6. Update the UI to support paycheck-style entry.

## UI Direction

The create and update income forms should evolve to support three editable sections:

- income lines
- deposits
- deductions

Useful UX behaviors:

- auto-calculate and display gross amount
- auto-calculate and display total deductions
- auto-calculate and display net deposited amount
- show remaining unallocated amount if deposits and deductions do not yet balance to gross
- allow multiple deposit accounts in a single income transaction

## Open Design Notes

- Keep `IncomeLine`, `IncomeDeposit`, and `IncomeDeduction` separate even if some lines often correspond closely to deposits.
- Avoid forcing a 1:1 mapping between income lines and deposits initially.
- Continue allowing either an external `SourceLocation` or an optional external/untracked debit-side source concept.
- Consider enums plus freeform labels for line and deduction types so reporting remains structured without losing flexibility.
