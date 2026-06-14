"use client";

import { Autocomplete, Checkbox, TextField } from "@mui/material";
import type {
  GoalDashboardGoalType,
  GoalDashboardView,
} from "@/goals/dashboard/goalDashboardTypes";
import {
  formatAssignmentGoalType,
  formatSpendingGoalType,
} from "@/goals/types";
import {
  getGoalTypeValues,
  normalizeGoalTypes,
} from "@/goals/dashboard/goalTypeFilter";
import type { JSX } from "react";

interface GoalDashboardGoalTypeFilterProps {
  readonly view: GoalDashboardView;
  readonly value: readonly GoalDashboardGoalType[];
  readonly onChange: (goalTypes: readonly GoalDashboardGoalType[]) => void;
  readonly disabled?: boolean;
}

/**
 * Renders the goal type multi-select with built-in search and scrolling.
 */
const GoalDashboardGoalTypeFilter = function ({
  view,
  value,
  onChange,
  disabled = false,
}: GoalDashboardGoalTypeFilterProps): JSX.Element {
  if (view === "assignment") {
    const goalTypeValues = getGoalTypeValues(view);
    const selectedGoalTypes = normalizeGoalTypes(value, view);
    const label = "Assignment goal types";

    return (
      <Autocomplete
        multiple
        disableCloseOnSelect
        size="small"
        options={[...goalTypeValues]}
        value={[...selectedGoalTypes]}
        disabled={disabled}
        limitTags={1}
        sx={{ minWidth: { xs: "100%", sm: 280 }, flex: { md: 1 } }}
        noOptionsText={
          goalTypeValues.length === 0
            ? "No goal types available"
            : "No goal types found"
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
        onChange={(_, nextGoalTypes) => {
          onChange(normalizeGoalTypes(nextGoalTypes, view));
        }}
        renderOption={(props, option, { selected }) => (
          <li {...props}>
            <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
            {formatAssignmentGoalType(option)}
          </li>
        )}
        renderInput={(params) => (
          <TextField
            {...params}
            label={label}
            {...(selectedGoalTypes.length === 0
              ? { placeholder: `All ${label.toLowerCase()}` }
              : {})}
          />
        )}
      />
    );
  }

  const goalTypeValues = getGoalTypeValues(view);
  const selectedGoalTypes = normalizeGoalTypes(value, view);
  const label = "Spending goal types";

  return (
    <Autocomplete
      multiple
      disableCloseOnSelect
      size="small"
      options={[...goalTypeValues]}
      value={[...selectedGoalTypes]}
      disabled={disabled}
      limitTags={1}
      sx={{ minWidth: { xs: "100%", sm: 280 }, flex: { md: 1 } }}
      noOptionsText={
        goalTypeValues.length === 0
          ? "No goal types available"
          : "No goal types found"
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
      onChange={(_, nextGoalTypes) => {
        onChange(normalizeGoalTypes(nextGoalTypes, view));
      }}
      renderOption={(props, option, { selected }) => (
        <li {...props}>
          <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
          {formatSpendingGoalType(option)}
        </li>
      )}
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          {...(selectedGoalTypes.length === 0
            ? { placeholder: `All ${label.toLowerCase()}` }
            : {})}
        />
      )}
    />
  );
};

export default GoalDashboardGoalTypeFilter;
