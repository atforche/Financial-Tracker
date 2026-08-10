"use client";

import { Box, Button, Stack, Typography } from "@mui/material";
import {
  accountWorkspaceParamNames,
  clearAccountWorkspaceFilters,
  parseAccountWorkspaceFilters,
} from "@/accounts/workspace/searchParams";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/accountTypeFilterHelpers";
import { useRouter, useSearchParams } from "next/navigation";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/types";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CardResponsiveGrid from "@/framework/view/CardResponsiveGrid";
import type { JSX } from "react";
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

const noFinancialInstitutionLabel = "No Financial Institution";

const compareStrings = function (left: string, right: string): number {
  return left < right ? -1 : left > right ? 1 : 0;
};

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
  const filters = parseAccountWorkspaceFilters(searchParams);
  const filteredData = shouldPersistAccountTypes(filters.accountTypes)
    ? data.filter((account) => filters.accountTypes.includes(account.type))
    : data;

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
    if (shouldPersistAccountTypes(accountTypes)) {
      detailSearchParams.accountType = accountTypes;
    }
    router.push(routes.workspaceDetail(accountId, detailSearchParams), {
      scroll: false,
    });
  };

  const groupedAccounts = new Map<string | null, AccountWithBalance[]>();
  filteredData.forEach((account) => {
    const accounts = groupedAccounts.get(account.financialInstitution) ?? [];
    accounts.push(account);
    groupedAccounts.set(account.financialInstitution, accounts);
  });
  const groups = [...groupedAccounts.entries()].sort(([left], [right]) =>
    compareStrings(
      left ?? noFinancialInstitutionLabel,
      right ?? noFinancialInstitutionLabel,
    ),
  );
  groups.forEach(([, accounts]) => {
    accounts.sort((left, right) => compareStrings(left.name, right.name));
  });

  return filteredData.length === 0 ? (
    <Stack spacing={2} alignItems="flex-start">
      <Typography color="text.secondary">
        {filters.hasActiveFilters
          ? "No accounts match the current filters. Try a different search or account type."
          : isInOnboardingMode
            ? "Use onboarding to add the first account to your workspace."
            : "Create an account to start building your workspace."}
      </Typography>
      {filters.hasActiveFilters ? (
        <Button variant="contained" onClick={clearFilters}>
          Clear Filters
        </Button>
      ) : null}
    </Stack>
  ) : (
    <Box
      sx={{
        display: "flex",
        flexWrap: "wrap",
        gap: 2,
        alignItems: "flex-start",
        "& > *": {
          flex: "0 1 auto",
          minWidth: 0,
          maxWidth: "100%",
        },
      }}
    >
      {groups.map(([financialInstitution, accounts]) => (
        <CaptionedFrame
          key={
            financialInstitution === null
              ? "no-financial-institution"
              : `financial-institution:${financialInstitution}`
          }
          caption={financialInstitution ?? noFinancialInstitutionLabel}
          minWidth={0}
          maxWidth="100%"
          width="max-content"
        >
          <CardResponsiveGrid minimumColumnWidth={280} spacing={2} wrap>
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
          </CardResponsiveGrid>
        </CaptionedFrame>
      ))}
    </Box>
  );
};

export default AccountWorkspaceCards;
