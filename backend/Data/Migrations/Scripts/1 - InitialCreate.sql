CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "AccountBalanceHistories" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AccountBalanceHistories" PRIMARY KEY,
    "AccountId" TEXT NOT NULL,
    "TransactionId" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL
);

CREATE TABLE "AccountingPeriods" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AccountingPeriods" PRIMARY KEY,
    "Year" INTEGER NOT NULL,
    "Month" INTEGER NOT NULL,
    "IsOpen" INTEGER NOT NULL
);

CREATE TABLE "Accounts" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Accounts" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Type" TEXT NOT NULL,
    "InitialTransaction" TEXT NULL
);

CREATE TABLE "FundBalanceHistories" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_FundBalanceHistories" PRIMARY KEY,
    "FundId" TEXT NOT NULL,
    "TransactionId" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL
);

CREATE TABLE "Funds" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Funds" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL
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

CREATE TABLE "TransactionAccount" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_TransactionAccount" PRIMARY KEY AUTOINCREMENT,
    "Account" TEXT NOT NULL,
    "PostedDate" TEXT NULL,
    CONSTRAINT "FK_TransactionAccount_Accounts_Account" FOREIGN KEY ("Account") REFERENCES "Accounts" ("Id") ON DELETE CASCADE
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

CREATE TABLE "TransactionAccountFundAmounts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_TransactionAccountFundAmounts" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "TransactionAccountId" INTEGER NOT NULL,
    CONSTRAINT "FK_TransactionAccountFundAmounts_TransactionAccount_TransactionAccountId" FOREIGN KEY ("TransactionAccountId") REFERENCES "TransactionAccount" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Transactions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Transactions" PRIMARY KEY,
    "AccountingPeriod" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "Location" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "DebitAccountId" INTEGER NULL,
    "CreditAccountId" INTEGER NULL,
    "InitialAccountTransaction" TEXT NULL,
    CONSTRAINT "FK_Transactions_TransactionAccount_CreditAccountId" FOREIGN KEY ("CreditAccountId") REFERENCES "TransactionAccount" ("Id"),
    CONSTRAINT "FK_Transactions_TransactionAccount_DebitAccountId" FOREIGN KEY ("DebitAccountId") REFERENCES "TransactionAccount" ("Id")
);

CREATE INDEX "IX_AccountBalanceHistoryFundBalances_AccountBalanceHistoryId" ON "AccountBalanceHistoryFundBalances" ("AccountBalanceHistoryId");

CREATE INDEX "IX_AccountBalanceHistoryPendingCredits_AccountBalanceHistoryId" ON "AccountBalanceHistoryPendingCredits" ("AccountBalanceHistoryId");

CREATE INDEX "IX_AccountBalanceHistoryPendingDebits_AccountBalanceHistoryId" ON "AccountBalanceHistoryPendingDebits" ("AccountBalanceHistoryId");

CREATE UNIQUE INDEX "IX_Accounts_Name" ON "Accounts" ("Name");

CREATE INDEX "IX_FundBalanceHistoryAccountBalances_FundBalanceHistoryId" ON "FundBalanceHistoryAccountBalances" ("FundBalanceHistoryId");

CREATE INDEX "IX_FundBalanceHistoryPendingCredits_FundBalanceHistoryId" ON "FundBalanceHistoryPendingCredits" ("FundBalanceHistoryId");

CREATE INDEX "IX_FundBalanceHistoryPendingDebits_FundBalanceHistoryId" ON "FundBalanceHistoryPendingDebits" ("FundBalanceHistoryId");

CREATE UNIQUE INDEX "IX_Funds_Name" ON "Funds" ("Name");

CREATE INDEX "IX_TransactionAccount_Account" ON "TransactionAccount" ("Account");

CREATE INDEX "IX_TransactionAccountFundAmounts_TransactionAccountId" ON "TransactionAccountFundAmounts" ("TransactionAccountId");

CREATE INDEX "IX_Transactions_CreditAccountId" ON "Transactions" ("CreditAccountId");

CREATE INDEX "IX_Transactions_DebitAccountId" ON "Transactions" ("DebitAccountId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260130234029_InitialCreate', '10.0.2');

COMMIT;

