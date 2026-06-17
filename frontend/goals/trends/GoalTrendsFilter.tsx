"use client";

import {
  Autocomplete,
  Button,
  Checkbox,
  Paper,
  Stack,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from "@mui/material";
import {
  type GoalTrendsGoalType,
  type GoalTrendsView,
  defaultGoalTrendsView,
} from "@/goals/trends/goalTrendsTypes";
import {
  normalizeFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import {
  normalizeGoalTypes,
  shouldPersistGoalTypes,
} from "@/goals/trends/goalTypeFilter";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AccountTrendsAccountingPeriodFilter from "@/accounts/trends/AccountTrendsAccountingPeriodFilter";
import type { AccountingPeriod } from "@/accounting-periods/types";
import GoalTrendsGoalTypeFilter from "@/goals/trends/GoalTrendsGoalTypeFilter";
import type { JSX } from "react";

/**
 * Props for the GoalTrendsFilter component.
 */
interface GoalTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly availableFundNames: readonly string[];
  readonly defaultAccountingPeriodId: string | null;
  readonly defaultStartDate: string;
  readonly defaultEndDate: string;
  readonly view: GoalTrendsView;
  readonly disabled?: boolean;
}

/**
 * Renders the trends filter card for the Goals view.
 */
const GoalTrendsFilter = function ({
  accountingPeriods,
  availableFundNames,
  defaultAccountingPeriodId,
  view,
  disabled = false,
}: GoalTrendsFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const pageParamName = "page";
  const balanceEventPageParamName = "balanceEventPage";
  const sortParamName = "sort";
  const balanceEventSortParamName = "balanceEventSort";
  const viewParamName = "view";
  const goalTypeParamName = "goalType";
  const fundNameParamName = "fundName";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";

  const currentGoalTypes = normalizeGoalTypes(
    searchParams.getAll(goalTypeParamName),
    view,
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
    params.delete(balanceEventPageParamName);
    const nextQuery = params.toString();
    router.replace(nextQuery === "" ? pathname : `${pathname}?${nextQuery}`, {
      scroll: false,
    });
  };

  const hasActiveView =
    shouldPersistGoalTypes(currentGoalTypes, view) ||
    shouldPersistFundNames(currentFundNames) ||
    currentStartAccountingPeriodId !== (defaultAccountingPeriodId ?? "") ||
    currentEndAccountingPeriodId !== (defaultAccountingPeriodId ?? "");

  const handleViewChange = function (nextView: GoalTrendsView | null): void {
    if (nextView === null || nextView === view) {
      return;
    }

    updateParams((params) => {
      if (nextView === defaultGoalTrendsView) {
        params.delete(viewParamName);
      } else {
        params.set(viewParamName, nextView);
      }
      params.delete(goalTypeParamName);
      params.delete(sortParamName);
      params.delete(balanceEventSortParamName);
    });
  };

  const handleGoalTypeChange = function (
    nextGoalTypes: readonly GoalTrendsGoalType[],
  ): void {
    updateParams((params) => {
      params.delete(goalTypeParamName);
      if (shouldPersistGoalTypes(nextGoalTypes, view)) {
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
      params.delete(startAccountingPeriodIdParamName);
      params.delete(endAccountingPeriodIdParamName);
      params.delete(sortParamName);
      params.delete(balanceEventSortParamName);
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
        <Stack
          direction={{ xs: "column", lg: "row" }}
          spacing={1.5}
          justifyContent="space-between"
          alignItems={{ xs: "flex-start", lg: "center" }}
        >
          <Stack spacing={0.5}>
            <Typography variant="h5">Goal Trends</Typography>
          </Stack>
          <ToggleButtonGroup
            exclusive
            size="small"
            value={view}
            onChange={(_, nextValue: GoalTrendsView | null) => {
              handleViewChange(nextValue);
            }}
          >
            <ToggleButton value="assignment">Assignment</ToggleButton>
            <ToggleButton value="spending">Spending</ToggleButton>
          </ToggleButtonGroup>
        </Stack>
        <Stack
          direction="row"
          spacing={1.5}
          useFlexGap
          flexWrap="wrap"
          alignItems={{ xs: "stretch", md: "center" }}
        >
          <AccountTrendsAccountingPeriodFilter
            accountingPeriods={accountingPeriods}
            label="Start period"
            value={currentStartAccountingPeriodId}
            onChange={handleStartAccountingPeriodChange}
            disabled={disabled}
          />
          <AccountTrendsAccountingPeriodFilter
            accountingPeriods={accountingPeriods}
            label="End period"
            value={currentEndAccountingPeriodId}
            onChange={handleEndAccountingPeriodChange}
            disabled={disabled}
          />
          <GoalTrendsGoalTypeFilter
            view={view}
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

export default GoalTrendsFilter;
