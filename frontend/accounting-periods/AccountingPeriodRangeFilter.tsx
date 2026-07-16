"use client";

import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodFilter from "@/accounting-periods/AccountingPeriodFilter";
import type { JSX } from "react";

/**
 * Accounting period range selected by the filter.
 */
interface AccountingPeriodRange {
  readonly start: string;
  readonly end: string;
}

/**
 * Props for the AccountingPeriodRangeFilter component.
 */
interface AccountingPeriodRangeFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly startValue: string;
  readonly endValue: string;
  readonly onChange: (range: AccountingPeriodRange) => void;
  readonly disabled?: boolean;
}

/**
 * Renders an ordered pair of accounting period selectors.
 */
const AccountingPeriodRangeFilter = function ({
  accountingPeriods,
  startValue,
  endValue,
  onChange,
  disabled = false,
}: AccountingPeriodRangeFilterProps): JSX.Element {
  const chronologicalPeriods = [...accountingPeriods].sort((a, b) => {
    if (a.year !== b.year) {
      return a.year - b.year;
    }
    return a.month - b.month;
  });
  const periodIndexes = new Map(
    chronologicalPeriods.map((period, index) => [period.id, index]),
  );

  const handleStartChange = function (nextStart: string): void {
    const nextStartIndex = periodIndexes.get(nextStart);
    const currentEndIndex = periodIndexes.get(endValue);
    const nextEnd =
      typeof nextStartIndex === "number" &&
      typeof currentEndIndex === "number" &&
      nextStartIndex <= currentEndIndex
        ? endValue
        : nextStart;

    onChange({ start: nextStart, end: nextEnd });
  };

  const handleEndChange = function (nextEnd: string): void {
    const nextEndIndex = periodIndexes.get(nextEnd);
    const currentStartIndex = periodIndexes.get(startValue);
    const nextStart =
      typeof nextEndIndex === "number" &&
      typeof currentStartIndex === "number" &&
      nextEndIndex >= currentStartIndex
        ? startValue
        : nextEnd;

    onChange({ start: nextStart, end: nextEnd });
  };

  return (
    <>
      <AccountingPeriodFilter
        accountingPeriods={accountingPeriods}
        label="Start period"
        value={startValue}
        onChange={handleStartChange}
        disabled={disabled}
      />
      <AccountingPeriodFilter
        accountingPeriods={accountingPeriods}
        label="End period"
        value={endValue}
        onChange={handleEndChange}
        disabled={disabled}
      />
    </>
  );
};

export default AccountingPeriodRangeFilter;
