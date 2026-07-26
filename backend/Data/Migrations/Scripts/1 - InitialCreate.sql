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
    "OpeningAccountingPeriodId" TEXT NULL,
    "DateOpened" TEXT NULL,
    "OnboardedBalance" TEXT NULL
);

CREATE TABLE "FundGoalTotalsHistories" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_FundGoalTotalsHistories" PRIMARY KEY,
    "FundId" TEXT NOT NULL,
    "AccountingPeriodId" TEXT NOT NULL,
    "TransactionId" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL,
    "AmountAssigned" TEXT NOT NULL,
    "AmountSpent" TEXT NOT NULL
);

CREATE TABLE "Funds" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Funds" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "OpeningAccountingPeriodId" TEXT NULL,
    "OnboardedBalance" TEXT NULL
);

CREATE TABLE "PendingFundGoalTotalsEffects" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_PendingFundGoalTotalsEffects" PRIMARY KEY,
    "FundId" TEXT NOT NULL,
    "AccountingPeriodId" TEXT NOT NULL,
    "TransactionId" TEXT NOT NULL,
    "PendingAmountAssigned" TEXT NOT NULL,
    "PendingAmountSpent" TEXT NOT NULL
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
    CONSTRAINT "FK_AccountBalanceHistories_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "PendingAccountBalanceEffects" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_PendingAccountBalanceEffects" PRIMARY KEY,
    "AccountId" TEXT NOT NULL,
    "TransactionId" TEXT NOT NULL,
    "PendingDebitAmount" TEXT NOT NULL,
    "PendingCreditAmount" TEXT NOT NULL,
    CONSTRAINT "FK_PendingAccountBalanceEffects_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "FundBalanceHistories" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_FundBalanceHistories" PRIMARY KEY,
    "FundId" TEXT NOT NULL,
    "TransactionId" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL,
    "PostedBalance" TEXT NOT NULL,
    CONSTRAINT "FK_FundBalanceHistories_Funds_FundId" FOREIGN KEY ("FundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE
);

CREATE TABLE "FundGoals" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_FundGoals" PRIMARY KEY,
    "FundId" TEXT NOT NULL,
    "AccountingPeriodId" TEXT NULL,
    "RegularContribution" TEXT NULL,
    "MinimumFundedBalance" TEXT NULL,
    "MaximumFundedBalance" TEXT NULL,
    "TargetEndingBalance" TEXT NULL,
    CONSTRAINT "FK_FundGoals_AccountingPeriods_AccountingPeriodId" FOREIGN KEY ("AccountingPeriodId") REFERENCES "AccountingPeriods" ("Id"),
    CONSTRAINT "FK_FundGoals_Funds_FundId" FOREIGN KEY ("FundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE
);

CREATE TABLE "PendingFundBalanceEffects" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_PendingFundBalanceEffects" PRIMARY KEY,
    "FundId" TEXT NOT NULL,
    "TransactionId" TEXT NOT NULL,
    "PendingDebitAmount" TEXT NOT NULL,
    "PendingCreditAmount" TEXT NOT NULL,
    CONSTRAINT "FK_PendingFundBalanceEffects_Funds_FundId" FOREIGN KEY ("FundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Transactions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Transactions" PRIMARY KEY,
    "Type" INTEGER NOT NULL,
    "AccountingPeriodId" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL,
    "Description" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "AccountTransaction_DebitAccountId" TEXT NULL,
    "AccountTransaction_DebitPostedDate" TEXT NULL,
    "AccountTransaction_SourceLocation" TEXT NULL,
    "FundTransaction_DebitFundId" TEXT NULL,
    "IncomeTransaction_SourceAccountId" TEXT NULL,
    "IncomeTransaction_SourcePostedDate" TEXT NULL,
    "IncomeTransaction_SourceLocation" TEXT NULL,
    "TrackedAmount" TEXT NULL,
    "SpendingTransaction_DebitAccountId" TEXT NULL,
    "SpendingTransaction_DebitPostedDate" TEXT NULL,
    CONSTRAINT "FK_Transactions_Accounts_AccountTransaction_DebitAccountId" FOREIGN KEY ("AccountTransaction_DebitAccountId") REFERENCES "Accounts" ("Id"),
    CONSTRAINT "FK_Transactions_Accounts_IncomeTransaction_SourceAccountId" FOREIGN KEY ("IncomeTransaction_SourceAccountId") REFERENCES "Accounts" ("Id"),
    CONSTRAINT "FK_Transactions_Accounts_SpendingTransaction_DebitAccountId" FOREIGN KEY ("SpendingTransaction_DebitAccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE,
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
    "ClosingBalance" TEXT NOT NULL,
    "AccountingPeriodBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_AccountingPeriodFundBalanceHistory_AccountingPeriodBalanceHistories_AccountingPeriodBalanceHistoryId" FOREIGN KEY ("AccountingPeriodBalanceHistoryId") REFERENCES "AccountingPeriodBalanceHistories" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AccountingPeriodFundBalanceHistory_AccountingPeriods_AccountingPeriodId" FOREIGN KEY ("AccountingPeriodId") REFERENCES "AccountingPeriods" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AccountingPeriodFundBalanceHistory_Funds_FundId" FOREIGN KEY ("FundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AccountingPeriodFundGoalTotals" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AccountingPeriodFundGoalTotals" PRIMARY KEY,
    "FundId" TEXT NOT NULL,
    "AccountingPeriodId" TEXT NOT NULL,
    "AmountAssigned" TEXT NOT NULL,
    "AmountSpent" TEXT NOT NULL,
    "AccountingPeriodBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_AccountingPeriodFundGoalTotals_AccountingPeriodBalanceHistories_AccountingPeriodBalanceHistoryId" FOREIGN KEY ("AccountingPeriodBalanceHistoryId") REFERENCES "AccountingPeriodBalanceHistories" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AccountingPeriodFundGoalTotals_AccountingPeriods_AccountingPeriodId" FOREIGN KEY ("AccountingPeriodId") REFERENCES "AccountingPeriods" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AccountingPeriodFundGoalTotals_Funds_FundId" FOREIGN KEY ("FundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AccountTransactionDestinations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AccountTransactionDestinations" PRIMARY KEY AUTOINCREMENT,
    "CreditAccountId" TEXT NULL,
    "CreditPostedDate" TEXT NULL,
    "Location" TEXT NULL,
    "Amount" TEXT NOT NULL,
    "AccountTransactionId" TEXT NOT NULL,
    CONSTRAINT "FK_AccountTransactionDestinations_Accounts_CreditAccountId" FOREIGN KEY ("CreditAccountId") REFERENCES "Accounts" ("Id"),
    CONSTRAINT "FK_AccountTransactionDestinations_Transactions_AccountTransactionId" FOREIGN KEY ("AccountTransactionId") REFERENCES "Transactions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "FundTransactionDestinations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_FundTransactionDestinations" PRIMARY KEY AUTOINCREMENT,
    "CreditFundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "FundTransactionId" TEXT NOT NULL,
    CONSTRAINT "FK_FundTransactionDestinations_Funds_CreditFundId" FOREIGN KEY ("CreditFundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_FundTransactionDestinations_Transactions_FundTransactionId" FOREIGN KEY ("FundTransactionId") REFERENCES "Transactions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "IncomeTransactionIncomeDeductions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_IncomeTransactionIncomeDeductions" PRIMARY KEY AUTOINCREMENT,
    "Description" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "IncomeTransactionId" TEXT NULL,
    CONSTRAINT "FK_IncomeTransactionIncomeDeductions_Transactions_IncomeTransactionId" FOREIGN KEY ("IncomeTransactionId") REFERENCES "Transactions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "IncomeTransactionIncomeDestinations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_IncomeTransactionIncomeDestinations" PRIMARY KEY AUTOINCREMENT,
    "AccountId" TEXT NOT NULL,
    "PostedDate" TEXT NULL,
    "Amount" TEXT NOT NULL,
    "IncomeTransactionId" TEXT NOT NULL,
    CONSTRAINT "FK_IncomeTransactionIncomeDestinations_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_IncomeTransactionIncomeDestinations_Transactions_IncomeTransactionId" FOREIGN KEY ("IncomeTransactionId") REFERENCES "Transactions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "IncomeTransactionIncomeLines" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_IncomeTransactionIncomeLines" PRIMARY KEY AUTOINCREMENT,
    "Description" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "IncomeTransactionId" TEXT NULL,
    CONSTRAINT "FK_IncomeTransactionIncomeLines_Transactions_IncomeTransactionId" FOREIGN KEY ("IncomeTransactionId") REFERENCES "Transactions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "SpendingTransactionDestinations" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SpendingTransactionDestinations" PRIMARY KEY AUTOINCREMENT,
    "CreditAccountId" TEXT NULL,
    "CreditPostedDate" TEXT NULL,
    "Location" TEXT NULL,
    "Amount" TEXT NOT NULL,
    "SpendingTransactionId" TEXT NOT NULL,
    CONSTRAINT "FK_SpendingTransactionDestinations_Accounts_CreditAccountId" FOREIGN KEY ("CreditAccountId") REFERENCES "Accounts" ("Id"),
    CONSTRAINT "FK_SpendingTransactionDestinations_Transactions_SpendingTransactionId" FOREIGN KEY ("SpendingTransactionId") REFERENCES "Transactions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "IncomeTransactionIncomeDestinationFundAssignments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_IncomeTransactionIncomeDestinationFundAssignments" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "IncomeDestinationId" INTEGER NOT NULL,
    CONSTRAINT "FK_IncomeTransactionIncomeDestinationFundAssignments_IncomeTransactionIncomeDestinations_IncomeDestinationId" FOREIGN KEY ("IncomeDestinationId") REFERENCES "IncomeTransactionIncomeDestinations" ("Id") ON DELETE CASCADE
);

CREATE TABLE "SpendingTransactionDestinationFundAssignments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SpendingTransactionDestinationFundAssignments" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "DestinationId" INTEGER NOT NULL,
    CONSTRAINT "FK_SpendingTransactionDestinationFundAssignments_SpendingTransactionDestinations_DestinationId" FOREIGN KEY ("DestinationId") REFERENCES "SpendingTransactionDestinations" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_AccountBalanceHistories_AccountId_Date_Sequence" ON "AccountBalanceHistories" ("AccountId", "Date", "Sequence");

CREATE INDEX "IX_AccountingPeriodAccountBalanceHistory_AccountId" ON "AccountingPeriodAccountBalanceHistory" ("AccountId");

CREATE INDEX "IX_AccountingPeriodAccountBalanceHistory_AccountingPeriodBalanceHistoryId" ON "AccountingPeriodAccountBalanceHistory" ("AccountingPeriodBalanceHistoryId");

CREATE INDEX "IX_AccountingPeriodAccountBalanceHistory_AccountingPeriodId" ON "AccountingPeriodAccountBalanceHistory" ("AccountingPeriodId");

CREATE INDEX "IX_AccountingPeriodBalanceHistories_AccountingPeriodId" ON "AccountingPeriodBalanceHistories" ("AccountingPeriodId");

CREATE INDEX "IX_AccountingPeriodFundBalanceHistory_AccountingPeriodBalanceHistoryId" ON "AccountingPeriodFundBalanceHistory" ("AccountingPeriodBalanceHistoryId");

CREATE INDEX "IX_AccountingPeriodFundBalanceHistory_AccountingPeriodId" ON "AccountingPeriodFundBalanceHistory" ("AccountingPeriodId");

CREATE INDEX "IX_AccountingPeriodFundBalanceHistory_FundId" ON "AccountingPeriodFundBalanceHistory" ("FundId");

CREATE INDEX "IX_AccountingPeriodFundGoalTotals_AccountingPeriodBalanceHistoryId" ON "AccountingPeriodFundGoalTotals" ("AccountingPeriodBalanceHistoryId");

CREATE INDEX "IX_AccountingPeriodFundGoalTotals_AccountingPeriodId" ON "AccountingPeriodFundGoalTotals" ("AccountingPeriodId");

CREATE INDEX "IX_AccountingPeriodFundGoalTotals_FundId" ON "AccountingPeriodFundGoalTotals" ("FundId");

CREATE INDEX "IX_AccountingPeriods_Name" ON "AccountingPeriods" ("Name");

CREATE UNIQUE INDEX "IX_Accounts_Name" ON "Accounts" ("Name");

CREATE INDEX "IX_AccountTransactionDestinations_AccountTransactionId" ON "AccountTransactionDestinations" ("AccountTransactionId");

CREATE INDEX "IX_AccountTransactionDestinations_CreditAccountId" ON "AccountTransactionDestinations" ("CreditAccountId");

CREATE UNIQUE INDEX "IX_FundBalanceHistories_FundId_Date_Sequence" ON "FundBalanceHistories" ("FundId", "Date", "Sequence");

CREATE INDEX "IX_FundGoals_AccountingPeriodId" ON "FundGoals" ("AccountingPeriodId");

CREATE UNIQUE INDEX "IX_FundGoals_FundId" ON "FundGoals" ("FundId") WHERE "AccountingPeriodId" IS NULL;

CREATE UNIQUE INDEX "IX_FundGoals_FundId_AccountingPeriodId" ON "FundGoals" ("FundId", "AccountingPeriodId");

CREATE UNIQUE INDEX "IX_FundGoalTotalsHistories_FundId_AccountingPeriodId_Date_Sequence" ON "FundGoalTotalsHistories" ("FundId", "AccountingPeriodId", "Date", "Sequence");

CREATE UNIQUE INDEX "IX_Funds_Name" ON "Funds" ("Name");

CREATE INDEX "IX_FundTransactionDestinations_CreditFundId" ON "FundTransactionDestinations" ("CreditFundId");

CREATE INDEX "IX_FundTransactionDestinations_FundTransactionId" ON "FundTransactionDestinations" ("FundTransactionId");

CREATE INDEX "IX_IncomeTransactionIncomeDeductions_IncomeTransactionId" ON "IncomeTransactionIncomeDeductions" ("IncomeTransactionId");

CREATE INDEX "IX_IncomeTransactionIncomeDestinationFundAssignments_IncomeDestinationId" ON "IncomeTransactionIncomeDestinationFundAssignments" ("IncomeDestinationId");

CREATE INDEX "IX_IncomeTransactionIncomeDestinations_AccountId" ON "IncomeTransactionIncomeDestinations" ("AccountId");

CREATE INDEX "IX_IncomeTransactionIncomeDestinations_IncomeTransactionId" ON "IncomeTransactionIncomeDestinations" ("IncomeTransactionId");

CREATE INDEX "IX_IncomeTransactionIncomeLines_IncomeTransactionId" ON "IncomeTransactionIncomeLines" ("IncomeTransactionId");

CREATE INDEX "IX_PendingAccountBalanceEffects_AccountId" ON "PendingAccountBalanceEffects" ("AccountId");

CREATE UNIQUE INDEX "IX_PendingAccountBalanceEffects_TransactionId_AccountId" ON "PendingAccountBalanceEffects" ("TransactionId", "AccountId");

CREATE INDEX "IX_PendingFundBalanceEffects_FundId" ON "PendingFundBalanceEffects" ("FundId");

CREATE UNIQUE INDEX "IX_PendingFundBalanceEffects_TransactionId_FundId" ON "PendingFundBalanceEffects" ("TransactionId", "FundId");

CREATE INDEX "IX_PendingFundGoalTotalsEffects_FundId_AccountingPeriodId" ON "PendingFundGoalTotalsEffects" ("FundId", "AccountingPeriodId");

CREATE UNIQUE INDEX "IX_PendingFundGoalTotalsEffects_TransactionId_FundId_AccountingPeriodId" ON "PendingFundGoalTotalsEffects" ("TransactionId", "FundId", "AccountingPeriodId");

CREATE INDEX "IX_SpendingTransactionDestinationFundAssignments_DestinationId" ON "SpendingTransactionDestinationFundAssignments" ("DestinationId");

CREATE INDEX "IX_SpendingTransactionDestinations_CreditAccountId" ON "SpendingTransactionDestinations" ("CreditAccountId");

CREATE INDEX "IX_SpendingTransactionDestinations_SpendingTransactionId" ON "SpendingTransactionDestinations" ("SpendingTransactionId");

CREATE INDEX "IX_Transactions_AccountTransaction_DebitAccountId" ON "Transactions" ("AccountTransaction_DebitAccountId");

CREATE UNIQUE INDEX "IX_Transactions_Date_Sequence" ON "Transactions" ("Date", "Sequence");

CREATE INDEX "IX_Transactions_FundTransaction_DebitFundId" ON "Transactions" ("FundTransaction_DebitFundId");

CREATE INDEX "IX_Transactions_IncomeTransaction_SourceAccountId" ON "Transactions" ("IncomeTransaction_SourceAccountId");

CREATE INDEX "IX_Transactions_SpendingTransaction_DebitAccountId" ON "Transactions" ("SpendingTransaction_DebitAccountId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260726155805_InitialCreate', '10.0.9');

COMMIT;

