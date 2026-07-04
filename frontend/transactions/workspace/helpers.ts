import dayjs, { type Dayjs } from "dayjs";
import type { AccountingPeriod } from "@/accounting-periods/types";

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
  params.set("selectedTransactionId", transactionId);
  const query = params.toString();
  return query === ""
    ? `${pathname}/${transactionId}`
    : `${pathname}/${transactionId}?${query}`;
};

/**
 * Interface representing a destination draft that has an amount property.
 */
interface AmountedDestinationDraft {
  readonly amount: number | null;
}

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
    (destination.amount !== 0 && destination.amount !== previousSourceAmount)
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
  const allocatedAmount = destinations.reduce(
    (total, destination) => total + (destination.amount ?? 0),
    0,
  );
  const remainingAmount = Math.max(sourceAmount - allocatedAmount, 0);
  return [...destinations, setAmount(newDestination, remainingAmount)];
};

export {
  getDefaultAccountingPeriod,
  getDefaultDate,
  redirectWithSelectedTransaction,
  syncDestinationAmountsToSource,
  appendDestinationWithAutofilledAmount,
};
