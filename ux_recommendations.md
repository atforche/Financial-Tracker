# UI Accessibility Review

## Goal

Reduce the number of screens and route transitions required to complete common tasks in the frontend.

## Summary

The frontend currently feels deeply nested because common work is spread across many page-level routes. Users often move through a pattern like list -> detail -> action page for routine tasks such as edit, delete, close, reopen, post, or unpost. The app already has strong route helpers and breadcrumbs, but those patterns are compensating for depth rather than reducing it.

## Findings

### 1. High-frequency workflows are missing from persistent navigation

The left navigation exposes only Overview, Accounting Periods, Accounts, and Funds. Transactions and Goals, which are frequent user tasks, are only surfaced through overview quick actions instead of global navigation.

Evidence:

- `frontend/app/NavigationLinks.tsx`
- `frontend/overview/OverviewHero.tsx`
- `frontend/overview/OverviewQuickActions.tsx`

Impact:

- Users must return to Overview to discover or start common actions.
- Core workflows feel hidden rather than directly accessible.

### 2. Routine actions are implemented as separate full-page routes

Create, update, delete, close, reopen, post, and unpost are all modeled as dedicated pages. This is especially visible for transactions and accounting periods.

Evidence:

- `frontend/transactions/routes.ts`
- `frontend/accounting-periods/routes.ts`
- `frontend/accounting-periods/AccountingPeriodView.tsx`
- `frontend/accounts/AccountView.tsx`
- `frontend/funds/FundView.tsx`
- `frontend/transactions/CreateTransactionForm.tsx`

Impact:

- Quick actions require multiple full route changes.
- Users lose list context, search state, and scroll position.
- The UI feels heavier than the task requires.

### 3. Accounting period detail is acting like four nested screens inside one page

The accounting period detail page loads funds, goals, accounts, and transactions, but only one collection is visible at a time through a toggle state.

Evidence:

- `frontend/accounting-periods/AccountingPeriodView.tsx`
- `frontend/accounting-periods/AccountingPeriodViewListFrames.tsx`

Impact:

- Users cannot scan adjacent information while working.
- Switching context inside the period page feels like navigating through sub-screens.
- The page behaves more like a router container than a workspace.

### 4. Switching period subviews drops user context

The accounting period view rewrites search params when users switch display sections, instead of preserving section-specific search, sort, and page state.

Evidence:

- `frontend/accounting-periods/AccountingPeriodViewListFrames.tsx`
- `frontend/accounting-periods/accounts/AccountingPeriodAccountListFrame.tsx`
- `frontend/accounting-periods/funds/AccountingPeriodFundListFrame.tsx`
- `frontend/accounting-periods/transactions/AccountingPeriodTransactionListFrame.tsx`

Impact:

- Users need to rebuild their place after each switch.
- The page feels fragile and stateful in the wrong way.
- Back/forward navigation is less predictable.

### 5. Most lists force a view-first workflow instead of action-in-place

Many list frames expose a single row action labeled View. Users then navigate to another page before they can edit, delete, or complete a routine status change.

Evidence:

- `frontend/accounting-periods/transactions/AccountingPeriodTransactionListFrame.tsx`
- `frontend/accounting-periods/accounts/AccountingPeriodAccountListFrame.tsx`
- `frontend/accounting-periods/funds/AccountingPeriodFundListFrame.tsx`
- `frontend/accounts/AccountTransactionListFrame.tsx`
- `frontend/funds/FundTransactionListFrame.tsx`

Impact:

- Simple maintenance actions take too many clicks.
- Users repeatedly bounce between list and detail pages.
- The interaction model encourages drilling down rather than staying oriented.

## Recommended Actions

### Priority 1: Expand persistent navigation around common work

Add direct navigation entries for Transactions and Goals. Also consider a current-period shortcut when an accounting period is open.

Why this comes first:

- It improves discoverability immediately.
- It reduces unnecessary returns to the overview page.
- It is likely the lowest-effort structural improvement.

### Priority 2: Replace action pages with dialogs or side sheets

Move delete, close, reopen, post, and unpost into confirmations launched from the current page. Where practical, move create and edit into dialogs or drawers as well.

Why this comes second:

- It removes the largest source of avoidable route transitions.
- It preserves table context, filters, and scroll position.
- It will make the UI feel materially flatter without changing domain logic.

Suggested starting point:

- Accounting period close/reopen/delete
- Transaction post/unpost/delete
- Account and fund delete

### Priority 3: Redesign accounting period detail into a workspace

Treat the accounting period page as a working surface rather than a detail page with hidden sections. Keep funds, goals, accounts, and transactions visible through tabs with counts, multi-panel layout, or progressive disclosure that preserves context.

Why this matters:

- The accounting period page is the densest nesting hotspot.
- It is the point where many related workflows converge.
- Improving this page will reduce both navigation depth and cognitive load.

### Priority 4: Preserve section state when switching within accounting periods

Keep search, sort, and pagination state per section instead of resetting state when the user changes display mode.

Why this matters:

- It makes the page feel stable.
- It reduces repetitive user work.
- It improves the usefulness of a tabbed or workspace-style design.

### Priority 5: Add inline row actions to list frames

Allow users to perform the most common actions directly from list rows. For example:

- Transactions: edit, post/unpost, delete
- Accounts: edit, delete
- Funds: edit, delete
- Accounting periods: open, close/reopen, delete

Why this matters:

- It shortens the common path from list -> detail -> action to list -> action.
- It keeps users anchored in the collection they are working through.

### Priority 6: Introduce a master-detail interaction pattern

For accounts, funds, and transactions, keep the list visible while showing details in a side panel, drawer, or adjacent pane.

Why this matters:

- Users can inspect and act without losing their place.
- It supports faster comparison and repeated edits.
- It is a better fit for data-heavy workflows than page stacks.

### Priority 7: Reduce reliance on URL parameters for transient workflow context

Several flows pass context such as accounting period or account selection through route query parameters. Move more of that context to shared layout state or an explicit current-period model.

Why this matters:

- It simplifies routing.
- It makes create flows feel more direct.
- It reduces the amount of UI state encoded in the URL.

## Practical Rollout Order

1. Add Transactions and Goals to persistent navigation.
2. Convert destructive and status-change pages to dialogs.
3. Add inline row actions on the highest-traffic list views.
4. Refactor accounting period detail into a tabbed or split workspace.
5. Introduce master-detail layouts for accounts, funds, and transactions.
6. Simplify context passing and reduce query-string-driven workflow state.

## Quick Wins

- Add Transactions and Goals to the sidebar.
- Replace close, reopen, post, unpost, and delete pages with confirmations.
- Add direct row actions next to View in the main list frames.

## Expected Outcome

If the changes above are implemented, the frontend should feel flatter, faster, and easier to operate because users will be able to start common tasks from anywhere, complete more work without leaving the current page, and move through core financial workflows with fewer route transitions.