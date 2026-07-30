"use client";

import { Button, Stack } from "@mui/material";
import {
  accountWorkspaceParamNames,
  clearAccountWorkspaceFilters,
  parseAccountWorkspaceFilters,
} from "@/accounts/workspace/searchParams";
import type { AccountType } from "@/accounts/types";
import AccountTypeFilter from "@/accounts/AccountTypeFilter";
import type { AccountWorkspaceAction } from "@/accounts/workspace/types";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import SearchBar from "@/framework/listframe/SearchBar";
import { shouldPersistAccountTypes } from "@/accounts/accountTypeFilterHelpers";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the AccountWorkspaceFilter component.
 */
interface AccountWorkspaceFilterProps {
  readonly isInOnboardingMode: boolean;
}

/**
 * Renders the filter card for the Accounts workspace with header and search bar.
 */
const AccountWorkspaceFilter = function ({
  isInOnboardingMode,
}: AccountWorkspaceFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const filters = parseAccountWorkspaceFilters(searchParams);

  const {
    action: actionParamName,
    accountType: accountTypeParamName,
    search: searchParamName,
  } = accountWorkspaceParamNames;

  const currentAccountTypes = filters.accountTypes;

  const updateParams = useSearchParamUpdater([]);

  const handleAccountTypeChange = function (
    nextAccountTypes: readonly AccountType[],
  ): void {
    updateParams((params) => {
      params.delete(accountTypeParamName);
      if (shouldPersistAccountTypes(nextAccountTypes)) {
        nextAccountTypes.forEach((accountType) => {
          params.append(accountTypeParamName, accountType);
        });
      }
    });
  };

  const clearView = function (): void {
    updateParams((params) => {
      clearAccountWorkspaceFilters(params);
    });
  };

  const addActionLabel = isInOnboardingMode
    ? "Onboard Account"
    : "Create Account";

  const setAction = function (action: AccountWorkspaceAction): void {
    updateParams((params) => {
      params.set(actionParamName, action);
    });
  };

  return (
    <PageFilterFrame
      title="Accounts Workspace"
      actions={
        <Stack direction="row" spacing={1.5} useFlexGap flexWrap="wrap">
          <Button
            variant="outlined"
            onClick={clearView}
            disabled={!filters.hasActiveFilters}
          >
            Reset Filters
          </Button>
          <Button
            variant="contained"
            onClick={() => {
              setAction(isInOnboardingMode ? "onboard" : "create");
            }}
          >
            {addActionLabel}
          </Button>
        </Stack>
      }
    >
      <AccountTypeFilter
        value={currentAccountTypes}
        onChange={handleAccountTypeChange}
      />
      <SearchBar searchParamName={searchParamName} />
    </PageFilterFrame>
  );
};

export default AccountWorkspaceFilter;
