"use client";

import { Autocomplete, Checkbox, TextField } from "@mui/material";
import type { JSX } from "react";
import { normalizeFundNames } from "@/funds/dashboard/fundNameFilter";

interface FundDashboardFundNameFilterProps {
  readonly availableFundNames: readonly string[];
  readonly value: readonly string[];
  readonly onChange: (fundNames: readonly string[]) => void;
  readonly disabled?: boolean;
}

/**
 * Renders the fund name multi-select with built-in search and scrolling.
 */
const FundDashboardFundNameFilter = function ({
  availableFundNames,
  value,
  onChange,
  disabled = false,
}: FundDashboardFundNameFilterProps): JSX.Element {
  return (
    <Autocomplete
      multiple
      disableCloseOnSelect
      size="small"
      options={[...availableFundNames]}
      value={[...value]}
      disabled={disabled || availableFundNames.length === 0}
      limitTags={1}
      sx={{ minWidth: { xs: "100%", sm: 280 }, flex: { md: 1 } }}
      noOptionsText={
        availableFundNames.length === 0
          ? "No matching fund names"
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
        onChange(normalizeFundNames(nextFundNames, availableFundNames));
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
          {...(value.length === 0 ? { placeholder: "All fund names" } : {})}
        />
      )}
    />
  );
};

export default FundDashboardFundNameFilter;
