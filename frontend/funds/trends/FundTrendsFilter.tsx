"use client";

import {
  Button,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
} from "@mui/material";
import {
  normalizeFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import FundTrendsFundNameFilter from "@/funds/trends/FundTrendsFundNameFilter";
import type { FundTrendsSearchParams } from "@/funds/trends/FundTrends";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import nameof from "@/framework/data/nameof";
import { setTrendRangeMode } from "@/framework/routes/trendRange";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Trends filter mode values used in the Funds view URL.
 */
type FundsTrendsFilterMode = "accounting-period" | "date";

/**
 * Props for the FundTrendsFilter component.
 */
interface FundTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly availableFundNames: readonly string[];
  readonly defaultAccountingPeriodId: string | null;
  readonly defaultStartDate: string;
  readonly defaultEndDate: string;
  readonly disabled?: boolean;
}

/**
 * Renders the trends filter card for the Funds view.
 */
const FundTrendsFilter = function ({
  accountingPeriods,
  availableFundNames,
  defaultAccountingPeriodId,
  defaultStartDate,
  defaultEndDate,
  disabled = false,
}: FundTrendsFilterProps): JSX.Element {
  const searchParams = useSearchParams();

  const pageParamName = nameof<FundTrendsSearchParams>("page");
  const modeParamName = nameof<FundTrendsSearchParams>("mode");
  const fundNameParamName = nameof<FundTrendsSearchParams>("fundName");
  const startAccountingPeriodIdParamName = nameof<FundTrendsSearchParams>(
    "startAccountingPeriodId",
  );
  const endAccountingPeriodIdParamName = nameof<FundTrendsSearchParams>(
    "endAccountingPeriodId",
  );
  const startDateParamName = nameof<FundTrendsSearchParams>("startDate");
  const endDateParamName = nameof<FundTrendsSearchParams>("endDate");

  const currentMode: FundsTrendsFilterMode =
    searchParams.get(modeParamName) === "accounting-period"
      ? "accounting-period"
      : "date";
  const currentFundNames = normalizeFundNames(
    searchParams.getAll(fundNameParamName),
    availableFundNames,
  );
  const currentStartAccountingPeriodId =
    searchParams.get(startAccountingPeriodIdParamName) ??
    defaultAccountingPeriodId ??
    "";
  const currentEndAccountingPeriodId =
    searchParams.get(endAccountingPeriodIdParamName) ??
    defaultAccountingPeriodId ??
    "";
  const currentStartDate =
    searchParams.get(startDateParamName) ?? defaultStartDate;
  const currentEndDate = searchParams.get(endDateParamName) ?? defaultEndDate;

  const updateParams = useSearchParamUpdater([pageParamName]);

  const hasActiveView =
    currentMode !== "date" ||
    shouldPersistFundNames(currentFundNames) ||
    currentStartAccountingPeriodId !== (defaultAccountingPeriodId ?? "") ||
    currentEndAccountingPeriodId !== (defaultAccountingPeriodId ?? "") ||
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

  const handleModeChange = function (
    _: React.MouseEvent<HTMLElement>,
    nextMode: FundsTrendsFilterMode | null,
  ): void {
    if (nextMode === null) {
      return;
    }
    updateParams((params) => {
      setTrendRangeMode(params, nextMode, {
        defaultAccountingPeriodId,
        defaultStartDate,
        defaultEndDate,
      });
    });
  };

  const handleAccountingPeriodRangeChange = function (range: {
    readonly start: string;
    readonly end: string;
  }): void {
    updateParams((params) => {
      params.set(startAccountingPeriodIdParamName, range.start);
      params.set(endAccountingPeriodIdParamName, range.end);
    });
  };

  const handleStartDateChange = function (
    event: React.ChangeEvent<HTMLInputElement>,
  ): void {
    const nextStartDate = event.target.value;
    const nextEndDate =
      nextStartDate > currentEndDate ? nextStartDate : currentEndDate;

    updateParams((params) => {
      params.set(startDateParamName, nextStartDate);
      params.set(endDateParamName, nextEndDate);
    });
  };

  const handleEndDateChange = function (
    event: React.ChangeEvent<HTMLInputElement>,
  ): void {
    const nextEndDate = event.target.value;
    const nextStartDate =
      nextEndDate < currentStartDate ? nextEndDate : currentStartDate;

    updateParams((params) => {
      params.set(startDateParamName, nextStartDate);
      params.set(endDateParamName, nextEndDate);
    });
  };

  const clearView = function (): void {
    updateParams((params) => {
      params.delete(fundNameParamName);
      params.set(modeParamName, "date");
      params.delete(startDateParamName);
      params.delete(endDateParamName);
      if (defaultAccountingPeriodId !== null) {
        params.set(startAccountingPeriodIdParamName, defaultAccountingPeriodId);
        params.set(endAccountingPeriodIdParamName, defaultAccountingPeriodId);
      } else {
        params.delete(startAccountingPeriodIdParamName);
        params.delete(endAccountingPeriodIdParamName);
      }
    });
  };

  const sharedFieldSx = {
    minWidth: { xs: "100%", sm: 180 },
  };

  return (
    <PageFilterFrame title="Fund Trends">
      <ToggleButtonGroup
        exclusive
        value={currentMode}
        size="small"
        disabled={disabled}
        onChange={handleModeChange}
        sx={{ flexShrink: 0 }}
      >
        <ToggleButton value="date">Dates</ToggleButton>
        <ToggleButton
          value="accounting-period"
          disabled={defaultAccountingPeriodId === null}
        >
          Accounting periods
        </ToggleButton>
      </ToggleButtonGroup>
      {currentMode === "accounting-period" ? (
        <AccountingPeriodRangeFilter
          accountingPeriods={accountingPeriods}
          startValue={currentStartAccountingPeriodId}
          endValue={currentEndAccountingPeriodId}
          onChange={handleAccountingPeriodRangeChange}
          disabled={disabled}
        />
      ) : (
        <>
          <TextField
            size="small"
            label="Start date"
            type="date"
            value={currentStartDate}
            onChange={handleStartDateChange}
            disabled={disabled}
            sx={sharedFieldSx}
            slotProps={{
              inputLabel: {
                shrink: true,
              },
            }}
          />
          <TextField
            size="small"
            label="End date"
            type="date"
            value={currentEndDate}
            onChange={handleEndDateChange}
            disabled={disabled}
            sx={sharedFieldSx}
            slotProps={{
              inputLabel: {
                shrink: true,
              },
            }}
          />
        </>
      )}
      <FundTrendsFundNameFilter
        availableFundNames={availableFundNames}
        value={currentFundNames}
        onChange={handleFundNameChange}
        disabled={disabled}
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
