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
    "PostedBalance" TEXT NOT NULL
);

CREATE TABLE "Funds" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Funds" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "AddAccountingPeriodId" TEXT NOT NULL,
    "AddDate" TEXT NOT NULL
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
    "AvailableToSpend" TEXT NULL,
    CONSTRAINT "FK_AccountBalanceHistories_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Transactions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Transactions" PRIMARY KEY,
    "AccountingPeriodId" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL,
    "Location" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "DebitAccount_AccountId" TEXT NULL,
    "DebitAccount_Type" INTEGER NULL,
    "DebitAccount_PostedDate" TEXT NULL,
    "CreditAccount_AccountId" TEXT NULL,
    "CreditAccount_Type" INTEGER NULL,
    "CreditAccount_PostedDate" TEXT NULL,
    "GeneratedByAccountId" TEXT NULL,
    CONSTRAINT "FK_Transactions_Accounts_CreditAccount_AccountId" FOREIGN KEY ("CreditAccount_AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Transactions_Accounts_DebitAccount_AccountId" FOREIGN KEY ("DebitAccount_AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "FundBalanceHistoryAccountBalances" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_FundBalanceHistoryAccountBalances" PRIMARY KEY AUTOINCREMENT,
    "AccountId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "FundBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_FundBalanceHistoryAccountBalances_FundBalanceHistories_FundBalanceHistoryId" FOREIGN KEY ("FundBalanceHistoryId") REFERENCES "FundBalanceHistories" ("Id") ON DELETE CASCADE
);

CREATE TABLE "FundBalanceHistoryPendingCredits" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_FundBalanceHistoryPendingCredits" PRIMARY KEY AUTOINCREMENT,
    "AccountId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "FundBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_FundBalanceHistoryPendingCredits_FundBalanceHistories_FundBalanceHistoryId" FOREIGN KEY ("FundBalanceHistoryId") REFERENCES "FundBalanceHistories" ("Id") ON DELETE CASCADE
);

CREATE TABLE "FundBalanceHistoryPendingDebits" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_FundBalanceHistoryPendingDebits" PRIMARY KEY AUTOINCREMENT,
    "AccountId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "FundBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_FundBalanceHistoryPendingDebits_FundBalanceHistories_FundBalanceHistoryId" FOREIGN KEY ("FundBalanceHistoryId") REFERENCES "FundBalanceHistories" ("Id") ON DELETE CASCADE
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
    "ClosingBalance" TEXT NOT NULL,
    "AccountingPeriodBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_FundAccountingPeriodBalanceHistory_AccountingPeriodBalanceHistories_AccountingPeriodBalanceHistoryId" FOREIGN KEY ("AccountingPeriodBalanceHistoryId") REFERENCES "AccountingPeriodBalanceHistories" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_FundAccountingPeriodBalanceHistory_AccountingPeriods_AccountingPeriodId" FOREIGN KEY ("AccountingPeriodId") REFERENCES "AccountingPeriods" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_FundAccountingPeriodBalanceHistory_Funds_FundId" FOREIGN KEY ("FundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AccountBalanceHistoryFundBalances" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AccountBalanceHistoryFundBalances" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "AccountBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_AccountBalanceHistoryFundBalances_AccountBalanceHistories_AccountBalanceHistoryId" FOREIGN KEY ("AccountBalanceHistoryId") REFERENCES "AccountBalanceHistories" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AccountBalanceHistoryPendingCredits" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AccountBalanceHistoryPendingCredits" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "AccountBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_AccountBalanceHistoryPendingCredits_AccountBalanceHistories_AccountBalanceHistoryId" FOREIGN KEY ("AccountBalanceHistoryId") REFERENCES "AccountBalanceHistories" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AccountBalanceHistoryPendingDebits" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AccountBalanceHistoryPendingDebits" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "AccountBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_AccountBalanceHistoryPendingDebits_AccountBalanceHistories_AccountBalanceHistoryId" FOREIGN KEY ("AccountBalanceHistoryId") REFERENCES "AccountBalanceHistories" ("Id") ON DELETE CASCADE
);

CREATE TABLE "TransactionCreditAccountFundAmounts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_TransactionCreditAccountFundAmounts" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "TransactionAccountTransactionId" TEXT NOT NULL,
    CONSTRAINT "FK_TransactionCreditAccountFundAmounts_Transactions_TransactionAccountTransactionId" FOREIGN KEY ("TransactionAccountTransactionId") REFERENCES "Transactions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "TransactionDebitAccountFundAmounts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_TransactionDebitAccountFundAmounts" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "TransactionAccountTransactionId" TEXT NOT NULL,
    CONSTRAINT "FK_TransactionDebitAccountFundAmounts_Transactions_TransactionAccountTransactionId" FOREIGN KEY ("TransactionAccountTransactionId") REFERENCES "Transactions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AccountAccountingPeriodBalanceHistoryClosingFundBalances" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AccountAccountingPeriodBalanceHistoryClosingFundBalances" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "AccountAccountingPeriodBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_AccountAccountingPeriodBalanceHistoryClosingFundBalances_AccountAccountingPeriodBalanceHistory_AccountAccountingPeriodBalanceHistoryId" FOREIGN KEY ("AccountAccountingPeriodBalanceHistoryId") REFERENCES "AccountAccountingPeriodBalanceHistory" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AccountAccountingPeriodBalanceHistoryOpeningFundBalances" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AccountAccountingPeriodBalanceHistoryOpeningFundBalances" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "AccountAccountingPeriodBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_AccountAccountingPeriodBalanceHistoryOpeningFundBalances_AccountAccountingPeriodBalanceHistory_AccountAccountingPeriodBalanceHistoryId" FOREIGN KEY ("AccountAccountingPeriodBalanceHistoryId") REFERENCES "AccountAccountingPeriodBalanceHistory" ("Id") ON DELETE CASCADE
);

CREATE TABLE "FundAccountingPeriodBalanceHistoryClosingAccountBalances" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_FundAccountingPeriodBalanceHistoryClosingAccountBalances" PRIMARY KEY AUTOINCREMENT,
    "AccountId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "FundAccountingPeriodBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_FundAccountingPeriodBalanceHistoryClosingAccountBalances_FundAccountingPeriodBalanceHistory_FundAccountingPeriodBalanceHistoryId" FOREIGN KEY ("FundAccountingPeriodBalanceHistoryId") REFERENCES "FundAccountingPeriodBalanceHistory" ("Id") ON DELETE CASCADE
);

CREATE TABLE "FundAccountingPeriodBalanceHistoryOpeningAccountBalances" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_FundAccountingPeriodBalanceHistoryOpeningAccountBalances" PRIMARY KEY AUTOINCREMENT,
    "AccountId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "FundAccountingPeriodBalanceHistoryId" TEXT NOT NULL,
    CONSTRAINT "FK_FundAccountingPeriodBalanceHistoryOpeningAccountBalances_FundAccountingPeriodBalanceHistory_FundAccountingPeriodBalanceHistoryId" FOREIGN KEY ("FundAccountingPeriodBalanceHistoryId") REFERENCES "FundAccountingPeriodBalanceHistory" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AccountAccountingPeriodBalanceHistory_AccountId" ON "AccountAccountingPeriodBalanceHistory" ("AccountId");

CREATE INDEX "IX_AccountAccountingPeriodBalanceHistory_AccountingPeriodBalanceHistoryId" ON "AccountAccountingPeriodBalanceHistory" ("AccountingPeriodBalanceHistoryId");

CREATE INDEX "IX_AccountAccountingPeriodBalanceHistory_AccountingPeriodId" ON "AccountAccountingPeriodBalanceHistory" ("AccountingPeriodId");

CREATE INDEX "IX_AccountAccountingPeriodBalanceHistoryClosingFundBalances_AccountAccountingPeriodBalanceHistoryId" ON "AccountAccountingPeriodBalanceHistoryClosingFundBalances" ("AccountAccountingPeriodBalanceHistoryId");

CREATE INDEX "IX_AccountAccountingPeriodBalanceHistoryOpeningFundBalances_AccountAccountingPeriodBalanceHistoryId" ON "AccountAccountingPeriodBalanceHistoryOpeningFundBalances" ("AccountAccountingPeriodBalanceHistoryId");

CREATE INDEX "IX_AccountBalanceHistories_AccountId" ON "AccountBalanceHistories" ("AccountId");

CREATE INDEX "IX_AccountBalanceHistoryFundBalances_AccountBalanceHistoryId" ON "AccountBalanceHistoryFundBalances" ("AccountBalanceHistoryId");

CREATE INDEX "IX_AccountBalanceHistoryPendingCredits_AccountBalanceHistoryId" ON "AccountBalanceHistoryPendingCredits" ("AccountBalanceHistoryId");

CREATE INDEX "IX_AccountBalanceHistoryPendingDebits_AccountBalanceHistoryId" ON "AccountBalanceHistoryPendingDebits" ("AccountBalanceHistoryId");

CREATE INDEX "IX_AccountingPeriodBalanceHistories_AccountingPeriodId" ON "AccountingPeriodBalanceHistories" ("AccountingPeriodId");

CREATE INDEX "IX_AccountingPeriods_Name" ON "AccountingPeriods" ("Name");

CREATE UNIQUE INDEX "IX_Accounts_Name" ON "Accounts" ("Name");

CREATE INDEX "IX_FundAccountingPeriodBalanceHistory_AccountingPeriodBalanceHistoryId" ON "FundAccountingPeriodBalanceHistory" ("AccountingPeriodBalanceHistoryId");

CREATE INDEX "IX_FundAccountingPeriodBalanceHistory_AccountingPeriodId" ON "FundAccountingPeriodBalanceHistory" ("AccountingPeriodId");

CREATE INDEX "IX_FundAccountingPeriodBalanceHistory_FundId" ON "FundAccountingPeriodBalanceHistory" ("FundId");

CREATE INDEX "IX_FundAccountingPeriodBalanceHistoryClosingAccountBalances_FundAccountingPeriodBalanceHistoryId" ON "FundAccountingPeriodBalanceHistoryClosingAccountBalances" ("FundAccountingPeriodBalanceHistoryId");

CREATE INDEX "IX_FundAccountingPeriodBalanceHistoryOpeningAccountBalances_FundAccountingPeriodBalanceHistoryId" ON "FundAccountingPeriodBalanceHistoryOpeningAccountBalances" ("FundAccountingPeriodBalanceHistoryId");

CREATE INDEX "IX_FundBalanceHistoryAccountBalances_FundBalanceHistoryId" ON "FundBalanceHistoryAccountBalances" ("FundBalanceHistoryId");

CREATE INDEX "IX_FundBalanceHistoryPendingCredits_FundBalanceHistoryId" ON "FundBalanceHistoryPendingCredits" ("FundBalanceHistoryId");

CREATE INDEX "IX_FundBalanceHistoryPendingDebits_FundBalanceHistoryId" ON "FundBalanceHistoryPendingDebits" ("FundBalanceHistoryId");

CREATE UNIQUE INDEX "IX_Funds_Name" ON "Funds" ("Name");

CREATE INDEX "IX_TransactionCreditAccountFundAmounts_TransactionAccountTransactionId" ON "TransactionCreditAccountFundAmounts" ("TransactionAccountTransactionId");

CREATE INDEX "IX_TransactionDebitAccountFundAmounts_TransactionAccountTransactionId" ON "TransactionDebitAccountFundAmounts" ("TransactionAccountTransactionId");

CREATE INDEX "IX_Transactions_CreditAccount_AccountId" ON "Transactions" ("CreditAccount_AccountId");

CREATE INDEX "IX_Transactions_DebitAccount_AccountId" ON "Transactions" ("DebitAccount_AccountId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260322153237_InitialCreate', '10.0.2');

COMMIT;

