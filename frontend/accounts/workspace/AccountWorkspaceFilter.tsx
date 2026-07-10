"use client";

import { Button, Paper, Stack, Typography } from "@mui/material";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/trends/accountTypeFilter";
import AccountTrendsAccountTypeFilter from "@/accounts/trends/AccountTrendsAccountTypeFilter";
import type { AccountType } from "@/accounts/types";
import type { JSX } from "react";
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

  const searchParamName = "search";
  const accountTypeParamName = "accountType";
  const pageParamName = "page";

  const currentAccountTypes = normalizeAccountTypes(
    searchParams.getAll(accountTypeParamName),
  );

  const updateParams = useSearchParamUpdater([pageParamName]);

  const handleAccountTypeChange = function (
    nextAccountTypes: readonly AccountType[],
  ): void {
    updateParams((params) => {
      params.delete(accountTypeParamName);
      params.delete(pageParamName);
      if (shouldPersistAccountTypes(nextAccountTypes)) {
        nextAccountTypes.forEach((accountType) => {
          params.append(accountTypeParamName, accountType);
        });
      }
    });
  };

  const clearView = function (): void {
    updateParams((params) => {
      params.delete(searchParamName);
      params.delete(accountTypeParamName);
      params.delete(pageParamName);
    });
  };

  const hasActiveView =
    (searchParams.get(searchParamName) ?? "").trim() !== "" ||
    shouldPersistAccountTypes(currentAccountTypes);

  const actionParamName = "action";
  const addActionLabel = isInOnboardingMode
    ? "Onboard account"
    : "Create account";

  const setAction = function (action: "create" | "onboard"): void {
    updateParams((params) => {
      params.set(actionParamName, action);
    });
  };

  return (
    <Paper
      sx={{
        top: 10,
        zIndex: (theme) => theme.zIndex.appBar - 1,
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        bgcolor: "background.paper",
        p: { xs: 2, md: 2.5 },
        maxWidth: 1440,
      }}
    >
      <Stack spacing={2}>
        <Typography variant="h5">Accounts Workspace</Typography>
        <Stack
          direction="row"
          spacing={1.5}
          useFlexGap
          flexWrap={{ xs: "wrap", md: "nowrap" }}
          alignItems={{ xs: "stretch", md: "center" }}
        >
          <AccountTrendsAccountTypeFilter
            value={currentAccountTypes}
            onChange={handleAccountTypeChange}
          />
          <SearchBar
            searchParamName={searchParamName}
            pageParamName={pageParamName}
          />
          <Button
            variant="outlined"
            onClick={clearView}
            disabled={!hasActiveView}
            sx={{ flexShrink: 0 }}
          >
            Reset filters
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
      </Stack>
    </Paper>
  );
};

export default AccountWorkspaceFilter;
