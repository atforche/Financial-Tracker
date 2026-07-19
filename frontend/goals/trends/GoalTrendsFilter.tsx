"use client";

import type {
  AccountingPeriod,
  AccountingPeriodRange,
} from "@/accounting-periods/types";
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
import AccountingPeriodRangeFilter from "@/accounting-periods/AccountingPeriodRangeFilter";
import { Button } from "@mui/material";
import GoalTrendsGoalTypeFilter from "@/goals/trends/GoalTrendsGoalTypeFilter";
import type { GoalTrendsSearchParams } from "@/goals/trends/GoalTrends";
import type { JSX } from "react";
import MultiSelectAutocompleteFilter from "@/framework/forms/MultiSelectAutocompleteFilter";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import nameof from "@/framework/data/nameof";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the GoalTrendsFilter component.
 */
interface GoalTrendsFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly availableFundNames: readonly string[];
  readonly defaultAccountingPeriodId: string | null;
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

  const pageParamName = nameof<GoalTrendsSearchParams>("page");
  const balanceEventPageParamName =
    nameof<GoalTrendsSearchParams>("balanceEventPage");
  const sortParamName = nameof<GoalTrendsSearchParams>("sort");
  const balanceEventSortParamName =
    nameof<GoalTrendsSearchParams>("balanceEventSort");
  const viewParamName = nameof<GoalTrendsSearchParams>("view");
  const goalTypeParamName = nameof<GoalTrendsSearchParams>("goalType");
  const fundNameParamName = nameof<GoalTrendsSearchParams>("fundName");
  const startAccountingPeriodIdParamName = nameof<GoalTrendsSearchParams>(
    "startAccountingPeriodId",
  );
  const endAccountingPeriodIdParamName = nameof<GoalTrendsSearchParams>(
    "endAccountingPeriodId",
  );

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

  const handleViewChange = function (nextView: GoalTrendsView): void {
    if (nextView === view) {
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

  const handleAccountingPeriodRangeChange = function (
    range: AccountingPeriodRange,
  ): void {
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
        <ToggleButtonSelector
          value={view}
          onChange={handleViewChange}
          options={[
            { value: "assignment", label: "Assignment" },
            { value: "spending", label: "Spending" },
          ]}
        />
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
      <MultiSelectAutocompleteFilter
        label="Fund names"
        options={availableFundNames}
        value={currentFundNames}
        disabled={disabled || availableFundNames.length === 0}
        placeholder="All fund names"
        noOptionsText={
          availableFundNames.length === 0
            ? "No fund names available"
            : "No fund names found"
        }
        onChange={(nextFundNames) => {
          handleFundNameChange(nextFundNames);
        }}
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
