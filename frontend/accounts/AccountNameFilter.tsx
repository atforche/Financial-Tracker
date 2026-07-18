"use client";

import { Autocomplete, Checkbox, TextField } from "@mui/material";
import type { JSX } from "react";
import { normalizeAccountNames } from "@/accounts/accountNameFilterHelpers";

/**
 * Props for the AccountNameFilter component.
 */
interface AccountNameFilterProps {
  readonly availableAccountNames: readonly string[];
  readonly value: readonly string[];
  readonly onChange: (accountNames: readonly string[]) => void;
  readonly disabled?: boolean;
}

/**
 * Renders the account name multi-select with built-in search and scrolling.
 */
const AccountNameFilter = function ({
  availableAccountNames,
  value,
  onChange,
  disabled = false,
}: AccountNameFilterProps): JSX.Element {
  return (
    <Autocomplete
      multiple
      disableCloseOnSelect
      size="small"
      options={[...availableAccountNames]}
      value={[...value]}
      disabled={disabled || availableAccountNames.length === 0}
      limitTags={1}
      sx={{ minWidth: { xs: "100%", sm: 280 }, flex: { md: 1 } }}
      noOptionsText={
        availableAccountNames.length === 0
          ? "No matching account names"
          : "No account names found"
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
      onChange={(_, nextAccountNames) => {
        onChange(
          normalizeAccountNames(nextAccountNames, availableAccountNames),
        );
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
          label="Account names"
          {...(value.length === 0 ? { placeholder: "All account names" } : {})}
        />
      )}
    />
  );
};

export default AccountNameFilter;
