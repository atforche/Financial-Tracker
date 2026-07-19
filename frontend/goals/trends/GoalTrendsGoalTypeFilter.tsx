"use client";

import { AssignmentGoalType, SpendingGoalType } from "@/goals/types";
import type {
  GoalTrendsGoalType,
  GoalTrendsView,
} from "@/goals/trends/goalTrendsTypes";
import {
  formatAssignmentGoalType,
  formatSpendingGoalType,
} from "@/goals/helpers";
import {
  getGoalTypeValues,
  normalizeGoalTypes,
} from "@/goals/trends/goalTypeFilter";
import type { JSX } from "react";
import MultiSelectAutocompleteFilter from "@/framework/forms/MultiSelectAutocompleteFilter";

interface GoalTrendsGoalTypeFilterProps {
  readonly view: GoalTrendsView;
  readonly value: readonly GoalTrendsGoalType[];
  readonly onChange: (goalTypes: readonly GoalTrendsGoalType[]) => void;
  readonly disabled?: boolean;
}

/**
 * Renders the goal type multi-select with built-in search and scrolling.
 */
const GoalTrendsGoalTypeFilter = function ({
  view,
  value,
  onChange,
  disabled = false,
}: GoalTrendsGoalTypeFilterProps): JSX.Element {
  const goalTypeValues = getGoalTypeValues(view);
  const selectedGoalTypes = normalizeGoalTypes(value, view);
  const isAssignment = view === "assignment";
  const label = isAssignment ? "Assignment goal types" : "Spending goal types";
  const formatGoalType = function (goalType: GoalTrendsGoalType): string {
    switch (goalType) {
      case AssignmentGoalType.MonthlyTarget:
      case AssignmentGoalType.RecurringContribution:
        return formatAssignmentGoalType(goalType);
      case SpendingGoalType.Standard:
      case SpendingGoalType.Debt:
        return formatSpendingGoalType(goalType);
      default:
        return String(goalType);
    }
  };

  return (
    <MultiSelectAutocompleteFilter
      label={label}
      options={goalTypeValues}
      value={selectedGoalTypes}
      disabled={disabled}
      placeholder={`All ${label.toLowerCase()}`}
      noOptionsText={
        goalTypeValues.length === 0
          ? "No goal types available"
          : "No goal types found"
      }
      getOptionLabel={formatGoalType}
      onChange={(nextGoalTypes) => {
        onChange(normalizeGoalTypes(nextGoalTypes, view));
      }}
    />
  );
};

export default GoalTrendsGoalTypeFilter;
