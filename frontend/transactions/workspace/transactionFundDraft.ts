import type { Fund, FundIdentifier } from "@/funds/types";
import type {
  TransactionFund,
  TransactionFundDraft,
} from "@/transactions/transaction";

/**
 * Gets the balance change captured in the provided transaction fund draft.
 */
const getTransactionFundDraftBalanceChange = function (
  fund: TransactionFundDraft | null,
): number {
  return (fund?.newFundBalance ?? 0) - (fund?.previousFundBalance ?? 0);
};

/**
 * Applies a balance change to an existing transaction fund draft.
 */
const setTransactionFundDraftBalanceChange = function (
  fund: TransactionFundDraft | null,
  balanceChange: number | null | undefined,
): TransactionFundDraft | null {
  if (fund === null) {
    return null;
  }
  return {
    ...fund,
    newFundBalance: (fund.previousFundBalance ?? 0) + (balanceChange ?? 0),
  };
};

/**
 * Creates a transaction fund draft from a fund and balance delta.
 */
const createTransactionFundDraftFromFund = function (
  fund: Fund,
  balanceChange = 0,
): TransactionFundDraft {
  const previousPostedBalance = fund.currentBalance.postedBalance;
  return {
    fundId: fund.id,
    fundName: fund.name,
    previousFundBalance: previousPostedBalance,
    newFundBalance: previousPostedBalance + balanceChange,
  };
};

/**
 * Hydrates a transaction fund draft from a persisted transaction fund.
 */
const getTransactionFundDraftFromTransactionFund = function (
  fund: TransactionFund | null | undefined,
): TransactionFundDraft | null {
  if (typeof fund === "undefined" || fund === null) {
    return null;
  }
  return {
    fundId: fund.fundId,
    fundName: fund.fundName,
    previousFundBalance: fund.previousFundBalance.postedBalance,
    newFundBalance: fund.newFundBalance.postedBalance,
  };
};

/**
 * Resolves the next selected transaction fund draft from a fund picker value.
 */
const getSelectedTransactionFundDraft = function (
  funds: Fund[],
  nextValue: FundIdentifier | null,
  currentFund: TransactionFundDraft | null,
  balanceChange?: number | null,
): TransactionFundDraft | null {
  if (nextValue === null) {
    return null;
  }
  const selectedFund =
    funds.find((candidate) => candidate.id === nextValue.id) ?? null;
  if (selectedFund === null) {
    return null;
  }
  return createTransactionFundDraftFromFund(
    selectedFund,
    balanceChange ?? getTransactionFundDraftBalanceChange(currentFund),
  );
};

export {
  createTransactionFundDraftFromFund,
  getSelectedTransactionFundDraft,
  getTransactionFundDraftBalanceChange,
  getTransactionFundDraftFromTransactionFund,
  setTransactionFundDraftBalanceChange,
};
