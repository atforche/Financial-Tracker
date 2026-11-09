"use client";

import {
  accountWorkspaceParamNames,
  clearAccountWorkspaceFilters,
  parseAccountWorkspaceFilters,
} from "@/accounts/workspace/searchParams";
import type { AccountType } from "@/accounts/types";
import AccountTypeFilter from "@/accounts/AccountTypeFilter";
import type { AccountWorkspaceAction } from "@/accounts/workspace/types";
import { Button } from "@mui/material";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import WorkspaceHeader from "@/framework/view/WorkspaceHeader";
import { shouldPersistAccountTypes } from "@/accounts/accountTypeFilterHelpers";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the AccountWorkspaceFilter component.
 */
interface AccountWorkspaceFilterProps {
  readonly isInOnboardingMode: boolean;
}

/**
 * Renders the Accounts workspace header with its filters and primary action.
 */
const AccountWorkspaceFilter = function ({
  isInOnboardingMode,
}: AccountWorkspaceFilterProps): JSX.Element {
  const canWrite = useWriteAccess();
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
    <WorkspaceHeader
      title="Accounts"
      action={
        canWrite ? (
          <Button
            variant="contained"
            onClick={() => {
              setAction(isInOnboardingMode ? "onboard" : "create");
            }}
          >
            {addActionLabel}
          </Button>
        ) : null
      }
    >
      <AccountTypeFilter
        value={currentAccountTypes}
        onChange={handleAccountTypeChange}
      />
      <SearchBar searchParamName={searchParamName} />
      <Button onClick={clearView} disabled={!filters.hasActiveFilters}>
        Reset Filters
      </Button>
    </WorkspaceHeader>
  );
};

export default AccountWorkspaceFilter;
