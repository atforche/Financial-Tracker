"use client";

import type {
  AccountingPeriod,
  AccountingPeriodRange,
} from "@/accounting-periods/types";
import { Button, Stack, Typography } from "@mui/material";
import {
  type ResolvedTrendRange,
  type TrendRangeMode,
  getDefaultTrendAccountingPeriodRange,
  getDefaultTrendDateRange,
  getTrendRangeModeOptions,
  maximumTrendRangeYears,
  setTrendRangeMode,
} from "@/framework/routes/trendRange";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import DateRangeFilter from "@/framework/forms/DateRangeFilter";
import type { JSX } from "react";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import { overviewParamNames } from "@/overview/helpers";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the OverviewPageHeader component.
 */
interface OverviewPageHeaderProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly range: ResolvedTrendRange;
}

/**
 * Displays the Overview heading and shared account/fund range controls.
 */
const OverviewPageHeader = function ({
  accountingPeriods,
  range,
}: OverviewPageHeaderProps): JSX.Element {
  const defaultAccountingPeriodRange =
    getDefaultTrendAccountingPeriodRange(accountingPeriods);
  const defaultStartAccountingPeriodId =
    defaultAccountingPeriodRange?.start ?? null;
  const defaultEndAccountingPeriodId =
    defaultAccountingPeriodRange?.end ?? null;
  const { start: defaultStartDate, end: defaultEndDate } =
    getDefaultTrendDateRange();
  const updateParams = useSearchParamUpdater([]);
  const hasCustomRange =
    range.mode !== "date" ||
    range.startDate !== defaultStartDate ||
    range.endDate !== defaultEndDate;

  const handleModeChange = function (nextMode: TrendRangeMode): void {
    updateParams((params) => {
      setTrendRangeMode(params, nextMode, {
        defaultStartAccountingPeriodId,
        defaultEndAccountingPeriodId,
        defaultStartDate,
        defaultEndDate,
      });
    });
  };
  const handleAccountingPeriodRangeChange = function (
    nextRange: AccountingPeriodRange,
  ): void {
    updateParams((params) => {
      params.set(overviewParamNames.startAccountingPeriodId, nextRange.start);
      params.set(overviewParamNames.endAccountingPeriodId, nextRange.end);
    });
  };
  const handleDateRangeChange = function (
    nextRange: AccountingPeriodRange,
  ): void {
    updateParams((params) => {
      params.set(overviewParamNames.startDate, nextRange.start);
      params.set(overviewParamNames.endDate, nextRange.end);
    });
  };
  const resetRange = function (): void {
    updateParams((params) => {
      params.set(overviewParamNames.mode, "date");
      params.set(overviewParamNames.startDate, defaultStartDate);
      params.set(overviewParamNames.endDate, defaultEndDate);
      params.delete(overviewParamNames.startAccountingPeriodId);
      params.delete(overviewParamNames.endAccountingPeriodId);
    });
  };

  return (
    <Stack spacing={2} data-page-filters>
      <Typography variant="h4">Overview</Typography>
      <Stack
        direction="row"
        spacing={1.5}
        useFlexGap
        flexWrap="wrap"
        alignItems={{ xs: "stretch", md: "center" }}
      >
        <ToggleButtonSelector
          value={range.mode}
          onChange={handleModeChange}
          options={getTrendRangeModeOptions(
            defaultAccountingPeriodRange !== null,
          )}
        />
        {range.mode === "accounting-period" ? (
          <AccountingPeriodRangeFilter
            accountingPeriods={accountingPeriods}
            startValue={range.startAccountingPeriodId}
            endValue={range.endAccountingPeriodId}
            onChange={handleAccountingPeriodRangeChange}
          />
        ) : (
          <DateRangeFilter
            value={{ start: range.startDate, end: range.endDate }}
            onChange={handleDateRangeChange}
            maxYears={maximumTrendRangeYears}
          />
        )}
        <Button
          variant="outlined"
          onClick={resetRange}
          disabled={!hasCustomRange}
          sx={{ flexShrink: 0 }}
        >
          Reset range
        </Button>
      </Stack>
    </Stack>
  );
};

export default OverviewPageHeader;
