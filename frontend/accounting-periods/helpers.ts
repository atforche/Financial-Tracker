import dayjs, { type Dayjs } from "dayjs";
import type { AccountingPeriod } from "@/accounting-periods/types";

const accountingPeriodMonths = Array.from(
  { length: 12 },
  (_, index) => index + 1,
);

const monthNameFormatter = new Intl.DateTimeFormat("en-US", {
  month: "long",
  timeZone: "UTC",
});

/**
 * Formats an accounting period month using its long name.
 */
const formatAccountingPeriodMonth = function (month: number): string {
  return monthNameFormatter.format(new Date(Date.UTC(2024, month - 1, 1)));
};

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
  accountingPeriodMonths,
  formatAccountingPeriodMonth,
  getMinimumDate,
  getMaximumDate,
  getDefaultDate,
};
