"use client";

import { Autocomplete, Checkbox, TextField } from "@mui/material";
import {
  goalTypeValues,
  normalizeGoalTypes,
} from "@/goals/dashboard/goalTypeFilter";
import type { GoalType } from "@/goals/types";
import type { JSX } from "react";

interface GoalDashboardGoalTypeFilterProps {
  readonly value: readonly GoalType[];
  readonly onChange: (goalTypes: readonly GoalType[]) => void;
  readonly disabled?: boolean;
}

/**
 * Renders the goal type multi-select with built-in search and scrolling.
 */
const GoalDashboardGoalTypeFilter = function ({
  value,
  onChange,
  disabled = false,
}: GoalDashboardGoalTypeFilterProps): JSX.Element {
  return (
    <Autocomplete
      multiple
      disableCloseOnSelect
      size="small"
      options={[...goalTypeValues]}
      value={[...value]}
      disabled={disabled}
      limitTags={1}
      sx={{ minWidth: { xs: "100%", sm: 280 }, flex: { md: 1 } }}
      noOptionsText={
        value.length === 0 ? "No goal types available" : "No goal types found"
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
        onChange(normalizeGoalTypes(nextGoalTypes));
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
          label="Goal types"
          {...(value.length === 0 ? { placeholder: "All goal types" } : {})}
        />
      )}
    />
  );
};

export default GoalDashboardGoalTypeFilter;
