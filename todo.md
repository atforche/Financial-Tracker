I want to implement a new set of pages to the frontend of my application. These new pages will be known as "current" pages. These are the steps that I want to implement. All new components should be put under new "current" subfolders under the main folders. 

1. Rename all "dashboard" related concepts in the frontend and backend to be called "trends".

    We need to add some distinction between a dashboard that displays data over a period of time and a dashboard that displays information only about the current snapshot of time. All of the existing "dashboards" that display information over a period of time will become known as "trends" pages. This paves the way for the new set of pages that I want to implement to become "current" pages.

1. Implement the "current" page for an accounting period

    Add a header to the page.

    Under the header, add a new component that is very similar to the existing AccountingPeriodDashboardSummaryCards, except it only displays information for the current accounting period

    Under the new summary card, add a new component that is very similar to the existing AccountingPeriodDashboardIncomeSpending except it only displays information for the current accounting period

    Under the new income spending card, add a new list frame component that displays information about the transactions in the current accounting period.

    Add the new "current" page to the accounting period navigation

    Implement a single "current" endpoint on the Accounting Period controller that includes all the information needed

1. Implement the "current" page for accounts

    Add a header to the page.

    Under the header, add a new component that is similar to the AccountDashboardSummaryCards except it only displays the current aggregated total account balance along with the current balance breakdown by type. Avoid expandable sections and just have everything always be visible.

    Under the new total balance card, implement a component that displays a single card for each account. The card should display the name of the account, the current balance of the account, and the last balance event date for the account. Each card should be expandable and expanding the card should display the last five balance events for the account.

    Add the new "current" page to the account navigation.

    Implement a single "current" endpoint on the Account controller that includes all the information needed.

1. Implement the "current" page for funds

    Add a header to the page.

    Under the header, add a new component similar to the FundDashboardSummaryCards except it only displays the current aggregated total fund balance along with the current balance breakdown by assigned/unassigned. Avoid expandable sections and just have everything always be visible.

    Under the new total balance card, implement a component that displays a single card for each fund. The card should display the name of the fund, the current balance of the fund, and the last balance event date for the fund. Each card should be expandable and expanding the card should display the last five balance events for the fund.

    Add the new "current" page to the fund navigation.

    Implement a single "current" endpoint on the Fund controller that includes all the information needed.

1. Implement the "current" page for goals.

    Add a header to the page.

    Under the header, add a new component similar to the GoalDashboardSummaryCards except it only displays information for the current accounting period.

    Under the new summary cards, implement a component that displays a single card for each fund. The card should display the name of the fund, the current progress as a bar toward the assignment goal, the last assignment balance event date for the assignment goal, the current process as a bar toward the spending goal, and the last spending balance event date for the spending goal. Each card should be expandable and expanding the card should display the last five assignment balance events and the last five spending balance events for the goal.

    Add the new "current" page to the goal navigation.

    Implement a single "current" endpoint on the Goal controller that includes all the information needed.

1. Implement the "current" page for transactions.

    Add a header to the page.

    Under the header, add a new component similar to the TransactionDashboardByTypeCard except it only displays information about transactions in the current accounting period.

    Under the type cards, add a list frame that displays all the transactions from the current accounting period that are not fully posted.

    Under the unposted list frame, add a list frame that displays information about the posted transactions in the current accounting period.

    Add the new "current" page to the transaction navigation.

    Implement a single "current" endpoint on the Transaction controller that includes all the information needed.
