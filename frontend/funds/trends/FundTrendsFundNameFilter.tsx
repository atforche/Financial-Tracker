"use client";

import type { JSX } from "react";
import MultiSelectAutocompleteFilter from "@/framework/forms/MultiSelectAutocompleteFilter";
import { normalizeFundNames } from "@/funds/trends/fundNameFilter";

interface FundTrendsFundNameFilterProps {
  readonly availableFundNames: readonly string[];
  readonly value: readonly string[];
  readonly onChange: (fundNames: readonly string[]) => void;
  readonly disabled?: boolean;
}

/**
 * Renders the fund name multi-select with built-in search and scrolling.
 */
const FundTrendsFundNameFilter = function ({
  availableFundNames,
  value,
  onChange,
  disabled = false,
}: FundTrendsFundNameFilterProps): JSX.Element {
  return (
    <MultiSelectAutocompleteFilter
      label="Fund names"
      options={availableFundNames}
      value={value}
      disabled={disabled || availableFundNames.length === 0}
      placeholder="All fund names"
      noOptionsText={
        availableFundNames.length === 0
          ? "No matching fund names"
          : "No fund names found"
      }
      onChange={(nextFundNames) => {
        onChange(normalizeFundNames(nextFundNames, availableFundNames));
      }}
    />
  );
};

export default FundTrendsFundNameFilter;
