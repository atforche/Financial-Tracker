"use client";

import { Autocomplete, TextField } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { JSX } from "react";

interface FundTrendsAccountingPeriodFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly label: string;
  readonly value: string;
  readonly onChange: (accountingPeriodId: string) => void;
  readonly disabled?: boolean;
}

const fieldSx = {
  minWidth: { xs: "100%", sm: 180 },
};

/**
 * Renders a single accounting period selector used in the fund trends.
 */
const FundTrendsAccountingPeriodFilter = function ({
  accountingPeriods,
  label,
  value,
  onChange,
  disabled = false,
}: FundTrendsAccountingPeriodFilterProps): JSX.Element {
  const isDisabled = disabled || accountingPeriods.length === 0;
  const sortedPeriods = [...accountingPeriods].sort((a, b) => {
    if (b.year !== a.year) {
      return b.year - a.year;
    }
    return b.month - a.month;
  });
  const selectedPeriod =
    sortedPeriods.find((accountingPeriod) => accountingPeriod.id === value) ??
    null;

  return (
    <Autocomplete
      size="small"
      options={sortedPeriods}
      value={selectedPeriod}
      onChange={(_, nextAccountingPeriod) => {
        if (nextAccountingPeriod !== null) {
          onChange(nextAccountingPeriod.id);
        }
      }}
      disabled={isDisabled}
      sx={fieldSx}
      getOptionLabel={(accountingPeriod) => accountingPeriod.name}
      isOptionEqualToValue={(option, selectedValue) =>
        option.id === selectedValue.id
      }
      noOptionsText={
        accountingPeriods.length === 0
          ? "No accounting periods available"
          : "No accounting periods found"
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
      renderInput={(params) => <TextField {...params} label={label} />}
    />
  );
};

export default FundTrendsAccountingPeriodFilter;
