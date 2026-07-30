"use client";

import {
  Checkbox,
  FormControl,
  InputLabel,
  ListItemText,
  MenuItem,
  OutlinedInput,
  Select,
  type SelectChangeEvent,
} from "@mui/material";
import { type JSX, useId } from "react";
import {
  accountTypeFilterGroups,
  formatSelectedAccountTypes,
  getAccountTypeFilterGroupFromValue,
  getAccountTypeGroupSelectionState,
  groupValuePrefix,
  isAccountTypeFilterGroupValue,
  normalizeAccountTypes,
  toggleAccountTypeGroup,
} from "@/accounts/accountTypeFilterHelpers";
import type { AccountType } from "@/accounts/types";
import { formatAccountType } from "@/accounts/helpers";

/**
 * Props for the AccountTypeFilter component.
 */
interface AccountTypeFilterProps {
  readonly value: readonly AccountType[];
  readonly onChange: (accountTypes: readonly AccountType[]) => void;
  readonly disabled?: boolean;
}

/**
 * Renders the account type multi-select with tracked and untracked group toggles.
 */
const AccountTypeFilter = function ({
  value,
  onChange,
  disabled = false,
}: AccountTypeFilterProps): JSX.Element {
  const labelId = useId();
  const handleChange = function (event: SelectChangeEvent<string[]>): void {
    const nextValue = event.target.value;
    const nextValues =
      typeof nextValue === "string" ? nextValue.split(",") : nextValue;
    const toggledGroupValue = nextValues.find(isAccountTypeFilterGroupValue);

    if (typeof toggledGroupValue !== "undefined") {
      onChange(
        toggleAccountTypeGroup(
          value,
          getAccountTypeFilterGroupFromValue(toggledGroupValue),
        ),
      );
      return;
    }

    onChange(normalizeAccountTypes(nextValues));
  };

  return (
    <FormControl size="small" sx={{ minWidth: { xs: "100%", sm: 220 } }}>
      <InputLabel id={labelId}>Account types</InputLabel>
      <Select
        multiple
        labelId={labelId}
        value={[...value]}
        onChange={handleChange}
        input={<OutlinedInput label="Account types" />}
        disabled={disabled}
        renderValue={(selected) => formatSelectedAccountTypes(selected)}
      >
        {accountTypeFilterGroups.map((group) => {
          const groupSelection = getAccountTypeGroupSelectionState(
            value,
            group.accountTypes,
          );
          const isGroupIndeterminate = groupSelection.someSelected
            ? !groupSelection.allSelected
            : false;

          return [
            <MenuItem
              key={group.value}
              value={`${groupValuePrefix}${group.value}`}
              sx={{ fontWeight: 600 }}
            >
              <Checkbox
                checked={groupSelection.allSelected}
                indeterminate={isGroupIndeterminate}
              />
              <ListItemText primary={group.label} />
            </MenuItem>,
            ...group.accountTypes.map((accountType) => (
              <MenuItem
                key={`${group.value}-${accountType}`}
                value={accountType}
                sx={{ pl: 5 }}
              >
                <Checkbox checked={value.includes(accountType)} />
                <ListItemText primary={formatAccountType(accountType)} />
              </MenuItem>
            )),
          ];
        })}
      </Select>
    </FormControl>
  );
};

export default AccountTypeFilter;
