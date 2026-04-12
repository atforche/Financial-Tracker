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
    "AddAccountingPeriodId" TEXT NOT NULL,
    "AddDate" TEXT NOT NULL,
    "InitialTransaction" TEXT NULL
);

CREATE TABLE "FundBalanceHistories" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_FundBalanceHistories" PRIMARY KEY,
    "FundId" TEXT NOT NULL,
    "TransactionId" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL,
    "PostedBalance" TEXT NOT NULL,
    "PendingDebitAmount" TEXT NOT NULL,
    "PendingCreditAmount" TEXT NOT NULL
);

CREATE TABLE "Funds" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Funds" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "IsSystemFund" INTEGER NOT NULL,
    "Description" TEXT NOT NULL,
    "AddAccountingPeriodId" TEXT NOT NULL
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

CREATE TABLE "Transactions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Transactions" PRIMARY KEY,
    "Type" INTEGER NOT NULL,
    "AccountingPeriodId" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL,
    "Location" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "IncomeTransaction_AccountId" TEXT NULL,
    "IncomeTransaction_PostedDate" TEXT NULL,
    "GeneratedByAccountId" TEXT NULL,
    "DebitAccountId" TEXT NULL,
    "DebitPostedDate" TEXT NULL,
    "AccountId" TEXT NULL,
    "PostedDate" TEXT NULL,
    "CreditAccountId" TEXT NULL,
    "CreditPostedDate" TEXT NULL,
    "TransferTransaction_DebitAccountId" TEXT NULL,
    "TransferTransaction_DebitPostedDate" TEXT NULL,
    "TransferTransaction_CreditAccountId" TEXT NULL,
    "TransferTransaction_CreditPostedDate" TEXT NULL,
    CONSTRAINT "FK_Transactions_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Transactions_Accounts_CreditAccountId" FOREIGN KEY ("CreditAccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Transactions_Accounts_DebitAccountId" FOREIGN KEY ("DebitAccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Transactions_Accounts_IncomeTransaction_AccountId" FOREIGN KEY ("IncomeTransaction_AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Transactions_Accounts_TransferTransaction_CreditAccountId" FOREIGN KEY ("TransferTransaction_CreditAccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Transactions_Accounts_TransferTransaction_DebitAccountId" FOREIGN KEY ("TransferTransaction_DebitAccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "FundGoals" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_FundGoals" PRIMARY KEY,
    "FundId" TEXT NOT NULL,
    "AccountingPeriodId" TEXT NOT NULL,
    "GoalType" TEXT NOT NULL,
    "GoalAmount" TEXT NOT NULL,
    "IsGoalMet" INTEGER NOT NULL,
    CONSTRAINT "FK_FundGoals_Funds_FundId" FOREIGN KEY ("FundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AccountAccountingPeriodBalanceHistory" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AccountAccountingPeriodBalanceHistory" PRIMARY KEY,
    "AccountId" TEXT NOT NULL,
    "AccountingPeriodId" TEXT NOT NULL,
    "OpeningBalance" TEXT NOT NULL,
    "ClosingBalance" TEXT NOT NULL,
    "AccountingPeriodBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_AccountAccountingPeriodBalanceHistory_AccountingPeriodBalanceHistories_AccountingPeriodBalanceHistoryId" FOREIGN KEY ("AccountingPeriodBalanceHistoryId") REFERENCES "AccountingPeriodBalanceHistories" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AccountAccountingPeriodBalanceHistory_AccountingPeriods_AccountingPeriodId" FOREIGN KEY ("AccountingPeriodId") REFERENCES "AccountingPeriods" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AccountAccountingPeriodBalanceHistory_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "FundAccountingPeriodBalanceHistory" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_FundAccountingPeriodBalanceHistory" PRIMARY KEY,
    "FundId" TEXT NOT NULL,
    "AccountingPeriodId" TEXT NOT NULL,
    "OpeningBalance" TEXT NOT NULL,
    "AmountAssigned" TEXT NOT NULL,
    "AmountSpent" TEXT NOT NULL,
    "ClosingBalance" TEXT NOT NULL,
    "AccountingPeriodBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_FundAccountingPeriodBalanceHistory_AccountingPeriodBalanceHistories_AccountingPeriodBalanceHistoryId" FOREIGN KEY ("AccountingPeriodBalanceHistoryId") REFERENCES "AccountingPeriodBalanceHistories" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_FundAccountingPeriodBalanceHistory_AccountingPeriods_AccountingPeriodId" FOREIGN KEY ("AccountingPeriodId") REFERENCES "AccountingPeriods" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_FundAccountingPeriodBalanceHistory_Funds_FundId" FOREIGN KEY ("FundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE
);

CREATE TABLE "IncomeTransactionFundAmounts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_IncomeTransactionFundAmounts" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "IncomeTransactionId" TEXT NOT NULL,
    CONSTRAINT "FK_IncomeTransactionFundAmounts_Transactions_IncomeTransactionId" FOREIGN KEY ("IncomeTransactionId") REFERENCES "Transactions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "SpendingTransactionFundAmounts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SpendingTransactionFundAmounts" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "SpendingTransactionId" TEXT NOT NULL,
    CONSTRAINT "FK_SpendingTransactionFundAmounts_Transactions_SpendingTransactionId" FOREIGN KEY ("SpendingTransactionId") REFERENCES "Transactions" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AccountAccountingPeriodBalanceHistory_AccountId" ON "AccountAccountingPeriodBalanceHistory" ("AccountId");

CREATE INDEX "IX_AccountAccountingPeriodBalanceHistory_AccountingPeriodBalanceHistoryId" ON "AccountAccountingPeriodBalanceHistory" ("AccountingPeriodBalanceHistoryId");

CREATE INDEX "IX_AccountAccountingPeriodBalanceHistory_AccountingPeriodId" ON "AccountAccountingPeriodBalanceHistory" ("AccountingPeriodId");

CREATE INDEX "IX_AccountBalanceHistories_AccountId" ON "AccountBalanceHistories" ("AccountId");

CREATE INDEX "IX_AccountingPeriodBalanceHistories_AccountingPeriodId" ON "AccountingPeriodBalanceHistories" ("AccountingPeriodId");

CREATE INDEX "IX_AccountingPeriods_Name" ON "AccountingPeriods" ("Name");

CREATE UNIQUE INDEX "IX_Accounts_Name" ON "Accounts" ("Name");

CREATE INDEX "IX_FundAccountingPeriodBalanceHistory_AccountingPeriodBalanceHistoryId" ON "FundAccountingPeriodBalanceHistory" ("AccountingPeriodBalanceHistoryId");

CREATE INDEX "IX_FundAccountingPeriodBalanceHistory_AccountingPeriodId" ON "FundAccountingPeriodBalanceHistory" ("AccountingPeriodId");

CREATE INDEX "IX_FundAccountingPeriodBalanceHistory_FundId" ON "FundAccountingPeriodBalanceHistory" ("FundId");

CREATE INDEX "IX_FundGoals_FundId" ON "FundGoals" ("FundId");

CREATE UNIQUE INDEX "IX_Funds_Name" ON "Funds" ("Name");

CREATE INDEX "IX_IncomeTransactionFundAmounts_IncomeTransactionId" ON "IncomeTransactionFundAmounts" ("IncomeTransactionId");

CREATE INDEX "IX_SpendingTransactionFundAmounts_SpendingTransactionId" ON "SpendingTransactionFundAmounts" ("SpendingTransactionId");

CREATE INDEX "IX_Transactions_AccountId" ON "Transactions" ("AccountId");

CREATE INDEX "IX_Transactions_CreditAccountId" ON "Transactions" ("CreditAccountId");

CREATE INDEX "IX_Transactions_DebitAccountId" ON "Transactions" ("DebitAccountId");

CREATE INDEX "IX_Transactions_IncomeTransaction_AccountId" ON "Transactions" ("IncomeTransaction_AccountId");

CREATE INDEX "IX_Transactions_TransferTransaction_CreditAccountId" ON "Transactions" ("TransferTransaction_CreditAccountId");

CREATE INDEX "IX_Transactions_TransferTransaction_DebitAccountId" ON "Transactions" ("TransferTransaction_DebitAccountId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260412140304_InitialCreate', '10.0.2');

COMMIT;

