"use client";

import { Button, Stack, Typography } from "@mui/material";
import {
  accountWorkspaceParamNames,
  clearAccountWorkspaceFilters,
} from "@/accounts/workspace/searchParams";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/accountTypeFilterHelpers";
import { useRouter, useSearchParams } from "next/navigation";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/AccountWorkspace";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import WorkspaceCard from "@/framework/view/WorkspaceCard";
import { formatCurrency } from "@/framework/currencyHelpers";
import { getAccountCardColor } from "@/accounts/workspace/helpers";
import routes from "@/accounts/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the AccountWorkspaceCards component.
 */
interface AccountWorkspaceCardsProps {
  readonly data: AccountWithBalance[];
  readonly isInOnboardingMode: boolean;
}

/**
 * Displays the account workspace as a collection of clickable cards.
 */
const AccountWorkspaceCards = function ({
  data,
  isInOnboardingMode,
}: AccountWorkspaceCardsProps): JSX.Element {
  const searchParams = useSearchParams();
  const router = useRouter();
  const updateParams = useSearchParamUpdater([]);

  const {
    action: actionParamName,
    accountType: accountTypeParamName,
    search: searchParamName,
  } = accountWorkspaceParamNames;

  const clearFilters = function (): void {
    updateParams((params) => {
      clearAccountWorkspaceFilters(params);
      params.delete(actionParamName);
    });
  };

  const openAccount = function (accountId: string): void {
    const params = new URLSearchParams(searchParams.toString());
    const detailSearchParams: AccountWorkspaceSearchParams = {};
    const search = params.get(searchParamName);
    const accountTypes = normalizeAccountTypes(
      params.getAll(accountTypeParamName),
    );
    if (search !== null) {
      detailSearchParams.search = search;
    }
    if (accountTypes.length > 0 && shouldPersistAccountTypes(accountTypes)) {
      detailSearchParams.accountType = accountTypes;
    }
    router.push(routes.workspaceDetail(accountId, detailSearchParams), {
      scroll: false,
    });
  };

  const hasSearch = (searchParams.get(searchParamName) ?? "").trim() !== "";
  const hasAccountTypeFilter =
    searchParams.getAll(accountTypeParamName).length > 0;
  const hasActiveFilters = hasSearch || hasAccountTypeFilter;
  const accounts = data;

  return accounts.length === 0 ? (
    <Stack spacing={2} alignItems="flex-start">
      <Typography color="text.secondary">
        {hasActiveFilters
          ? "No accounts match the current filters. Try a different search or account type."
          : isInOnboardingMode
            ? "Use onboarding to add the first account to your workspace."
            : "Create an account to start building your workspace."}
      </Typography>
      {hasActiveFilters ? (
        <Button variant="contained" onClick={clearFilters}>
          Clear Filters
        </Button>
      ) : null}
    </Stack>
  ) : (
    <ResponsiveGrid minimumColumnWidth={280} spacing={2}>
      {accounts.map((account) => (
        <WorkspaceCard
          key={account.id}
          title={account.name}
          color={getAccountCardColor(account.type)}
          onClick={() => {
            openAccount(account.id);
          }}
        >
          <Stack spacing={0.5}>
            <Typography
              variant="overline"
              sx={{ color: "text.secondary", fontWeight: 700 }}
            >
              Current balance
            </Typography>
            <Typography variant="h5">
              {formatCurrency(account.currentBalance.postedBalance)}
            </Typography>
          </Stack>
        </WorkspaceCard>
      ))}
    </ResponsiveGrid>
  );
};

export default AccountWorkspaceCards;
