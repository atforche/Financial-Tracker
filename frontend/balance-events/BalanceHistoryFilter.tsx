"use client";

import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import DateRangeFilter from "@/framework/forms/DateRangeFilter";
import type { JSX } from "react";
import { maximumTrendRangeYears } from "@/framework/routes/trendRange";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the BalanceHistoryFilter component. Accounting periods are provided for a period range only.
 */
interface BalanceHistoryFilterProps {
  readonly start: string;
  readonly end: string;
  readonly accountingPeriods?: readonly AccountingPeriod[];
}

/**
 * Chooses the dates or accounting periods shown by one view of an account or fund page.
 */
const BalanceHistoryFilter = function ({
  start,
  end,
  accountingPeriods,
}: BalanceHistoryFilterProps): JSX.Element {
  const updateParams = useSearchParamUpdater([]);
  const setRange = function (
    startName: string,
    endName: string,
    range: { readonly start: string; readonly end: string },
  ): void {
    updateParams((params) => {
      params.set(startName, range.start);
      params.set(endName, range.end);
    });
  };

  return accountingPeriods === undefined ? (
    <DateRangeFilter
      value={{ start, end }}
      onChange={(range) => {
        setRange("startDate", "endDate", range);
      }}
      maxYears={maximumTrendRangeYears}
    />
  ) : (
    <AccountingPeriodRangeFilter
      accountingPeriods={accountingPeriods}
      startValue={start}
      endValue={end}
      onChange={(range) => {
        setRange("startAccountingPeriodId", "endAccountingPeriodId", range);
      }}
    />
  );
};

export default BalanceHistoryFilter;
