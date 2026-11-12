import { TransactionType } from "@/transactions/types";

/**
 * Display labels for each transaction type.
 */
const transactionTypeLabels: Readonly<Record<TransactionType, string>> = {
  [TransactionType.Spending]: "Spending",
  [TransactionType.Refund]: "Refund",
  [TransactionType.Income]: "Income",
  [TransactionType.Account]: "Account",
  [TransactionType.Fund]: "Fund",
  [TransactionType.BalanceAdjustment]: "Balance adjustment",
};

/**
 * Transaction types in the order they are offered when creating a transaction.
 */
const transactionTypeOptions: readonly TransactionType[] = [
  TransactionType.Spending,
  TransactionType.Refund,
  TransactionType.Income,
  TransactionType.Account,
  TransactionType.Fund,
  TransactionType.BalanceAdjustment,
];

/**
 * Gets the display label for a transaction type.
 */
const getTransactionTypeLabel = function (type: TransactionType): string {
  return transactionTypeLabels[type];
};

export { getTransactionTypeLabel, transactionTypeOptions };
