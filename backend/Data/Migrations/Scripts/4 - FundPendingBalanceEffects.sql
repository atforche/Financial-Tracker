BEGIN TRANSACTION;
CREATE TABLE "PendingFundBalanceEffects" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_PendingFundBalanceEffects" PRIMARY KEY,
    "FundId" TEXT NOT NULL,
    "TransactionId" TEXT NOT NULL,
    "PendingDebitAmount" TEXT NOT NULL,
    "PendingCreditAmount" TEXT NOT NULL,
    CONSTRAINT "FK_PendingFundBalanceEffects_Funds_FundId" FOREIGN KEY ("FundId") REFERENCES "Funds" ("Id") ON DELETE CASCADE
);
CREATE INDEX "IX_PendingFundBalanceEffects_FundId" ON "PendingFundBalanceEffects" ("FundId");
CREATE UNIQUE INDEX "IX_PendingFundBalanceEffects_TransactionId_FundId" ON "PendingFundBalanceEffects" ("TransactionId", "FundId");
CREATE TABLE "PendingFundPlanTotalsEffects" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_PendingFundPlanTotalsEffects" PRIMARY KEY,
    "FundId" TEXT NOT NULL,
    "AccountingPeriodId" TEXT NOT NULL,
    "TransactionId" TEXT NOT NULL,
    "PendingAmountAssigned" TEXT NOT NULL,
    "PendingAmountSpent" TEXT NOT NULL
);
CREATE INDEX "IX_PendingFundPlanTotalsEffects_FundId_AccountingPeriodId" ON "PendingFundPlanTotalsEffects" ("FundId", "AccountingPeriodId");
CREATE UNIQUE INDEX "IX_PendingFundPlanTotalsEffects_TransactionId_FundId_AccountingPeriodId" ON "PendingFundPlanTotalsEffects" ("TransactionId", "FundId", "AccountingPeriodId");
ALTER TABLE "FundBalanceHistories" DROP COLUMN "PendingCreditAmount";
ALTER TABLE "FundBalanceHistories" DROP COLUMN "PendingDebitAmount";
ALTER TABLE "FundPlanTotalsHistories" DROP COLUMN "PendingAmountAssigned";
ALTER TABLE "FundPlanTotalsHistories" DROP COLUMN "PendingAmountSpent";
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20260723200000_FundPendingBalanceEffects', '10.0.9');
COMMIT;
