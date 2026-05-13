## Develop a new account and fund onboarding flow for initial setup of the system.

### Motivation:

Currently, you can add an account with an initial balance, and that initial balance can be assigned to different funds. The problem is that this initial transaction needs an accounting period to be added, so these initial setup transactions appear as regular transactions in the first accounting period. This is problematic because it appears as if you have huge amounts of income coming in during that initial accounting period.

### Goal:

Develop a new onboarding flow that allows multiple accounts and funds to be added (with initial balances) without having any impact on any accounting periods. This facilitates getting accounts and funds set up in the system without polluting any of the accounting periods used for actual tracking.

### Tasks:

1. Remove the existing "initial balance" feature

    1. Remove the concepts of an "initial balance" and "initial transaction" from creating an Account. When an account is created through the normal flow, a transaction must be manually added to populate the balance of that account.

    1. Rename the "AddAccountingPeriodId" on the Account and Fund classes to be "OpeningAccountingPeriodId"

    1. Rename the "AddDate" on the Account class to be "DateOpened"

    1. Update the UI to remove the now deprecated features

1. Add a new "onboarding" flow

    1. Add new decimal "OnboardedBalance" properties to the Account and Fund classes.

    1. Make the "OpeningAccountingPeriodId" and "DateOpened" properties on the Account and Fund nullable.

    1. Add an "Onboard" method to the AccountService.

        1. Validate that onboarding can only be completed if there are no accounting periods set up in the system.

        1. Account onboarding takes an account name, type, and initial balance.

        1. An onboarded account will have the OnboardedBalance set and the OpeningAccountingPeriodId and OpenedDate set to null

        1. If the onboarded account is tracked and the Unassigned fund doesn't exist, create it.

        1. If the onboarded account is tracked, update the Unassigned fund's OnboardedBalance with the initial balance of the account. Non-debt accounts should increase the unassigned balance and debt accounts should decrease the unassigned balance.

        1. Add validation that the balance of the unassigned fund can't go negative when onboarding an account

    1. Add an "Onboard" method to the FundService.

        1. Validate that onboard can only be completed if there are no accounting periods set up in the system.

        1. Fund onboarding takes a fund name, description, and initial balance.

        1. An onboarded fund will have the OnboardedBalance set and the OpeningAccountingPeriodId set to null

        1. Onboarding a fund will decrease the OnboardedBalance of the unassigned fund

        1. Add validation that the unassigned fund must exist and there must be a sufficient unassigned amount to cover the fund's initial balance.

    1. Modify the AccountBalanceService and FundBalanceService to fall back to the onboarded balance if no balance histories can be found

    1. Add validation that prevents onboarded accounts and funds from being deleted.