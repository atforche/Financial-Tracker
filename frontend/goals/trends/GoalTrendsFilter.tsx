"use client";

import {
  Autocomplete,
  Button,
  Checkbox,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
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
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import GoalTrendsGoalTypeFilter from "@/goals/trends/GoalTrendsGoalTypeFilter";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

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

  const updateParams = useSearchParamUpdater([
    pageParamName,
    balanceEventPageParamName,
  ]);

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

  const handleAccountingPeriodRangeChange = function (range: {
    readonly start: string;
    readonly end: string;
  }): void {
    updateParams((params) => {
      params.set(startAccountingPeriodIdParamName, range.start);
      params.set(endAccountingPeriodIdParamName, range.end);
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
    <PageFilterFrame
      title="Goal Trends"
      headerContent={
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
      }
    >
      <AccountingPeriodRangeFilter
        accountingPeriods={accountingPeriods}
        startValue={currentStartAccountingPeriodId}
        endValue={currentEndAccountingPeriodId}
        onChange={handleAccountingPeriodRangeChange}
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
    </PageFilterFrame>
  );
};

export default GoalTrendsFilter;
