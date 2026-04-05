# Budgets

This feature introduces the concept of budgets to the application. A budget is a way to track spending against a particular category on a per-accounting period basis.

## Backend

1. Add a budget entity.

    This entity is used to track information about the entity that persists across accounting periods. It should include a unique BudgetID, a name, a description, a budget type, and a linked fund ID.

    The available budget types include Monthly, Rolling, Savings, and Debt.

2. Add a budget amount value object.

    This value object represents an amount associated with a particular budget. This should look very similar to the existing AccountAmount and FundAmount objects.

3. Add a budget goal entity.

    This entity links a particular budget to its state within a particular accounting period. It should include a unique BudgetGoalID, an accounting period ID, a goal amount, a boolean indicating whether the goal is met, and a collection of budget balance histories.

    A budget balance history is an owned entity of the budget goal. It represents an event that caused in the balance of a budget to change. It should include a unique BudgetBalanceHistoryID, a parent budget goal, a transaction ID, a date, a sequence, a posted balance, and a nullable available to spend.

4. Create a budget service to manage budgets and budget goals. It should expose the following methods:

    1. A TryCreate method that attempts to create a new budget

    2. A TryCreateGoal method that attempts to create a new budget goal in a particular accounting period

    3. A TryUpdate method that attempts to update an existing budget. The only properties on the budget that can be updated are the name and description

    4. A TryUpdateGoal method that attempts to update an existing budget goal. The only property on the budget goal that can be updated is the goal amount.

    5. A TryDelete method that attempts to delete an existing budget. Deleting a budget should only be allowed if the budget isn't linked to any transactions (i.e. the budget doesn't have any budget balance histories)

    6. A TryDeleteGoal method that attempts to delete an existing budget goal. Deleting a budget should only be allowed if the budget isn't linked to any transactions within the goal's accounting period (i.e. the goal doesn't have any budget balance histories)

5. Update the accounting period service to allow budget goals to be specified when creating a new accounting period

    The TryCreate method on the accounting period service should allow a collection of requests to create budget goals to be specified. These budget goals will be added to the newly created accounting period.

6. Allow transactions to affect the balance of a budget

    Transaction accounts should have a new collection of budget amounts alongside the existing collection of fund amounts.

    For places where a transaction impacts the balance of a fund, the budget amounts should act as an additional fund amount for their linked fund. For example, imagine Budget A is linked to Fund B. If a transaction has a debit fund amount of $100 for Fund B and debit budget amount of $50 for Budget A, the total amount of the debit to Fund B is $150. 

7. Implement a budget balance service

    This budget balance service should look similar to the existing FundBalanceService, AccountBalanceService, and AccountingPeriodBalanceService. It should expose methods that other services use to keep budget balances in sync.

    Create a new budget balance value object. This object should be simpler than the existing account balance and fund balance value objects because a budget balance should only store the posted amount and a nullable available to spend amount. The available to spend amount should always be null for budgets of type savings or debt.

    Add new ApplyToBudgetBalance and ReverseFromBudgetBalance methods to the Transaction account. Also add a new ApplyToBudgetBalance method to the Transaction.

    Similar to the FundBalanceService, a debit will always decrease the balance of a budget and a credit will always increase it.

8. Update the fund balance service to consider budget amounts on a transaction

    Any balance changes made to a budget should also impact the linked account in the same way. 