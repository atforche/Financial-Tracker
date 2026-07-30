import {
  compareCurrencyAmounts,
  getCurrencyDifference,
  getCurrencyTotal,
  getMaximumCurrencyAmount,
} from "@/framework/currencyHelpers";
import dayjs, { type Dayjs } from "dayjs";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import { buildUrl } from "@/framework/routes/helpers";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";
import propertyName from "@/framework/data/propertyName";

/**
 * Gets the default accounting period from a list of accounting periods.
 */
const getDefaultAccountingPeriod = function (
  accountingPeriods: AccountingPeriod[],
): AccountingPeriod | null {
  return accountingPeriods.length > 0
    ? (accountingPeriods[accountingPeriods.length - 1] ?? null)
    : null;
};

/**
 * Gets the default date from an accounting period.
 */
const getDefaultDate = function (
  accountingPeriod: AccountingPeriod | null,
): Dayjs | null {
  return accountingPeriod !== null
    ? dayjs(`${accountingPeriod.year}-${accountingPeriod.month}-01`)
    : null;
};

/**
 * Adds the provided transaction ID to the redirect URL as a query parameter.
 */
const redirectWithSelectedTransaction = function (
  redirectUrl: string,
  transactionId: string,
): string {
  const [pathname, search = ""] = redirectUrl.split("?");
  const params = new URLSearchParams(search);
  params.set(
    propertyName<TransactionWorkspaceSearchParams>("selectedTransactionId"),
    transactionId,
  );
  return buildUrl(`${pathname}/${transactionId}`, params);
};

/**
 * Interface representing a destination draft that has an amount property.
 */
interface AmountedDestinationDraft {
  readonly amount: number | null;
}

/**
 * Validates the shared transaction details section.
 */
const validateDetails = function (
  accountingPeriod: AccountingPeriod | null,
  date: Dayjs | null,
  defaultDate: Dayjs | null,
  description: string,
): boolean {
  return (
    accountingPeriod !== null &&
    (date !== null || defaultDate !== null) &&
    description.trim() !== ""
  );
};

/**
 * Validates the shared transaction summary section.
 */
const validateSummary = function (
  sourceAmount: number | null | undefined,
  destinationAmount: number,
  destinationCount: number,
): boolean {
  return (
    isNotNullOrUndefined(sourceAmount) &&
    destinationCount > 0 &&
    getCurrencyDifference(sourceAmount, destinationAmount) === 0
  );
};

/**
 * Updates the sole destination amount when it is still effectively mirroring
 * the source amount.
 */
const syncDestinationAmountsToSource = function <
  DestinationDraft extends AmountedDestinationDraft,
>(
  destinations: DestinationDraft[],
  previousSourceAmount: number | null,
  nextSourceAmount: number | null,
  setAmount: (
    destination: DestinationDraft,
    amount: number | null,
  ) => DestinationDraft,
): DestinationDraft[] {
  if (destinations.length !== 1) {
    return destinations;
  }
  const [destination] = destinations;
  if (
    typeof destination === "undefined" ||
    (compareCurrencyAmounts(destination.amount ?? 0, 0) !== 0 &&
      previousSourceAmount !== null &&
      compareCurrencyAmounts(destination.amount ?? 0, previousSourceAmount) !==
        0)
  ) {
    return destinations;
  }
  return [setAmount(destination, nextSourceAmount)];
};

/**
 * Appends a destination and seeds its amount with the remaining unallocated
 * source amount when a source amount is already available.
 */
const appendDestinationWithAutofilledAmount = function <
  DestinationDraft extends AmountedDestinationDraft,
>(
  destinations: DestinationDraft[],
  newDestination: DestinationDraft,
  sourceAmount: number | null,
  setAmount: (
    destination: DestinationDraft,
    amount: number | null,
  ) => DestinationDraft,
): DestinationDraft[] {
  if (sourceAmount === null) {
    return [...destinations, newDestination];
  }
  const allocatedAmount = getCurrencyTotal(
    destinations.map((destination) => destination.amount),
  );
  const remainingAmount = getMaximumCurrencyAmount(
    getCurrencyDifference(sourceAmount, allocatedAmount),
    0,
  );
  return [...destinations, setAmount(newDestination, remainingAmount)];
};

export {
  getDefaultAccountingPeriod,
  getDefaultDate,
  redirectWithSelectedTransaction,
  syncDestinationAmountsToSource,
  appendDestinationWithAutofilledAmount,
  validateDetails,
  validateSummary,
};
