import {
  CreateTransactionModelCreateSpendingTransactionModelType,
  UpdateTransactionModelUpdateSpendingTransactionModelType,
} from "@/framework/data/api";
import { type FundAmount, hasIncompleteFundAssignments } from "@/funds/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { type Account, isTrackedAccountType } from "@/accounts/types";
import type { Dayjs } from "dayjs";

/**
 * Interface representing a potentially unfinished spending transaction source.
 */
interface SpendingSourceDraft {
  readonly account: Account | null;
  readonly amount: number | null;
}

/**
 * Interface representing a potentially unfinished spending transaction destination.
 */
interface SpendingDestinationDraft {
  readonly account: Account | null;
  readonly location: string | null;
  readonly amount: number | null;
  readonly fundAssignments: FundAmount[];
}

/**
 * Creates an empty source draft.
 */
const createEmptySource = function (): SpendingSourceDraft {
  return {
    account: null,
    amount: null,
  };
}

/**
 * Creates an empty destination draft.
 */
const createEmptyDestination = function (): SpendingDestinationDraft {
  return {
    account: null,
    location: null,
    amount: null,
    fundAssignments: [],
  };
}

/**
 * Validates the source of a spending transaction.
 */
const validateSource = function (source: SpendingSourceDraft): boolean {
  if (source.account === null) {
    return false;
  }
  if (!isTrackedAccountType(source.account.type)) {
    return false;
  }
  return source.amount !== null && source.amount > 0;
}

/**
 * Validates the destination of a spending transaction.
 */
const validateDestination = function (
  destination: SpendingDestinationDraft,
  sourceAccount: Account | null,
): boolean {
  const normalizedLocation = destination.location?.trim() ?? "";
  const hasAccount = destination.account !== null;
  const hasLocation = normalizedLocation !== "";
  const destinationIsTracked =
    destination.account !== null &&
    isTrackedAccountType(destination.account.type);

  if (destination.amount === null || destination.amount <= 0) {
    return false;
  }
  if ((hasAccount && hasLocation) || (!hasAccount && !hasLocation)) {
    return false;
  }
  if (destination.account?.id === sourceAccount?.id) {
    return false;
  }
  if (hasAccount && isTrackedAccountType(destination.account.type))
  {
    return false;
  }
  if (!hasAccount && destination.fundAssignments.length > 0) {
    return false;
  }
  return true;
};

/**
 * Validates the entire spending transaction request.
 */
const validateRequest = function (
  accountingPeriod: AccountingPeriod | null,
  date: Dayjs | null,
  defaultDate: Dayjs | null,
  description: string,
  source: SpendingSourceDraft,
  destinations: SpendingDestinationDraft[],
): boolean {
  return accountingPeriod !== null &&
    (date !== null || defaultDate !== null) &&
    description !== "" &&
    amount !== null &&
    amount > 0 &&
    sourceAccount !== null &&
    destinations.length > 0 &&
    destinationTotal === amount &&
    areDestinationsComplete
}

export {
  CreateTransactionModelCreateSpendingTransactionModelType as CreateSpendingTransactionType,
  UpdateTransactionModelUpdateSpendingTransactionModelType as UpdateSpendingTransactionType,
  isSpendingTransactionComplete,
};
