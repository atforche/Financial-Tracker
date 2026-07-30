"use client";

import type {
  AccountingPeriod,
  AccountingPeriodRange,
} from "@/accounting-periods/types";
import AccountingPeriodFilter from "@/accounting-periods/AccountingPeriodFilter";
import type { JSX } from "react";
import { updateAccountingPeriodRange } from "@/accounting-periods/helpers";

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
  const handleChange = function (
    boundary: keyof AccountingPeriodRange,
    value: string,
  ): void {
    onChange(
      updateAccountingPeriodRange(
        { start: startValue, end: endValue },
        boundary,
        value,
        accountingPeriods,
      ),
    );
  };

  return (
    <>
      <AccountingPeriodFilter
        accountingPeriods={accountingPeriods}
        label="Start period"
        value={startValue}
        onChange={(value) => {
          handleChange("start", value);
        }}
        disabled={disabled}
      />
      <AccountingPeriodFilter
        accountingPeriods={accountingPeriods}
        label="End period"
        value={endValue}
        onChange={(value) => {
          handleChange("end", value);
        }}
        disabled={disabled}
      />
    </>
  );
};

export default AccountingPeriodRangeFilter;
