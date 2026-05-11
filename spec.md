## Develop a new account and fund onboarding flow for initial setup of the system.

### Motivation:

Currently, you can add an account with an initial balance, and that initial balance can be assigned to different funds. The problem is that this initial transaction needs an accounting period to be added, so these initial setup transactions appear as regular transactions in the first accounting period. This is problematic because it appears as if you have huge amounts of income coming in during that initial accounting period.

### Goal:

Develop a new onboarding flow that allows multiple accounts and funds to be added (with initial balances) without having any impact on any accounting periods. This facilitates getting accounts and funds set up in the system without polluting any of the accounting periods used for actual tracking.

### Tasks:

1. Remove the existing "initial balance" feature

    1. Remove the concepts of an "initial balance" and "initial transaction" from creating an Account. When an account is created through the normal flow, a transaction must be manually added to populate the balance of that account.

    1. Rename the "AddAccountingPeriodId" on the Account and Fund classes to be "OpenedAccountingPeriodId"

    1. Rename the "AddDate" on the Account class to be "OpenedDate"

    1. Update the UI to remove the now deprecated features

1. Add a new "onboarding" flow

    1. Add new decimal "OnboardedBalance" properties to the Account and Fund classes.

    1. Make the "OpenedAccountingPeriodId" and "OpenedDate" properties on the Account and Fund nullable.

    1. Create a new OnboardingService domain service to orchestrate the onboarding process.

    1. Validate that onboarding can only be completed if there are no accounts, funds, or accounting periods set up in the system.

    1. Allow two collections to be passed into onboarding: a collection of account names, types, and initial balances and a collection of fund names and initial balances.

    1. Onboarding will create a set of accounts with the "OnboardedBalance" set and the "OpenedAccountingPeriodId" and "OpenedDate" set to null.

    1. Onboarding will create a set of funds with the "OnboardedBalance" set and the "OpenedAccountingPeriodId" set to null.

    1. Modify the AccountBalanceService and FundBalanceService to fall back to the onboarded balance if no balance histories can be found

    1. Onboarding will create the Unassigned fund if it doesn't already exist. Ensure that it is created first so that any subsequent funds are prevented from having a name of "unassigned".

    1. Add validation that prevents onboarded accounts from being deleted.

    1. Add an OnboardingController to the REST API

    1. Add UI to the frontend to support onboarding