"use client";

import { Button, Stack } from "@mui/material";
import type { JSX, ReactNode } from "react";
import {
  type TrendRangeMode,
  getDefaultTrendAccountingPeriodRange,
  getDefaultTrendDateRange,
  getTrendRangeModeOptions,
  maximumTrendRangeYears,
  setTrendRangeMode,
} from "@/framework/routes/trendRange";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import DateRangeFilter from "@/framework/forms/DateRangeFilter";
import ReturnLink from "@/framework/view/ReturnLink";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import WorkspaceHeader from "@/framework/view/WorkspaceHeader";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the TrendFilterHeader component.
 */
interface TrendFilterHeaderProps {
  readonly title: string;
  readonly returnUrl: string | undefined;
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly hasActiveFilters: boolean;
  readonly clearFilters: (params: URLSearchParams) => void;

  /**
   * Actions shown beside the title.
   */
  readonly action?: ReactNode;
  readonly children: ReactNode;
}

/**
 * Displays an account or fund trends page's title with its date or period range controls, the page's own filters, and
 * a reset for all of them.
 */
const TrendFilterHeader = function ({
  title,
  returnUrl,
  accountingPeriods,
  hasActiveFilters,
  clearFilters,
  action = null,
  children,
}: TrendFilterHeaderProps): JSX.Element {
  const defaultRange = getDefaultTrendAccountingPeriodRange(accountingPeriods);
  const { start: defaultStartDate, end: defaultEndDate } =
    getDefaultTrendDateRange();
  const defaults = {
    defaultStartAccountingPeriodId: defaultRange?.start ?? null,
    defaultEndAccountingPeriodId: defaultRange?.end ?? null,
    defaultStartDate,
    defaultEndDate,
  };
  const searchParams = useSearchParams();
  const updateParams = useSearchParamUpdater(["page"]);

  const mode: TrendRangeMode =
    searchParams.get("mode") === "accounting-period"
      ? "accounting-period"
      : "date";
  const startAccountingPeriodId =
    searchParams.get("startAccountingPeriodId") ?? defaultRange?.start ?? "";
  const endAccountingPeriodId =
    searchParams.get("endAccountingPeriodId") ?? defaultRange?.end ?? "";
  const startDate = searchParams.get("startDate") ?? defaultStartDate;
  const endDate = searchParams.get("endDate") ?? defaultEndDate;
  const hasActiveView =
    hasActiveFilters ||
    mode !== "date" ||
    startDate !== defaultStartDate ||
    endDate !== defaultEndDate;

  return (
    <Stack spacing={2}>
      <ReturnLink returnUrl={returnUrl} />
      <WorkspaceHeader title={title} action={action}>
        <ToggleButtonSelector
          value={mode}
          onChange={(nextMode: TrendRangeMode) => {
            updateParams((params) => {
              setTrendRangeMode(params, nextMode, defaults);
            });
          }}
          options={getTrendRangeModeOptions(defaultRange !== null)}
        />
        {mode === "accounting-period" ? (
          <AccountingPeriodRangeFilter
            accountingPeriods={accountingPeriods}
            startValue={startAccountingPeriodId}
            endValue={endAccountingPeriodId}
            onChange={(range) => {
              updateParams((params) => {
                params.set("startAccountingPeriodId", range.start);
                params.set("endAccountingPeriodId", range.end);
              });
            }}
          />
        ) : (
          <DateRangeFilter
            value={{ start: startDate, end: endDate }}
            onChange={(range) => {
              updateParams((params) => {
                params.set("startDate", range.start);
                params.set("endDate", range.end);
              });
            }}
            maxYears={maximumTrendRangeYears}
          />
        )}
        {children}
        <Button
          disabled={!hasActiveView}
          sx={{ flexShrink: 0 }}
          onClick={() => {
            updateParams((params) => {
              clearFilters(params);
              params.set("startDate", defaultStartDate);
              params.set("endDate", defaultEndDate);
              setTrendRangeMode(params, "date", defaults);
            });
          }}
        >
          Reset Filters
        </Button>
      </WorkspaceHeader>
    </Stack>
  );
};

export default TrendFilterHeader;
