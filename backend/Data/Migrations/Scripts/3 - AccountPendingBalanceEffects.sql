BEGIN TRANSACTION;
CREATE TABLE "PendingAccountBalanceEffects" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_PendingAccountBalanceEffects" PRIMARY KEY,
    "AccountId" TEXT NOT NULL,
    "TransactionId" TEXT NOT NULL,
    "PendingDebitAmount" TEXT NOT NULL,
    "PendingCreditAmount" TEXT NOT NULL,
    CONSTRAINT "FK_PendingAccountBalanceEffects_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_PendingAccountBalanceEffects_AccountId" ON "PendingAccountBalanceEffects" ("AccountId");

CREATE UNIQUE INDEX "IX_PendingAccountBalanceEffects_TransactionId_AccountId" ON "PendingAccountBalanceEffects" ("TransactionId", "AccountId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260723151024_AccountPendingBalanceEffects', '10.0.9');

COMMIT;

