"use client";

import type {
  AccountingPeriod,
  AccountingPeriodRange,
} from "@/accounting-periods/types";
import { Button, Stack, Typography } from "@mui/material";
import {
  type TrendRangeMode,
  getDefaultTrendAccountingPeriodRange,
  getDefaultTrendDateRange,
  setTrendRangeMode,
} from "@/framework/routes/trendRange";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import DateRangeFilter from "@/framework/forms/DateRangeFilter";
import type { JSX } from "react";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import { overviewParamNames } from "@/overview/helpers";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

interface OverviewPageHeaderProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
}

/** Displays the Overview heading and shared account/fund range controls. */
const OverviewPageHeader = function ({
  accountingPeriods,
}: OverviewPageHeaderProps): JSX.Element {
  const defaultAccountingPeriodRange =
    getDefaultTrendAccountingPeriodRange(accountingPeriods);
  const defaultStartAccountingPeriodId =
    defaultAccountingPeriodRange?.start ?? null;
  const defaultEndAccountingPeriodId =
    defaultAccountingPeriodRange?.end ?? null;
  const { start: defaultStartDate, end: defaultEndDate } =
    getDefaultTrendDateRange();
  const searchParams = useSearchParams();
  const currentMode: TrendRangeMode =
    searchParams.get(overviewParamNames.mode) === "accounting-period" &&
    defaultAccountingPeriodRange !== null
      ? "accounting-period"
      : "date";
  const currentStartAccountingPeriodId =
    searchParams.get(overviewParamNames.startAccountingPeriodId) ??
    defaultStartAccountingPeriodId ??
    "";
  const currentEndAccountingPeriodId =
    searchParams.get(overviewParamNames.endAccountingPeriodId) ??
    defaultEndAccountingPeriodId ??
    "";
  const currentStartDate =
    searchParams.get(overviewParamNames.startDate) ?? defaultStartDate;
  const currentEndDate =
    searchParams.get(overviewParamNames.endDate) ?? defaultEndDate;
  const updateParams = useSearchParamUpdater([]);
  const hasCustomRange =
    currentMode !== "date" ||
    currentStartDate !== defaultStartDate ||
    currentEndDate !== defaultEndDate;

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
    range: AccountingPeriodRange,
  ): void {
    updateParams((params) => {
      params.set(overviewParamNames.startAccountingPeriodId, range.start);
      params.set(overviewParamNames.endAccountingPeriodId, range.end);
    });
  };
  const handleDateRangeChange = function (range: AccountingPeriodRange): void {
    updateParams((params) => {
      params.set(overviewParamNames.startDate, range.start);
      params.set(overviewParamNames.endDate, range.end);
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
    <Stack spacing={2}>
      <Typography variant="h4">Overview</Typography>
      <Stack
        direction="row"
        spacing={1.5}
        useFlexGap
        flexWrap="wrap"
        alignItems={{ xs: "stretch", md: "center" }}
      >
        <ToggleButtonSelector
          value={currentMode}
          onChange={handleModeChange}
          options={[
            { value: "date", label: "Dates" },
            {
              value: "accounting-period",
              label: "Accounting periods",
              disabled: defaultAccountingPeriodRange === null,
            },
          ]}
        />
        {currentMode === "accounting-period" ? (
          <AccountingPeriodRangeFilter
            accountingPeriods={accountingPeriods}
            startValue={currentStartAccountingPeriodId}
            endValue={currentEndAccountingPeriodId}
            onChange={handleAccountingPeriodRangeChange}
          />
        ) : (
          <DateRangeFilter
            value={{ start: currentStartDate, end: currentEndDate }}
            onChange={handleDateRangeChange}
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
