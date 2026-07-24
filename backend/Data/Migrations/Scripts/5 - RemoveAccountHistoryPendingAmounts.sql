ALTER TABLE "AccountBalanceHistories" DROP COLUMN "PendingCreditAmount";
ALTER TABLE "AccountBalanceHistories" DROP COLUMN "PendingDebitAmount";
ALTER TABLE "AccountingPeriodFundPlanTotals" DROP COLUMN "PendingAmountAssigned";
ALTER TABLE "AccountingPeriodFundPlanTotals" DROP COLUMN "PendingAmountSpent";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20260724090000_RemoveAccountHistoryPendingAmounts', '10.0.9');
