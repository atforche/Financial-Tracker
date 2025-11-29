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

CREATE TABLE "Funds" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Funds" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL
);

CREATE TABLE "AccountAddedBalanceEvent" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AccountAddedBalanceEvent" PRIMARY KEY,
    "AccountingPeriodId" TEXT NOT NULL,
    "EventDate" TEXT NOT NULL,
    "EventSequence" INTEGER NOT NULL,
    CONSTRAINT "FK_AccountAddedBalanceEvent_AccountingPeriods_AccountingPeriodId" FOREIGN KEY ("AccountingPeriodId") REFERENCES "AccountingPeriods" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Accounts" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Accounts" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Type" TEXT NOT NULL,
    "AccountAddedBalanceEventId" TEXT NOT NULL,
    CONSTRAINT "FK_Accounts_AccountAddedBalanceEvent_AccountAddedBalanceEventId" FOREIGN KEY ("AccountAddedBalanceEventId") REFERENCES "AccountAddedBalanceEvent" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AccountBalanceCheckpoint" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AccountBalanceCheckpoint" PRIMARY KEY,
    "AccountId" TEXT NOT NULL,
    "AccountingPeriodId" TEXT NOT NULL,
    CONSTRAINT "FK_AccountBalanceCheckpoint_AccountingPeriods_AccountingPeriodId" FOREIGN KEY ("AccountingPeriodId") REFERENCES "AccountingPeriods" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AccountBalanceCheckpoint_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "FundConversions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_FundConversions" PRIMARY KEY,
    "AccountingPeriodId" TEXT NOT NULL,
    "EventDate" TEXT NOT NULL,
    "EventSequence" INTEGER NOT NULL,
    "AccountId" TEXT NOT NULL,
    "FromFundId" TEXT NOT NULL,
    "ToFundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    CONSTRAINT "FK_FundConversions_AccountingPeriods_AccountingPeriodId" FOREIGN KEY ("AccountingPeriodId") REFERENCES "AccountingPeriods" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_FundConversions_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_FundConversions_Funds_FromFundId" FOREIGN KEY ("FromFundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_FundConversions_Funds_ToFundId" FOREIGN KEY ("ToFundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Transactions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Transactions" PRIMARY KEY,
    "AccountingPeriodId" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "DebitAccountId" TEXT NULL,
    "CreditAccountId" TEXT NULL,
    CONSTRAINT "FK_Transactions_AccountingPeriods_AccountingPeriodId" FOREIGN KEY ("AccountingPeriodId") REFERENCES "AccountingPeriods" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Transactions_Accounts_CreditAccountId" FOREIGN KEY ("CreditAccountId") REFERENCES "Accounts" ("Id"),
    CONSTRAINT "FK_Transactions_Accounts_DebitAccountId" FOREIGN KEY ("DebitAccountId") REFERENCES "Accounts" ("Id")
);

CREATE TABLE "FundAmount" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_FundAmount" PRIMARY KEY AUTOINCREMENT,
    "FundId" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "AccountAddedBalanceEventId" TEXT NULL,
    "AccountBalanceCheckpointId" TEXT NULL,
    "CreditTransactionId" TEXT NULL,
    "DebitTransactionId" TEXT NULL,
    CONSTRAINT "FK_FundAmount_AccountAddedBalanceEvent_AccountAddedBalanceEventId" FOREIGN KEY ("AccountAddedBalanceEventId") REFERENCES "AccountAddedBalanceEvent" ("Id"),
    CONSTRAINT "FK_FundAmount_AccountBalanceCheckpoint_AccountBalanceCheckpointId" FOREIGN KEY ("AccountBalanceCheckpointId") REFERENCES "AccountBalanceCheckpoint" ("Id"),
    CONSTRAINT "FK_FundAmount_Funds_FundId" FOREIGN KEY ("FundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_FundAmount_Transactions_CreditTransactionId" FOREIGN KEY ("CreditTransactionId") REFERENCES "Transactions" ("Id"),
    CONSTRAINT "FK_FundAmount_Transactions_DebitTransactionId" FOREIGN KEY ("DebitTransactionId") REFERENCES "Transactions" ("Id")
);

CREATE TABLE "TransactionBalanceEvent" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_TransactionBalanceEvent" PRIMARY KEY,
    "TransactionId" TEXT NOT NULL,
    "EventDate" TEXT NOT NULL,
    "EventSequence" INTEGER NOT NULL,
    CONSTRAINT "FK_TransactionBalanceEvent_Transactions_TransactionId" FOREIGN KEY ("TransactionId") REFERENCES "Transactions" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ChangeInValues" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_ChangeInValues" PRIMARY KEY,
    "AccountingPeriodId" TEXT NOT NULL,
    "EventDate" TEXT NOT NULL,
    "EventSequence" INTEGER NOT NULL,
    "AccountId" TEXT NOT NULL,
    "FundAmountId" INTEGER NOT NULL,
    CONSTRAINT "FK_ChangeInValues_AccountingPeriods_AccountingPeriodId" FOREIGN KEY ("AccountingPeriodId") REFERENCES "AccountingPeriods" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ChangeInValues_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ChangeInValues_FundAmount_FundAmountId" FOREIGN KEY ("FundAmountId") REFERENCES "FundAmount" ("Id") ON DELETE CASCADE
);

CREATE TABLE "TransactionBalanceEventPart" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_TransactionBalanceEventPart" PRIMARY KEY,
    "TransactionBalanceEventId" TEXT NOT NULL,
    "Type" TEXT NOT NULL,
    CONSTRAINT "FK_TransactionBalanceEventPart_TransactionBalanceEvent_TransactionBalanceEventId" FOREIGN KEY ("TransactionBalanceEventId") REFERENCES "TransactionBalanceEvent" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AccountAddedBalanceEvent_AccountingPeriodId" ON "AccountAddedBalanceEvent" ("AccountingPeriodId");

CREATE INDEX "IX_AccountBalanceCheckpoint_AccountId" ON "AccountBalanceCheckpoint" ("AccountId");

CREATE INDEX "IX_AccountBalanceCheckpoint_AccountingPeriodId" ON "AccountBalanceCheckpoint" ("AccountingPeriodId");

CREATE UNIQUE INDEX "IX_Accounts_AccountAddedBalanceEventId" ON "Accounts" ("AccountAddedBalanceEventId");

CREATE UNIQUE INDEX "IX_Accounts_Name" ON "Accounts" ("Name");

CREATE INDEX "IX_ChangeInValues_AccountId" ON "ChangeInValues" ("AccountId");

CREATE INDEX "IX_ChangeInValues_AccountingPeriodId" ON "ChangeInValues" ("AccountingPeriodId");

CREATE UNIQUE INDEX "IX_ChangeInValues_EventDate_EventSequence" ON "ChangeInValues" ("EventDate", "EventSequence");

CREATE UNIQUE INDEX "IX_ChangeInValues_FundAmountId" ON "ChangeInValues" ("FundAmountId");

CREATE INDEX "IX_FundAmount_AccountAddedBalanceEventId" ON "FundAmount" ("AccountAddedBalanceEventId");

CREATE INDEX "IX_FundAmount_AccountBalanceCheckpointId" ON "FundAmount" ("AccountBalanceCheckpointId");

CREATE INDEX "IX_FundAmount_CreditTransactionId" ON "FundAmount" ("CreditTransactionId");

CREATE INDEX "IX_FundAmount_DebitTransactionId" ON "FundAmount" ("DebitTransactionId");

CREATE INDEX "IX_FundAmount_FundId" ON "FundAmount" ("FundId");

CREATE INDEX "IX_FundConversions_AccountId" ON "FundConversions" ("AccountId");

CREATE INDEX "IX_FundConversions_AccountingPeriodId" ON "FundConversions" ("AccountingPeriodId");

CREATE UNIQUE INDEX "IX_FundConversions_EventDate_EventSequence" ON "FundConversions" ("EventDate", "EventSequence");

CREATE INDEX "IX_FundConversions_FromFundId" ON "FundConversions" ("FromFundId");

CREATE INDEX "IX_FundConversions_ToFundId" ON "FundConversions" ("ToFundId");

CREATE UNIQUE INDEX "IX_Funds_Name" ON "Funds" ("Name");

CREATE UNIQUE INDEX "IX_TransactionBalanceEvent_EventDate_EventSequence" ON "TransactionBalanceEvent" ("EventDate", "EventSequence");

CREATE INDEX "IX_TransactionBalanceEvent_TransactionId" ON "TransactionBalanceEvent" ("TransactionId");

CREATE INDEX "IX_TransactionBalanceEventPart_TransactionBalanceEventId" ON "TransactionBalanceEventPart" ("TransactionBalanceEventId");

CREATE INDEX "IX_Transactions_AccountingPeriodId" ON "Transactions" ("AccountingPeriodId");

CREATE INDEX "IX_Transactions_CreditAccountId" ON "Transactions" ("CreditAccountId");

CREATE INDEX "IX_Transactions_DebitAccountId" ON "Transactions" ("DebitAccountId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250906004747_InitialCreate', '9.0.5');

COMMIT;

