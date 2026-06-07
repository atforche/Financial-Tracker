"use client";

import {
  Autocomplete,
  Button,
  Checkbox,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import {
  normalizeFundNames,
  shouldPersistFundNames,
} from "@/funds/dashboard/fundNameFilter";
import {
  normalizeGoalTypes,
  shouldPersistGoalTypes,
} from "@/goals/dashboard/goalTypeFilter";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AccountDashboardAccountingPeriodFilter from "@/accounts/dashboard/AccountDashboardAccountingPeriodFilter";
import type { AccountingPeriod } from "@/accounting-periods/types";
import GoalDashboardGoalTypeFilter from "@/goals/dashboard/GoalDashboardGoalTypeFilter";
import type { GoalType } from "@/goals/types";
import type { JSX } from "react";

/**
 * Props for the GoalDashboardFilter component.
 */
interface GoalDashboardFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly availableFundNames: readonly string[];
  readonly defaultAccountingPeriodId: string | null;
  readonly defaultStartDate: string;
  readonly defaultEndDate: string;
  readonly disabled?: boolean;
}

/**
 * Renders the dashboard filter card for the Goals view.
 */
const GoalDashboardFilter = function ({
  accountingPeriods,
  availableFundNames,
  defaultAccountingPeriodId,
  disabled = false,
}: GoalDashboardFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const pageParamName = "page";
  const modeParamName = "mode";
  const goalTypeParamName = "goalType";
  const fundNameParamName = "fundName";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";

  const currentGoalTypes = normalizeGoalTypes(
    searchParams.getAll(goalTypeParamName),
  );
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
    router.replace(nextQuery === "" ? pathname : `${pathname}?${nextQuery}`, {
      scroll: false,
    });
  };

  const hasActiveView =
    shouldPersistGoalTypes(currentGoalTypes) ||
    shouldPersistFundNames(currentFundNames) ||
    currentStartAccountingPeriodId !== (defaultAccountingPeriodId ?? "") ||
    currentEndAccountingPeriodId !== (defaultAccountingPeriodId ?? "");

  const handleGoalTypeChange = function (
    nextGoalTypes: readonly GoalType[],
  ): void {
    updateParams((params) => {
      params.delete(goalTypeParamName);
      if (shouldPersistGoalTypes(nextGoalTypes)) {
        nextGoalTypes.forEach((goalType) => {
          params.append(goalTypeParamName, goalType);
        });
      }
    });
  };

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

  const clearView = function (): void {
    updateParams((params) => {
      params.delete(goalTypeParamName);
      params.delete(fundNameParamName);
      params.set(modeParamName, "date");
      params.delete(startAccountingPeriodIdParamName);
      params.delete(endAccountingPeriodIdParamName);
    });
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
          <Typography variant="h5">Goal Dashboard</Typography>
        </Stack>
        <Stack
          direction="row"
          spacing={1.5}
          useFlexGap
          flexWrap="wrap"
          alignItems={{ xs: "stretch", md: "center" }}
        >
          <AccountDashboardAccountingPeriodFilter
            accountingPeriods={accountingPeriods}
            label="Start period"
            value={currentStartAccountingPeriodId}
            onChange={handleStartAccountingPeriodChange}
            disabled={disabled}
          />
          <AccountDashboardAccountingPeriodFilter
            accountingPeriods={accountingPeriods}
            label="End period"
            value={currentEndAccountingPeriodId}
            onChange={handleEndAccountingPeriodChange}
            disabled={disabled}
          />
          <GoalDashboardGoalTypeFilter
            value={currentGoalTypes}
            onChange={handleGoalTypeChange}
            disabled={disabled}
          />
          <Autocomplete
            multiple
            disableCloseOnSelect
            size="small"
            options={[...availableFundNames]}
            value={[...currentFundNames]}
            disabled={disabled || availableFundNames.length === 0}
            limitTags={1}
            sx={{ minWidth: { xs: "100%", sm: 280 }, flex: { md: 1 } }}
            noOptionsText={
              availableFundNames.length === 0
                ? "No fund names available"
                : "No fund names found"
            }
            slotProps={{
              paper: {
                sx: {
                  "& .MuiAutocomplete-listbox": {
                    maxHeight: 320,
                  },
                },
              },
            }}
            onChange={(_, nextFundNames) => {
              handleFundNameChange(nextFundNames);
            }}
            renderOption={(props, option, { selected }) => (
              <li {...props}>
                <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
                {option}
              </li>
            )}
            renderInput={(params) => (
              <TextField
                {...params}
                label="Fund names"
                {...(currentFundNames.length === 0
                  ? { placeholder: "All fund names" }
                  : {})}
              />
            )}
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

export default GoalDashboardFilter;
