import type {
  Fund,
  FundBalanceEvent,
  FundBalanceEventDraft,
  FundWithBalance,
} from "@/funds/types";
import { isNullOrUndefined } from "@/framework/nullHelpers";

/**
 * Gets the balance change captured in the provided transaction fund draft.
 */
const getTransactionFundDraftBalanceChange = function (
  fund: FundBalanceEventDraft | null,
): number {
  return (fund?.newFundBalance ?? 0) - (fund?.previousFundBalance ?? 0);
};

/**
 * Applies a balance change to an existing transaction fund draft.
 */
const setTransactionFundDraftBalanceChange = function (
  fund: FundBalanceEventDraft | null,
  balanceChange: number | null | undefined,
): FundBalanceEventDraft | null {
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
  fund: FundWithBalance,
  balanceChange = 0,
): FundBalanceEventDraft {
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
  fund: FundBalanceEvent | null | undefined,
): FundBalanceEventDraft | null {
  if (isNullOrUndefined(fund)) {
    return null;
  }
  return {
    fundId: fund.fund.id,
    fundName: fund.fund.name,
    previousFundBalance: fund.previousBalance.postedBalance,
    newFundBalance: fund.newBalance.postedBalance,
  };
};

/**
 * Resolves the next selected transaction fund draft from a fund picker value.
 */
const getSelectedTransactionFundDraft = function (
  funds: FundWithBalance[],
  nextValue: Fund | null,
  currentFund: FundBalanceEventDraft | null,
  balanceChange?: number | null,
): FundBalanceEventDraft | null {
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
