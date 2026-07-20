import {
  type Transaction,
  asAccountTransaction,
  asFundTransaction,
  asIncomeTransaction,
  asSpendingTransaction,
} from "@/transactions/types";
import type { AccountBalanceEvent } from "@/accounts/types";
import type { FundBalanceEvent } from "@/funds/types";
import type { FundPlanBalanceEvent } from "@/goals/types";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";

/**
 * Type representing an account involved in a transaction posting.
 */
interface TransactionPostingAccount {
  readonly accountId: string;
  readonly accountName: string;
  readonly postedDate: string | null;
}

/**
 * Gets the Account balance events captured by the provided Transaction.
 */
const getTransactionAccountBalanceEvents = function (
  transaction: Transaction,
): AccountBalanceEvent[] {
  const spendingTransaction = asSpendingTransaction(transaction);
  if (spendingTransaction !== null) {
    return [
      spendingTransaction.source.account,
      ...spendingTransaction.destinations.map(
        (destination) => destination.account,
      ),
    ].filter(isNotNullOrUndefined);
  }

  const incomeTransaction = asIncomeTransaction(transaction);
  if (incomeTransaction !== null) {
    return [
      incomeTransaction.source.account,
      ...incomeTransaction.destinations.map(
        (destination) => destination.account,
      ),
    ].filter(isNotNullOrUndefined);
  }

  const accountTransaction = asAccountTransaction(transaction);
  if (accountTransaction !== null) {
    return [
      accountTransaction.source.account,
      ...accountTransaction.destinations.map(
        (destination) => destination.account,
      ),
    ].filter(isNotNullOrUndefined);
  }
  return [];
};

/**
 * Gets the account IDs involved in the provided transaction.
 */
const getTransactionAccountIds = function (transaction: Transaction): string[] {
  return Array.from(
    new Set(
      getTransactionAccountBalanceEvents(transaction).map(
        (event) => event.account.id,
      ),
    ),
  );
};

/**
 * Gets the Fund balance events captured by the provided Transaction.
 */
const getTransactionFundBalanceEvents = function (
  transaction: Transaction,
): FundBalanceEvent[] {
  const spendingTransaction = asSpendingTransaction(transaction);
  if (spendingTransaction !== null) {
    return spendingTransaction.destinations.flatMap(
      (destination) => destination.fundAssignments,
    );
  }

  const incomeTransaction = asIncomeTransaction(transaction);
  if (incomeTransaction !== null) {
    return incomeTransaction.destinations.flatMap(
      (destination) => destination.fundAssignments,
    );
  }

  const fundTransaction = asFundTransaction(transaction);
  if (fundTransaction !== null) {
    return [
      fundTransaction.source.fund,
      ...fundTransaction.destinations.map((destination) => destination.fund),
    ];
  }
  return [];
};

/**
 * Gets the fund IDs involved in the provided transaction.
 */
const getTransactionFundIds = function (transaction: Transaction): string[] {
  return Array.from(
    new Set(
      getTransactionFundBalanceEvents(transaction).map(
        (event) => event.fund.id,
      ),
    ),
  );
};

/**
 * Gets the Goal balance events captured by the provided Transaction.
 */
const getTransactionFundPlanBalanceEvents = function (
  transaction: Transaction,
): FundPlanBalanceEvent[] {
  const spendingTransaction = asSpendingTransaction(transaction);
  if (spendingTransaction !== null) {
    return spendingTransaction.destinations.flatMap(
      (destination) => destination.fundPlans,
    );
  }

  const incomeTransaction = asIncomeTransaction(transaction);
  if (incomeTransaction !== null) {
    return incomeTransaction.destinations.flatMap(
      (destination) => destination.fundPlans,
    );
  }

  const fundTransaction = asFundTransaction(transaction);
  if (fundTransaction !== null) {
    return [
      fundTransaction.source.fundPlan,
      ...fundTransaction.destinations.map(
        (destination) => destination.fundPlan,
      ),
    ].filter(isNotNullOrUndefined);
  }
  return [];
};

/**
 * Gets the Transaction Posting Accounts for the provided Transaction.
 */
const collectTransactionPostingAccounts = function (
  transaction: Transaction,
): TransactionPostingAccount[] {
  return Array.from(
    getTransactionAccountBalanceEvents(transaction)
      .reduce((accounts, event) => {
        accounts.set(event.account.id, {
          accountId: event.account.id,
          accountName: event.account.name,
          postedDate: event.date ?? null,
        });
        return accounts;
      }, new Map<string, TransactionPostingAccount>())
      .values(),
  );
};

/**
 * Gets the accounts involved in the provided transaction that have not been posted.
 */
const getPostableTransactionAccounts = function (
  transaction: Transaction,
): { accountId: string; accountName: string }[] {
  return collectTransactionPostingAccounts(transaction)
    .filter((account) => account.postedDate === null)
    .map((account) => ({
      accountId: account.accountId,
      accountName: account.accountName,
    }));
};

/**
 * Gets the accounts involved in the provided transaction that have been posted.
 */
const getPostedTransactionAccounts = function (
  transaction: Transaction,
): { accountId: string; accountName: string; postedDate: string | null }[] {
  return collectTransactionPostingAccounts(transaction)
    .filter((account) => account.postedDate !== null)
    .map((account) => ({
      accountId: account.accountId,
      accountName: account.accountName,
      postedDate: account.postedDate,
    }));
};

export {
  getTransactionAccountIds,
  getTransactionAccountBalanceEvents,
  getTransactionFundIds,
  getTransactionFundBalanceEvents,
  getTransactionFundPlanBalanceEvents,
  getPostableTransactionAccounts,
  getPostedTransactionAccounts,
};
