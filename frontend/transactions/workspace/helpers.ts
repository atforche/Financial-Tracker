import dayjs, { type Dayjs } from "dayjs";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import { buildUrl } from "@/framework/routes/helpers";
import { getCurrencyDifference } from "@/framework/currencyHelpers";
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

export {
  getDefaultAccountingPeriod,
  getDefaultDate,
  redirectWithSelectedTransaction,
  validateDetails,
  validateSummary,
};
