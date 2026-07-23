BEGIN TRANSACTION;
CREATE UNIQUE INDEX "IX_FundBalanceHistories_FundId_Date_Sequence" ON "FundBalanceHistories" ("FundId", "Date", "Sequence");

CREATE TABLE "ef_temp_FundBalanceHistories" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_FundBalanceHistories" PRIMARY KEY,
    "Date" TEXT NOT NULL,
    "FundId" TEXT NOT NULL,
    "PendingCreditAmount" TEXT NOT NULL,
    "PendingDebitAmount" TEXT NOT NULL,
    "PostedBalance" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL,
    "TransactionId" TEXT NOT NULL,
    CONSTRAINT "FK_FundBalanceHistories_Funds_FundId" FOREIGN KEY ("FundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_FundBalanceHistories" ("Id", "Date", "FundId", "PendingCreditAmount", "PendingDebitAmount", "PostedBalance", "Sequence", "TransactionId")
SELECT "Id", "Date", "FundId", "PendingCreditAmount", "PendingDebitAmount", "PostedBalance", "Sequence", "TransactionId"
FROM "FundBalanceHistories";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "FundBalanceHistories";

ALTER TABLE "ef_temp_FundBalanceHistories" RENAME TO "FundBalanceHistories";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE UNIQUE INDEX "IX_FundBalanceHistories_FundId_Date_Sequence" ON "FundBalanceHistories" ("FundId", "Date", "Sequence");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260723111003_FundBalanceAdjustment', '10.0.9');

