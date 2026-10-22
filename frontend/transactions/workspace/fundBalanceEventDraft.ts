import type {
  Fund,
  FundBalanceEvent,
  FundBalanceEventDraft,
  FundWithBalance,
} from "@/funds/types";
import { isNullOrUndefined } from "@/framework/nullHelpers";

/**
 * Creates a transaction fund draft from a fund.
 */
const createTransactionFundDraftFromFund = function (
  fund: FundWithBalance,
): FundBalanceEventDraft {
  return {
    fundId: fund.id,
    fundName: fund.name,
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
  };
};

/**
 * Resolves the next selected transaction fund draft from a fund picker value.
 */
const getSelectedTransactionFundDraft = function (
  funds: FundWithBalance[],
  nextValue: Fund | null,
): FundBalanceEventDraft | null {
  if (nextValue === null) {
    return null;
  }
  const selectedFund =
    funds.find((candidate) => candidate.id === nextValue.id) ?? null;
  if (selectedFund === null) {
    return null;
  }
  return createTransactionFundDraftFromFund(selectedFund);
};

export {
  createTransactionFundDraftFromFund,
  getSelectedTransactionFundDraft,
  getTransactionFundDraftFromTransactionFund,
};
