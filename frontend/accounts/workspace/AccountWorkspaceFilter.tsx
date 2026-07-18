"use client";

import { Button, Stack } from "@mui/material";
import {
  accountWorkspaceParamNames,
  clearAccountWorkspaceFilters,
} from "@/accounts/workspace/searchParams";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/accountTypeFilterHelpers";
import type { AccountType } from "@/accounts/types";
import AccountTypeFilter from "@/accounts/AccountTypeFilter";
import type { JSX } from "react";
import PageFilterFrame from "@/framework/view/PageFilterFrame";
import SearchBar from "@/framework/listframe/SearchBar";
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

  const {
    action: actionParamName,
    accountType: accountTypeParamName,
    search: searchParamName,
  } = accountWorkspaceParamNames;

  const currentAccountTypes = normalizeAccountTypes(
    searchParams.getAll(accountTypeParamName),
  );

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

  const hasActiveView =
    (searchParams.get(searchParamName) ?? "").trim() !== "" ||
    shouldPersistAccountTypes(currentAccountTypes);

  const addActionLabel = isInOnboardingMode
    ? "Onboard Account"
    : "Create Account";

  const setAction = function (action: "create" | "onboard"): void {
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
            disabled={!hasActiveView}
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
