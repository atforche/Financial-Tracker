I want to readd the CRUD interface for Transactions back to the frontend. Here are some important details to keep in mind.

### Transaction Types

There are four different types of transactions that all perform different tasks

1. Income Transaction - represents money entering a tracked account from an untracked place (either a standalone credit or a transfer from an untracked account)

1. Spending Transaction - represent money leaving a tracked account to an untracked place (either a standalone debit or a transaction to an untracked account)

1. Fund Transaction - represents money moving between funds without affecting any accounts

1. Account Transaction - represents money money between accounts without affecting any funds. There are three valid types of account transactions.
    
    1. Transferring money between two tracked accounts
    1. Transferring money between two untracked accounts
    1. A standalone debit or credit to an untracked account

I want the UI to present the user with the exact set of fields that they need, without them having to worry about the different transaction types. This likely means that we need two "phases" of transaction entry: one phase where the user enters enough information to determine what type of transaction is needed, and a second phase where the user provides the remaining needed information. I'm imaging each phase as a collapsible section where only one section can be open at a time.

### Routing

The transaction routes should be similar to the existing account, fund, and goal routes. There's a top-level /transactions route with a single detail, create, update, etc routes. Use query parameters to determine how the user navigated to the transaction page.

Users should be able to add transactions from either the AccountingPeriodTransactionListFrame or the AccountTransactionListFrame. So the optional query parameters should be accountingPeriodId and accountId.

I want to expose full create, read, update, post, unpost, and delete functionality for transactions through the UI.