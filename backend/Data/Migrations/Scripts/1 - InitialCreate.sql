CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
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

CREATE TABLE "Funds" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Funds" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL
);

CREATE TABLE "TransactionAccount" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_TransactionAccount" PRIMARY KEY AUTOINCREMENT,
    "Account" TEXT NOT NULL,
    "PostedDate" TEXT NULL,
    CONSTRAINT "FK_TransactionAccount_Accounts_Account" FOREIGN KEY ("Account") REFERENCES "Accounts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "FundAmount" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_FundAmount" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "TransactionAccountId" INTEGER NULL,
    CONSTRAINT "FK_FundAmount_Funds_FundId" FOREIGN KEY ("FundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_FundAmount_TransactionAccount_TransactionAccountId" FOREIGN KEY ("TransactionAccountId") REFERENCES "TransactionAccount" ("Id")
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

CREATE UNIQUE INDEX "IX_Accounts_Name" ON "Accounts" ("Name");

CREATE INDEX "IX_FundAmount_FundId" ON "FundAmount" ("FundId");

CREATE INDEX "IX_FundAmount_TransactionAccountId" ON "FundAmount" ("TransactionAccountId");

CREATE UNIQUE INDEX "IX_Funds_Name" ON "Funds" ("Name");

CREATE INDEX "IX_TransactionAccount_Account" ON "TransactionAccount" ("Account");

CREATE INDEX "IX_Transactions_CreditAccountId" ON "Transactions" ("CreditAccountId");

CREATE INDEX "IX_Transactions_DebitAccountId" ON "Transactions" ("DebitAccountId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260125151334_InitialCreate', '10.0.2');

COMMIT;

