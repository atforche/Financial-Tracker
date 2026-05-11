CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "AccountingPeriods" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AccountingPeriods" PRIMARY KEY,
    "Year" INTEGER NOT NULL,
    "Month" INTEGER NOT NULL,
    "Name" TEXT NOT NULL,
    "IsOpen" INTEGER NOT NULL
);

CREATE TABLE "Accounts" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Accounts" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Type" TEXT NOT NULL,
    "OpeningAccountingPeriodId" TEXT NOT NULL,
    "DateOpened" TEXT NOT NULL
);

CREATE TABLE "FundBalanceHistories" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_FundBalanceHistories" PRIMARY KEY,
    "FundId" TEXT NOT NULL,
    "TransactionId" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL,
    "PostedBalance" TEXT NOT NULL,
    "AmountAssigned" TEXT NOT NULL,
    "PendingAmountAssigned" TEXT NOT NULL,
    "AmountSpent" TEXT NOT NULL,
    "PendingAmountSpent" TEXT NOT NULL
);

CREATE TABLE "Funds" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Funds" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "IsSystemFund" INTEGER NOT NULL,
    "Description" TEXT NOT NULL,
    "OpeningAccountingPeriodId" TEXT NOT NULL
);

CREATE TABLE "AccountingPeriodBalanceHistories" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AccountingPeriodBalanceHistories" PRIMARY KEY,
    "AccountingPeriodId" TEXT NOT NULL,
    "OpeningBalance" TEXT NOT NULL,
    "ClosingBalance" TEXT NOT NULL,
    CONSTRAINT "FK_AccountingPeriodBalanceHistories_AccountingPeriods_AccountingPeriodId" FOREIGN KEY ("AccountingPeriodId") REFERENCES "AccountingPeriods" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AccountBalanceHistories" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AccountBalanceHistories" PRIMARY KEY,
    "AccountId" TEXT NOT NULL,
    "TransactionId" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL,
    "PostedBalance" TEXT NOT NULL,
    "PendingDebitAmount" TEXT NOT NULL,
    "PendingCreditAmount" TEXT NOT NULL,
    "AvailableToSpend" TEXT NULL,
    CONSTRAINT "FK_AccountBalanceHistories_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Goals" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Goals" PRIMARY KEY,
    "FundId" TEXT NOT NULL,
    "AccountingPeriodId" TEXT NOT NULL,
    "GoalType" TEXT NOT NULL,
    "GoalAmount" TEXT NOT NULL,
    "RemainingAmountToAssign" TEXT NOT NULL,
    "RemainingAmountToAssignIncludingPending" TEXT NOT NULL,
    "IsAssignmentGoalMet" INTEGER NOT NULL,
    "IsAssignmentGoalMetIncludingPending" INTEGER NOT NULL,
    "RemainingAmountToSpend" TEXT NOT NULL,
    "RemainingAmountToSpendIncludingPending" TEXT NOT NULL,
    "IsSpendingGoalMet" INTEGER NOT NULL,
    "IsSpendingGoalMetIncludingPending" INTEGER NOT NULL,
    CONSTRAINT "FK_Goals_Funds_FundId" FOREIGN KEY ("FundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Transactions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Transactions" PRIMARY KEY,
    "Type" INTEGER NOT NULL,
    "AccountingPeriodId" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL,
    "Location" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "AccountTransaction_DebitAccountId" TEXT NULL,
    "AccountTransaction_DebitPostedDate" TEXT NULL,
    "AccountTransaction_CreditAccountId" TEXT NULL,
    "AccountTransaction_CreditPostedDate" TEXT NULL,
    "FundTransaction_DebitFundId" TEXT NULL,
    "FundTransaction_CreditFundId" TEXT NULL,
    "IncomeTransaction_CreditAccountId" TEXT NULL,
    "IncomeTransaction_CreditPostedDate" TEXT NULL,
    "IncomeTransaction_DebitAccountId" TEXT NULL,
    "IncomeTransaction_DebitPostedDate" TEXT NULL,
    "SpendingTransaction_DebitAccountId" TEXT NULL,
    "SpendingTransaction_DebitPostedDate" TEXT NULL,
    "SpendingTransaction_CreditAccountId" TEXT NULL,
    "SpendingTransaction_CreditPostedDate" TEXT NULL,
    CONSTRAINT "FK_Transactions_Accounts_AccountTransaction_CreditAccountId" FOREIGN KEY ("AccountTransaction_CreditAccountId") REFERENCES "Accounts" ("Id"),
    CONSTRAINT "FK_Transactions_Accounts_AccountTransaction_DebitAccountId" FOREIGN KEY ("AccountTransaction_DebitAccountId") REFERENCES "Accounts" ("Id"),
    CONSTRAINT "FK_Transactions_Accounts_IncomeTransaction_CreditAccountId" FOREIGN KEY ("IncomeTransaction_CreditAccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Transactions_Accounts_IncomeTransaction_DebitAccountId" FOREIGN KEY ("IncomeTransaction_DebitAccountId") REFERENCES "Accounts" ("Id"),
    CONSTRAINT "FK_Transactions_Accounts_SpendingTransaction_CreditAccountId" FOREIGN KEY ("SpendingTransaction_CreditAccountId") REFERENCES "Accounts" ("Id"),
    CONSTRAINT "FK_Transactions_Accounts_SpendingTransaction_DebitAccountId" FOREIGN KEY ("SpendingTransaction_DebitAccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Transactions_Funds_FundTransaction_CreditFundId" FOREIGN KEY ("FundTransaction_CreditFundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Transactions_Funds_FundTransaction_DebitFundId" FOREIGN KEY ("FundTransaction_DebitFundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AccountingPeriodAccountBalanceHistory" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AccountingPeriodAccountBalanceHistory" PRIMARY KEY,
    "AccountId" TEXT NOT NULL,
    "AccountingPeriodId" TEXT NOT NULL,
    "OpeningBalance" TEXT NOT NULL,
    "ClosingBalance" TEXT NOT NULL,
    "AccountingPeriodBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_AccountingPeriodAccountBalanceHistory_AccountingPeriodBalanceHistories_AccountingPeriodBalanceHistoryId" FOREIGN KEY ("AccountingPeriodBalanceHistoryId") REFERENCES "AccountingPeriodBalanceHistories" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AccountingPeriodAccountBalanceHistory_AccountingPeriods_AccountingPeriodId" FOREIGN KEY ("AccountingPeriodId") REFERENCES "AccountingPeriods" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AccountingPeriodAccountBalanceHistory_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AccountingPeriodFundBalanceHistory" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AccountingPeriodFundBalanceHistory" PRIMARY KEY,
    "FundId" TEXT NOT NULL,
    "AccountingPeriodId" TEXT NOT NULL,
    "OpeningBalance" TEXT NOT NULL,
    "AmountAssigned" TEXT NOT NULL,
    "PendingAmountAssigned" TEXT NOT NULL,
    "AmountSpent" TEXT NOT NULL,
    "PendingAmountSpent" TEXT NOT NULL,
    "ClosingBalance" TEXT NOT NULL,
    "AccountingPeriodBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_AccountingPeriodFundBalanceHistory_AccountingPeriodBalanceHistories_AccountingPeriodBalanceHistoryId" FOREIGN KEY ("AccountingPeriodBalanceHistoryId") REFERENCES "AccountingPeriodBalanceHistories" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AccountingPeriodFundBalanceHistory_AccountingPeriods_AccountingPeriodId" FOREIGN KEY ("AccountingPeriodId") REFERENCES "AccountingPeriods" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AccountingPeriodFundBalanceHistory_Funds_FundId" FOREIGN KEY ("FundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE
);

CREATE TABLE "IncomeTransactionFundAssignments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_IncomeTransactionFundAssignments" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "IncomeTransactionId" TEXT NOT NULL,
    CONSTRAINT "FK_IncomeTransactionFundAssignments_Transactions_IncomeTransactionId" FOREIGN KEY ("IncomeTransactionId") REFERENCES "Transactions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "SpendingTransactionFundAssignments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SpendingTransactionFundAssignments" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "SpendingTransactionId" TEXT NOT NULL,
    CONSTRAINT "FK_SpendingTransactionFundAssignments_Transactions_SpendingTransactionId" FOREIGN KEY ("SpendingTransactionId") REFERENCES "Transactions" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AccountBalanceHistories_AccountId" ON "AccountBalanceHistories" ("AccountId");

CREATE INDEX "IX_AccountingPeriodAccountBalanceHistory_AccountId" ON "AccountingPeriodAccountBalanceHistory" ("AccountId");

CREATE INDEX "IX_AccountingPeriodAccountBalanceHistory_AccountingPeriodBalanceHistoryId" ON "AccountingPeriodAccountBalanceHistory" ("AccountingPeriodBalanceHistoryId");

CREATE INDEX "IX_AccountingPeriodAccountBalanceHistory_AccountingPeriodId" ON "AccountingPeriodAccountBalanceHistory" ("AccountingPeriodId");

CREATE INDEX "IX_AccountingPeriodBalanceHistories_AccountingPeriodId" ON "AccountingPeriodBalanceHistories" ("AccountingPeriodId");

CREATE INDEX "IX_AccountingPeriodFundBalanceHistory_AccountingPeriodBalanceHistoryId" ON "AccountingPeriodFundBalanceHistory" ("AccountingPeriodBalanceHistoryId");

CREATE INDEX "IX_AccountingPeriodFundBalanceHistory_AccountingPeriodId" ON "AccountingPeriodFundBalanceHistory" ("AccountingPeriodId");

CREATE INDEX "IX_AccountingPeriodFundBalanceHistory_FundId" ON "AccountingPeriodFundBalanceHistory" ("FundId");

CREATE INDEX "IX_AccountingPeriods_Name" ON "AccountingPeriods" ("Name");

CREATE UNIQUE INDEX "IX_Accounts_Name" ON "Accounts" ("Name");

CREATE UNIQUE INDEX "IX_Funds_Name" ON "Funds" ("Name");

CREATE INDEX "IX_Goals_FundId" ON "Goals" ("FundId");

CREATE INDEX "IX_IncomeTransactionFundAssignments_IncomeTransactionId" ON "IncomeTransactionFundAssignments" ("IncomeTransactionId");

CREATE INDEX "IX_SpendingTransactionFundAssignments_SpendingTransactionId" ON "SpendingTransactionFundAssignments" ("SpendingTransactionId");

CREATE INDEX "IX_Transactions_AccountTransaction_CreditAccountId" ON "Transactions" ("AccountTransaction_CreditAccountId");

CREATE INDEX "IX_Transactions_AccountTransaction_DebitAccountId" ON "Transactions" ("AccountTransaction_DebitAccountId");

CREATE INDEX "IX_Transactions_FundTransaction_CreditFundId" ON "Transactions" ("FundTransaction_CreditFundId");

CREATE INDEX "IX_Transactions_FundTransaction_DebitFundId" ON "Transactions" ("FundTransaction_DebitFundId");

CREATE INDEX "IX_Transactions_IncomeTransaction_CreditAccountId" ON "Transactions" ("IncomeTransaction_CreditAccountId");

CREATE INDEX "IX_Transactions_IncomeTransaction_DebitAccountId" ON "Transactions" ("IncomeTransaction_DebitAccountId");

CREATE INDEX "IX_Transactions_SpendingTransaction_CreditAccountId" ON "Transactions" ("SpendingTransaction_CreditAccountId");

CREATE INDEX "IX_Transactions_SpendingTransaction_DebitAccountId" ON "Transactions" ("SpendingTransaction_DebitAccountId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260511170733_InitialCreate', '10.0.2');

COMMIT;

