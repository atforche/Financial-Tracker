"use client";

import { Autocomplete, TextField } from "@mui/material";
import { type JSX, useMemo } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { compareAccountingPeriods } from "@/accounting-periods/helpers";

/**
 * Props for the AccountingPeriodFilter component.
 */
interface AccountingPeriodFilterProps {
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
 * Renders a single accounting period selector.
 */
const AccountingPeriodFilter = function ({
  accountingPeriods,
  label,
  value,
  onChange,
  disabled = false,
}: AccountingPeriodFilterProps): JSX.Element {
  const isDisabled = disabled || accountingPeriods.length === 0;
  const sortedPeriods = useMemo(
    () => [...accountingPeriods].sort((a, b) => compareAccountingPeriods(b, a)),
    [accountingPeriods],
  );
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

export default AccountingPeriodFilter;
