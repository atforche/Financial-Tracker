"use client";

import { type AccountType, formatAccountType } from "@/accounts/types";
import {
  type AccountTypeFilterGroup,
  accountTypeFilterGroups,
  formatSelectedAccountTypes,
  getAccountTypeGroupSelectionState,
  normalizeAccountTypes,
  toggleAccountTypeGroup,
} from "@/accounts/trends/accountTypeFilter";
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
import type { JSX } from "react";

interface AccountTrendsAccountTypeFilterProps {
  readonly value: readonly AccountType[];
  readonly onChange: (accountTypes: readonly AccountType[]) => void;
  readonly disabled?: boolean;
}

const groupValuePrefix = "__group__";

const isAccountTypeFilterGroupValue = function (
  value: string,
): value is `${typeof groupValuePrefix}${AccountTypeFilterGroup}` {
  return value.startsWith(groupValuePrefix);
};

const getAccountTypeFilterGroupFromValue = function (
  value: `${typeof groupValuePrefix}${AccountTypeFilterGroup}`,
): AccountTypeFilterGroup {
  const groupValue = value.slice(groupValuePrefix.length);
  if (groupValue === "tracked" || groupValue === "untracked") {
    return groupValue;
  }

  throw new Error(`Unrecognized account type filter group value: ${value}`);
};

/**
 * Renders the account type multi-select with tracked and untracked group toggles.
 */
const AccountTrendsAccountTypeFilter = function ({
  value,
  onChange,
  disabled = false,
}: AccountTrendsAccountTypeFilterProps): JSX.Element {
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
      <InputLabel id="account-overview-account-type-filter-label">
        Account types
      </InputLabel>
      <Select
        multiple
        labelId="account-overview-account-type-filter-label"
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

export default AccountTrendsAccountTypeFilter;
