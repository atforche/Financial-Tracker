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

export {
  getDefaultAccountingPeriod,
  getDefaultDate,
  redirectWithSelectedTransaction,
};
