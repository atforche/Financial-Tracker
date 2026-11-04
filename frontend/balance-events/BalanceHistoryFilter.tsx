"use client";

import {
  type TrendRangeMode,
  getDefaultTrendDateRange,
  getTrendRangeModeOptions,
  maximumTrendRangeYears,
  setTrendRangeMode,
} from "@/framework/routes/trendRange";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import type { BalanceHistoryRange } from "@/balance-events/resolveBalanceHistory";
import DateRangeFilter from "@/framework/forms/DateRangeFilter";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the BalanceHistoryFilter component.
 */
interface BalanceHistoryFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly range: BalanceHistoryRange;
}

/**
 * Chooses the dates or accounting periods shown by an account or fund's history.
 */
const BalanceHistoryFilter = function ({
  accountingPeriods,
  range,
}: BalanceHistoryFilterProps): JSX.Element {
  const updateParams = useSearchParamUpdater(["balanceEventPage"]);
  const defaultDates = getDefaultTrendDateRange();
  const newest = accountingPeriods[0]?.id ?? null;
  const oldest = accountingPeriods.at(-1)?.id ?? null;
  const setRange = function (
    start: [string, string],
    end: [string, string],
  ): void {
    updateParams((params) => {
      params.set(...start);
      params.set(...end);
    });
  };

  return (
    <Stack
      direction="row"
      useFlexGap
      spacing={2}
      sx={{ alignItems: "center", flexWrap: "wrap" }}
    >
      <ToggleButtonSelector<TrendRangeMode>
        value={range.mode}
        onChange={(mode) => {
          updateParams((params) => {
            setTrendRangeMode(params, mode, {
              defaultStartAccountingPeriodId: oldest,
              defaultEndAccountingPeriodId: newest,
              defaultStartDate: defaultDates.start,
              defaultEndDate: defaultDates.end,
            });
          });
        }}
        options={getTrendRangeModeOptions(newest !== null)}
      />
      {range.mode === "date" ? (
        <DateRangeFilter
          value={{ start: range.startDate, end: range.endDate }}
          onChange={({ start, end }) => {
            setRange(["startDate", start], ["endDate", end]);
          }}
          maxYears={maximumTrendRangeYears}
        />
      ) : (
        <AccountingPeriodRangeFilter
          accountingPeriods={accountingPeriods}
          startValue={range.startAccountingPeriodId}
          endValue={range.endAccountingPeriodId}
          onChange={({ start, end }) => {
            setRange(
              ["startAccountingPeriodId", start],
              ["endAccountingPeriodId", end],
            );
          }}
        />
      )}
    </Stack>
  );
};

export default BalanceHistoryFilter;
