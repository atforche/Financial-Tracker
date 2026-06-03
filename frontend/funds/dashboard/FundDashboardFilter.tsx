"use client";

import {
  Button,
  Paper,
  Stack,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from "@mui/material";
import {
  normalizeFundNames,
  shouldPersistFundNames,
} from "@/funds/dashboard/fundNameFilter";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/accounting-periods/types";
import FundDashboardAccountingPeriodFilter from "@/funds/dashboard/FundDashboardAccountingPeriodFilter";
import FundDashboardFundNameFilter from "@/funds/dashboard/FundDashboardFundNameFilter";
import type { JSX } from "react";

/**
 * Dashboard filter mode values used in the Funds view URL.
 */
type FundsDashboardFilterMode = "accounting-period" | "date";

/**
 * Props for the FundDashboardFilter component.
 */
interface FundDashboardFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly availableFundNames: readonly string[];
  readonly defaultAccountingPeriodId: string | null;
  readonly defaultStartDate: string;
  readonly defaultEndDate: string;
  readonly disabled?: boolean;
}

/**
 * Renders the dashboard filter card for the Funds view.
 */
const FundDashboardFilter = function ({
  accountingPeriods,
  availableFundNames,
  defaultAccountingPeriodId,
  defaultStartDate,
  defaultEndDate,
  disabled = false,
}: FundDashboardFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const pageParamName = "page";
  const modeParamName = "mode";
  const fundNameParamName = "fundName";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";
  const startDateParamName = "startDate";
  const endDateParamName = "endDate";

  const currentMode: FundsDashboardFilterMode =
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

  const accountingPeriodIndexes = new Map(
    accountingPeriods.map((period, index) => [period.id, index]),
  );

  const updateParams = function (
    updater: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    updater(params);
    params.delete(pageParamName);
    const nextQuery = params.toString();
    router.replace(nextQuery === "" ? pathname : `${pathname}?${nextQuery}`);
  };

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
    nextMode: FundsDashboardFilterMode | null,
  ): void {
    if (nextMode === null) {
      return;
    }
    updateParams((params) => {
      params.set(modeParamName, nextMode);
      if (nextMode === "date") {
        params.delete(startAccountingPeriodIdParamName);
        params.delete(endAccountingPeriodIdParamName);
        params.set(
          startDateParamName,
          params.get(startDateParamName) ?? defaultStartDate,
        );
        params.set(
          endDateParamName,
          params.get(endDateParamName) ?? defaultEndDate,
        );
        return;
      }

      params.delete(startDateParamName);
      params.delete(endDateParamName);
      if (defaultAccountingPeriodId !== null) {
        params.set(
          startAccountingPeriodIdParamName,
          params.get(startAccountingPeriodIdParamName) ??
            defaultAccountingPeriodId,
        );
        params.set(
          endAccountingPeriodIdParamName,
          params.get(endAccountingPeriodIdParamName) ??
            defaultAccountingPeriodId,
        );
      }
    });
  };

  const handleStartAccountingPeriodChange = function (
    nextStartAccountingPeriodId: string,
  ): void {
    const nextStartIndex =
      accountingPeriodIndexes.get(nextStartAccountingPeriodId) ?? 0;
    const currentEndIndex =
      accountingPeriodIndexes.get(currentEndAccountingPeriodId) ??
      nextStartIndex;
    const nextEndAccountingPeriodId =
      nextStartIndex > currentEndIndex
        ? nextStartAccountingPeriodId
        : currentEndAccountingPeriodId;

    updateParams((params) => {
      params.set(startAccountingPeriodIdParamName, nextStartAccountingPeriodId);
      params.set(endAccountingPeriodIdParamName, nextEndAccountingPeriodId);
    });
  };

  const handleEndAccountingPeriodChange = function (
    nextEndAccountingPeriodId: string,
  ): void {
    const nextEndIndex =
      accountingPeriodIndexes.get(nextEndAccountingPeriodId) ?? 0;
    const currentStartIndex =
      accountingPeriodIndexes.get(currentStartAccountingPeriodId) ??
      nextEndIndex;
    const nextStartAccountingPeriodId =
      nextEndIndex < currentStartIndex
        ? nextEndAccountingPeriodId
        : currentStartAccountingPeriodId;

    updateParams((params) => {
      params.set(startAccountingPeriodIdParamName, nextStartAccountingPeriodId);
      params.set(endAccountingPeriodIdParamName, nextEndAccountingPeriodId);
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
    <Paper
      sx={{
        position: "sticky",
        top: 10,
        zIndex: (theme) => theme.zIndex.appBar - 1,
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        bgcolor: "background.paper",
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2}>
        <Stack spacing={0.5}>
          <Typography variant="h5">Fund Dashboard</Typography>
        </Stack>
        <Stack
          direction="row"
          spacing={1.5}
          useFlexGap
          flexWrap="wrap"
          alignItems={{ xs: "stretch", md: "center" }}
        >
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
            <>
              <FundDashboardAccountingPeriodFilter
                accountingPeriods={accountingPeriods}
                label="Start period"
                value={currentStartAccountingPeriodId}
                onChange={handleStartAccountingPeriodChange}
                disabled={disabled}
              />
              <FundDashboardAccountingPeriodFilter
                accountingPeriods={accountingPeriods}
                label="End period"
                value={currentEndAccountingPeriodId}
                onChange={handleEndAccountingPeriodChange}
                disabled={disabled}
              />
            </>
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
          <FundDashboardFundNameFilter
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
        </Stack>
      </Stack>
    </Paper>
  );
};

export default FundDashboardFilter;
