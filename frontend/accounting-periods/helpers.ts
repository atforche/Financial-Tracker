import dayjs, { type Dayjs } from "dayjs";
import type { AccountingPeriod } from "@/accounting-periods/types";

/**
 * Gets the minimum date associated with the provided accounting period.
 */
const getMinimumDate = function (accountingPeriod: AccountingPeriod): Dayjs {
  return dayjs(accountingPeriod.name, "MMMM YYYY").subtract(1, "month");
};

/**
 * Gets the maximum date associated with the provided accounting period.
 */
const getMaximumDate = function (accountingPeriod: AccountingPeriod): Dayjs {
  return dayjs(accountingPeriod.name, "MMMM YYYY")
    .add(2, "month")
    .subtract(1, "day");
};

/**
 * Gets the default date associated with the provided accounting period.
 */
const getDefaultDate = function (
  accountingPeriod: AccountingPeriod | null,
): Dayjs | null {
  if (accountingPeriod === null) {
    return null;
  }
  return dayjs(accountingPeriod.name, "MMMM YYYY");
};

export {
  getMinimumDate,
  getMaximumDate,
  getDefaultDate,
}