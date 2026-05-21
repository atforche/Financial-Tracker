BEGIN TRANSACTION;
CREATE TABLE "ef_temp_AccountBalanceHistories" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AccountBalanceHistories" PRIMARY KEY,
    "AccountId" TEXT NOT NULL,
    "Date" TEXT NOT NULL,
    "PendingCreditAmount" TEXT NOT NULL,
    "PendingDebitAmount" TEXT NOT NULL,
    "PostedBalance" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL,
    "TransactionId" TEXT NOT NULL,
    CONSTRAINT "FK_AccountBalanceHistories_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_AccountBalanceHistories" ("Id", "AccountId", "Date", "PendingCreditAmount", "PendingDebitAmount", "PostedBalance", "Sequence", "TransactionId")
SELECT "Id", "AccountId", "Date", "PendingCreditAmount", "PendingDebitAmount", "PostedBalance", "Sequence", "TransactionId"
FROM "AccountBalanceHistories";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "AccountBalanceHistories";

ALTER TABLE "ef_temp_AccountBalanceHistories" RENAME TO "AccountBalanceHistories";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_AccountBalanceHistories_AccountId" ON "AccountBalanceHistories" ("AccountId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260521131438_RemoveAvailableToSpend', '10.0.2');

