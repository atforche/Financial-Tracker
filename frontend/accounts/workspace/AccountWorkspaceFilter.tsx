"use client";

import {
  accountWorkspaceParamNames,
  clearAccountWorkspaceFilters,
  parseAccountWorkspaceFilters,
} from "@/accounts/workspace/searchParams";
import type { AccountType } from "@/accounts/types";
import AccountTypeFilter from "@/accounts/AccountTypeFilter";
import { Button } from "@mui/material";
import type { JSX } from "react";
import SearchBar from "@/framework/view/SearchBar";
import WorkspaceHeader from "@/framework/view/WorkspaceHeader";
import routes from "@/accounts/routes";
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

  const { accountType: accountTypeParamName, search: searchParamName } =
    accountWorkspaceParamNames;

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

  return (
    <WorkspaceHeader
      title="Accounts"
      action={
        canWrite ? (
          <Button
            variant="contained"
            href={
              isInOnboardingMode
                ? routes.workspaceOnboard()
                : routes.workspaceCreate()
            }
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
