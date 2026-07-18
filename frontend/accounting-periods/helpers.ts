import type {
  AccountingPeriod,
  AccountingPeriodRange,
} from "@/accounting-periods/types";
import dayjs, { type Dayjs } from "dayjs";
import type { ComboBoxOption } from "@/framework/forms/ComboBoxEntryField";

/**
 * An array of all accounting period months (1-12).
 */
const accountingPeriodMonths: readonly number[] = Array.from(
  { length: 12 },
  (_, index) => index + 1,
);

/**
 * A formatter for accounting period months that uses their long names.
 */
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
 * An array of all accounting period months formatted with their long names.
 */
const accountingPeriodMonthOptions: ComboBoxOption<number>[] =
  accountingPeriodMonths.map((accountingPeriodMonth) => ({
    label: formatAccountingPeriodMonth(accountingPeriodMonth),
    value: accountingPeriodMonth,
  }));

/**
 * Gets the chronological order value for an accounting period.
 */
const getAccountingPeriodOrder = function (
  accountingPeriod: AccountingPeriod,
): number {
  return accountingPeriod.year * 12 + accountingPeriod.month;
};

/**
 * Compares accounting periods in chronological order.
 */
const compareAccountingPeriods = function (
  first: AccountingPeriod,
  second: AccountingPeriod,
): number {
  return getAccountingPeriodOrder(first) - getAccountingPeriodOrder(second);
};

/**
 * Updates one boundary of an accounting period range while preserving its order.
 */
const updateAccountingPeriodRange = function (
  range: AccountingPeriodRange,
  boundary: keyof AccountingPeriodRange,
  value: string,
  accountingPeriods: readonly AccountingPeriod[],
): AccountingPeriodRange {
  const nextRange = { ...range, [boundary]: value };
  const startPeriod = accountingPeriods.find(
    (accountingPeriod) => accountingPeriod.id === nextRange.start,
  );
  const endPeriod = accountingPeriods.find(
    (accountingPeriod) => accountingPeriod.id === nextRange.end,
  );

  if (
    typeof startPeriod === "undefined" ||
    typeof endPeriod === "undefined" ||
    compareAccountingPeriods(startPeriod, endPeriod) > 0
  ) {
    return { start: value, end: value };
  }

  return nextRange;
};

/**
 * Gets the first date in the provided accounting period.
 */
const getAccountingPeriodDate = function (
  accountingPeriod: AccountingPeriod,
): Dayjs {
  return dayjs()
    .date(1)
    .year(accountingPeriod.year)
    .month(accountingPeriod.month - 1)
    .startOf("month");
};

/**
 * Gets the minimum date associated with the provided accounting period.
 */
const getMinimumDate = function (accountingPeriod: AccountingPeriod): Dayjs {
  return getAccountingPeriodDate(accountingPeriod).subtract(1, "month");
};

/**
 * Gets the maximum date associated with the provided accounting period.
 */
const getMaximumDate = function (accountingPeriod: AccountingPeriod): Dayjs {
  return getAccountingPeriodDate(accountingPeriod)
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
  return getAccountingPeriodDate(accountingPeriod);
};

export {
  accountingPeriodMonths,
  accountingPeriodMonthOptions,
  compareAccountingPeriods,
  formatAccountingPeriodMonth,
  getAccountingPeriodOrder,
  getMinimumDate,
  getMaximumDate,
  getDefaultDate,
  updateAccountingPeriodRange,
};
