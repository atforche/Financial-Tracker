import {
  type Transaction,
  asAccountTransaction,
  asIncomeTransaction,
  asRefundTransaction,
  asSpendingTransaction,
} from "@/transactions/types";
import type { AccountBalanceEvent } from "@/accounts/types";
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

  const refundTransaction = asRefundTransaction(transaction);
  if (refundTransaction !== null) {
    return [
      ...refundTransaction.sources.map((source) => source.account),
      refundTransaction.destination.account,
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
          postedDate: event.eventDate ?? null,
        });
        return accounts;
      }, new Map<string, TransactionPostingAccount>())
      .values(),
  );
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

export { getPostedTransactionAccounts };
