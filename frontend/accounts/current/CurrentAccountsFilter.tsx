"use client";

import { Button, Paper, Stack, Typography } from "@mui/material";
import {
  normalizeAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/trends/accountNameFilter";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/trends/accountTypeFilter";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AccountTrendsAccountNameFilter from "@/accounts/trends/AccountTrendsAccountNameFilter";
import AccountTrendsAccountTypeFilter from "@/accounts/trends/AccountTrendsAccountTypeFilter";
import type { AccountType } from "@/accounts/types";
import type { JSX } from "react";

interface CurrentAccountsFilterProps {
  readonly availableAccountNames: readonly string[];
  readonly disabled?: boolean;
}

/**
 * Filters the current accounts page.
 */
const CurrentAccountsFilter = function ({
  availableAccountNames,
  disabled = false,
}: CurrentAccountsFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const accountTypeParamName = "accountType";
  const accountNameParamName = "accountName";

  const currentAccountTypes = normalizeAccountTypes(
    searchParams.getAll(accountTypeParamName),
  );
  const currentAccountNames = normalizeAccountNames(
    searchParams.getAll(accountNameParamName),
    availableAccountNames,
  );

  const updateParams = function (
    updater: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    updater(params);
    const nextQuery = params.toString();
    router.replace(nextQuery === "" ? pathname : `${pathname}?${nextQuery}`, {
      scroll: false,
    });
  };

  const handleAccountTypeChange = function (
    nextAccountTypes: readonly AccountType[],
  ): void {
    updateParams((params) => {
      params.delete(accountTypeParamName);
      params.delete(accountNameParamName);
      if (shouldPersistAccountTypes(nextAccountTypes)) {
        nextAccountTypes.forEach((accountType) => {
          params.append(accountTypeParamName, accountType);
        });
      }
    });
  };

  const handleAccountNameChange = function (
    nextAccountNames: readonly string[],
  ): void {
    updateParams((params) => {
      params.delete(accountNameParamName);
      if (shouldPersistAccountNames(nextAccountNames)) {
        nextAccountNames.forEach((accountName) => {
          params.append(accountNameParamName, accountName);
        });
      }
    });
  };

  const clearView = function (): void {
    updateParams((params) => {
      params.delete(accountTypeParamName);
      params.delete(accountNameParamName);
    });
  };

  const hasActiveView =
    shouldPersistAccountTypes(currentAccountTypes) ||
    shouldPersistAccountNames(currentAccountNames);

  return (
    <Paper
      sx={{
        position: "sticky",
        top: 10,
        zIndex: (theme) => theme.zIndex.appBar - 1,
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        bgcolor: "background.paper",
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2}>
        <Typography variant="h5">Current Accounts</Typography>
        <Stack
          direction="row"
          spacing={1.5}
          useFlexGap
          flexWrap="wrap"
          alignItems={{ xs: "stretch", md: "center" }}
        >
          <AccountTrendsAccountTypeFilter
            value={currentAccountTypes}
            onChange={handleAccountTypeChange}
            disabled={disabled}
          />
          <AccountTrendsAccountNameFilter
            availableAccountNames={availableAccountNames}
            value={currentAccountNames}
            onChange={handleAccountNameChange}
            disabled={disabled}
          />
          <Button
            variant="outlined"
            onClick={clearView}
            disabled={!hasActiveView}
            sx={{ flexShrink: 0 }}
          >
            Reset filters
          </Button>
        </Stack>
      </Stack>
    </Paper>
  );
};

export default CurrentAccountsFilter;
