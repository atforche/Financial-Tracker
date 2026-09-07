"use client";

import type {
  AccountingPeriod,
  AccountingPeriodRange,
} from "@/accounting-periods/types";
import {
  type TrendRangeMode,
  getDefaultTrendAccountingPeriodRange,
  getDefaultTrendDateRange,
  setTrendRangeMode,
} from "@/framework/routes/trendRange";
import {
  normalizeFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import { Button } from "@mui/material";
import DateRangeFilter from "@/framework/forms/DateRangeFilter";
import FundTrendsFundNameFilter from "@/funds/trends/FundTrendsFundNameFilter";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import { fundTrendsParamNames } from "@/funds/trends/helpers";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the FundTrendsFilter component.
 */
interface FundTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly availableFundNames: readonly string[];
}

/**
 * Renders the trends filter card for the Funds view.
 */
const FundTrendsFilter = function ({
  accountingPeriods,
  availableFundNames,
}: FundTrendsFilterProps): JSX.Element {
  const defaultRange = getDefaultTrendAccountingPeriodRange(accountingPeriods);
  const defaultStartAccountingPeriodId = defaultRange?.start ?? null;
  const defaultEndAccountingPeriodId = defaultRange?.end ?? null;
  const { start: defaultStartDate, end: defaultEndDate } =
    getDefaultTrendDateRange();
  const searchParams = useSearchParams();

  const pageParamName = fundTrendsParamNames.page;
  const balanceEventPageParamName = fundTrendsParamNames.balanceEventPage;
  const modeParamName = fundTrendsParamNames.mode;
  const fundNameParamName = fundTrendsParamNames.fundName;
  const startAccountingPeriodIdParamName =
    fundTrendsParamNames.startAccountingPeriodId;
  const endAccountingPeriodIdParamName =
    fundTrendsParamNames.endAccountingPeriodId;
  const startDateParamName = fundTrendsParamNames.startDate;
  const endDateParamName = fundTrendsParamNames.endDate;

  const currentMode: TrendRangeMode =
    searchParams.get(modeParamName) === "accounting-period"
      ? "accounting-period"
      : "date";
  const currentFundNames = normalizeFundNames(
    searchParams.getAll(fundNameParamName),
    availableFundNames,
  );
  const currentStartAccountingPeriodId =
    searchParams.get(startAccountingPeriodIdParamName) ??
    defaultStartAccountingPeriodId ??
    "";
  const currentEndAccountingPeriodId =
    searchParams.get(endAccountingPeriodIdParamName) ??
    defaultEndAccountingPeriodId ??
    "";
  const currentStartDate =
    searchParams.get(startDateParamName) ?? defaultStartDate;
  const currentEndDate = searchParams.get(endDateParamName) ?? defaultEndDate;

  const updateParams = useSearchParamUpdater([
    pageParamName,
    balanceEventPageParamName,
  ]);

  const hasActiveView =
    currentMode !== "date" ||
    shouldPersistFundNames(currentFundNames) ||
    currentStartAccountingPeriodId !== (defaultStartAccountingPeriodId ?? "") ||
    currentEndAccountingPeriodId !== (defaultEndAccountingPeriodId ?? "") ||
    currentStartDate !== defaultStartDate ||
    currentEndDate !== defaultEndDate;

  const handleFundNameChange = function (
    nextFundNames: readonly string[],
  ): void {
    updateParams((params) => {
      params.delete(fundNameParamName);
      if (shouldPersistFundNames(nextFundNames)) {
        nextFundNames.forEach((fundName) => {
          params.append(fundNameParamName, fundName);
        });
      }
    });
  };

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
      params.set(startAccountingPeriodIdParamName, range.start);
      params.set(endAccountingPeriodIdParamName, range.end);
    });
  };

  const handleDateRangeChange = function (range: AccountingPeriodRange): void {
    updateParams((params) => {
      params.set(startDateParamName, range.start);
      params.set(endDateParamName, range.end);
    });
  };

  const clearView = function (): void {
    updateParams((params) => {
      params.delete(fundNameParamName);
      setTrendRangeMode(params, "date", {
        defaultStartAccountingPeriodId,
        defaultEndAccountingPeriodId,
        defaultStartDate,
        defaultEndDate,
      });
      params.set(startDateParamName, defaultStartDate);
      params.set(endDateParamName, defaultEndDate);
    });
  };

  return (
    <PageFilterFrame title="Fund Trends">
      <ToggleButtonSelector
        value={currentMode}
        onChange={handleModeChange}
        options={[
          { value: "date", label: "Dates" },
          {
            value: "accounting-period",
            label: "Accounting periods",
            disabled: defaultEndAccountingPeriodId === null,
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
      <FundTrendsFundNameFilter
        availableFundNames={availableFundNames}
        value={currentFundNames}
        onChange={handleFundNameChange}
      />
      <Button
        variant="outlined"
        onClick={clearView}
        disabled={!hasActiveView}
        sx={{ flexShrink: 0 }}
      >
        Reset filters
      </Button>
    </PageFilterFrame>
  );
};

export default FundTrendsFilter;
