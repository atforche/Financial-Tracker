import { type Account, isTrackedAccountType } from "@/accounts/types";
import {
  CreateTransactionModelCreateAccountTransactionModelType,
  CreateTransactionModelCreateFundTransactionModelType,
  CreateTransactionModelCreateIncomeTransactionModelType,
  CreateTransactionModelCreateSpendingTransactionModelType,
  TransactionAccountTypeModel,
  TransactionModelAccountTransactionModelType,
  TransactionModelFundTransactionModelType,
  TransactionModelIncomeTransactionModelType,
  TransactionModelSpendingTransactionModelType,
  UpdateTransactionModelUpdateAccountTransactionModelType,
  UpdateTransactionModelUpdateFundTransactionModelType,
  UpdateTransactionModelUpdateIncomeTransactionModelType,
  UpdateTransactionModelUpdateSpendingTransactionModelType,
  type components,
} from "@/framework/data/api";
import type { Fund, FundAmount } from "@/funds/types";
import { hasIncompleteFundAssignments } from "@/funds/FundAssignmentEntryFrame";

/**
 * Type representing a Transaction.
 */
type Transaction = components["schemas"]["TransactionModel"];

/**
 * Type representing a Transaction Account.
 */
type TransactionAccount = components["schemas"]["TransactionAccountModel"];

/**
 * Type representing the request to create a transaction.
 */
type CreateTransactionRequest = components["schemas"]["CreateTransactionModel"];

/**
 * Type representing the request to update a transaction.
 */
type UpdateTransactionRequest = components["schemas"]["UpdateTransactionModel"];

/**
 * Type representing the request to post a transaction to an account.
 */
type PostTransactionRequest = components["schemas"]["PostTransactionModel"];

/**
 * Determines if the transaction being created or updated is an income transaction.
 */
const isIncomeTransaction = function (
  debitAccount: Account | null,
  creditAccount: Account | null,
  debitFund: Fund | null,
  creditFund: Fund | null,
): boolean {
  if (debitFund !== null || creditFund !== null) {
    return false;
  }
  if (debitAccount === null && creditAccount === null) {
    return false;
  }
  if (creditAccount === null || !isTrackedAccountType(creditAccount.type)) {
    return false;
  }
  if (debitAccount !== null && isTrackedAccountType(debitAccount.type)) {
    return false;
  }
  return true;
};

/**
 * Determines if the income transaction being created or updated has all required information to be created or updated.
 */
const isIncomeTransactionComplete = function (
  incomeFundAssignments: FundAmount[],
): boolean {
  return !hasIncompleteFundAssignments(incomeFundAssignments);
};

/**
 * Determines if the transaction being created or updated is a spending transaction.
 */
const isSpendingTransaction = function (
  debitAccount: Account | null,
  creditAccount: Account | null,
  debitFund: Fund | null,
  creditFund: Fund | null,
): boolean {
  if (debitFund !== null || creditFund !== null) {
    return false;
  }
  if (debitAccount === null && creditAccount === null) {
    return false;
  }
  if (creditAccount !== null && isTrackedAccountType(creditAccount.type)) {
    return false;
  }
  if (debitAccount === null || !isTrackedAccountType(debitAccount.type)) {
    return false;
  }
  return true;
};

/**
 * Determines if the spending transaction being created or updated has all required information to be created or updated.
 */
const isSpendingTransactionComplete = function (
  spendingFundAssignments: FundAmount[],
): boolean {
  return (
    !hasIncompleteFundAssignments(spendingFundAssignments) &&
    spendingFundAssignments.every(
      (fundAmount) =>
        fundAmount.fundName !== "Unassigned" || fundAmount.amount === 0,
    )
  );
};

/**
 * Determines if the transaction being created or updated is an account transaction.
 */
const isAccountTransaction = function (
  debitAccount: Account | null,
  creditAccount: Account | null,
  debitFund: Fund | null,
  creditFund: Fund | null,
): boolean {
  if (debitFund !== null || creditFund !== null) {
    return false;
  }
  if (debitAccount !== null && creditAccount !== null) {
    return (
      isTrackedAccountType(debitAccount.type) ===
      isTrackedAccountType(creditAccount.type)
    );
  }
  if (debitAccount !== null) {
    return !isTrackedAccountType(debitAccount.type);
  }
  if (creditAccount !== null) {
    return !isTrackedAccountType(creditAccount.type);
  }
  return false;
};

/**
 * Determines if the transaction being created or updated is a fund transaction.
 */
const isFundTransaction = function (
  debitAccount: Account | null,
  creditAccount: Account | null,
  debitFund: Fund | null,
  creditFund: Fund | null,
): boolean {
  if (debitAccount !== null || creditAccount !== null) {
    return false;
  }
  return debitFund !== null && creditFund !== null;
};

/**
 * Determines if the fund transaction being created or updated has all required information to be created or updated.
 */
const isFundTransactionComplete = function (
  debitFund: Fund | null,
  creditFund: Fund | null,
): boolean {
  return debitFund !== null && creditFund !== null;
};

export {
  type Transaction,
  type TransactionAccount,
  type CreateTransactionRequest,
  type UpdateTransactionRequest,
  type PostTransactionRequest,
  CreateTransactionModelCreateAccountTransactionModelType as CreateAccountTransactionType,
  CreateTransactionModelCreateFundTransactionModelType as CreateFundTransactionType,
  CreateTransactionModelCreateIncomeTransactionModelType as CreateIncomeTransactionType,
  CreateTransactionModelCreateSpendingTransactionModelType as CreateSpendingTransactionType,
  TransactionModelAccountTransactionModelType as AccountTransactionType,
  TransactionModelFundTransactionModelType as FundTransactionType,
  TransactionModelIncomeTransactionModelType as IncomeTransactionType,
  TransactionModelSpendingTransactionModelType as SpendingTransactionType,
  TransactionAccountTypeModel as TransactionAccountType,
  UpdateTransactionModelUpdateAccountTransactionModelType as UpdateAccountTransactionType,
  UpdateTransactionModelUpdateFundTransactionModelType as UpdateFundTransactionType,
  UpdateTransactionModelUpdateIncomeTransactionModelType as UpdateIncomeTransactionType,
  UpdateTransactionModelUpdateSpendingTransactionModelType as UpdateSpendingTransactionType,
  isIncomeTransaction,
  isIncomeTransactionComplete,
  isSpendingTransaction,
  isSpendingTransactionComplete,
  isAccountTransaction,
  isFundTransaction,
  isFundTransactionComplete,
};
