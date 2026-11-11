"use client";

import {
  accountWorkspaceParamNames,
  clearAccountWorkspaceFilters,
  parseAccountWorkspaceFilters,
} from "@/accounts/workspace/searchParams";
import type { AccountType } from "@/accounts/types";
import AccountTypeFilter from "@/accounts/AccountTypeFilter";
import ActionIconButton from "@/framework/view/ActionIconButton";
import type { JSX } from "react";
import ResetFiltersButton from "@/framework/view/ResetFiltersButton";
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
    ? "Onboard account"
    : "Create account";

  return (
    <WorkspaceHeader
      title="Accounts"
      action={
        canWrite ? (
          <ActionIconButton
            action="add"
            label={addActionLabel}
            href={
              isInOnboardingMode
                ? routes.workspaceOnboard()
                : routes.workspaceCreate()
            }
          />
        ) : null
      }
    >
      <AccountTypeFilter
        value={currentAccountTypes}
        onChange={handleAccountTypeChange}
      />
      <SearchBar searchParamName={searchParamName} />
      <ResetFiltersButton
        active={filters.hasActiveFilters}
        onClick={clearView}
      />
    </WorkspaceHeader>
  );
};

export default AccountWorkspaceFilter;
