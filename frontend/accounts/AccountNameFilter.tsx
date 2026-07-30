"use client";

import type { JSX } from "react";
import MultiSelectAutocompleteFilter from "@/framework/forms/MultiSelectAutocompleteFilter";
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
    <MultiSelectAutocompleteFilter
      label="Account names"
      options={availableAccountNames}
      value={value}
      disabled={disabled || availableAccountNames.length === 0}
      placeholder="All account names"
      noOptionsText={
        availableAccountNames.length === 0
          ? "No matching account names"
          : "No account names found"
      }
      onChange={(nextAccountNames) => {
        onChange(
          normalizeAccountNames(nextAccountNames, availableAccountNames),
        );
      }}
    />
  );
};

export default AccountNameFilter;
