"use client";

import { Box, Button, ButtonBase, Stack, Typography } from "@mui/material";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/accountTypeFilterHelpers";
import { useRouter, useSearchParams } from "next/navigation";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/AccountWorkspace";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import { formatCurrency } from "@/framework/currencyHelpers";
import { getAccountCardColor } from "@/accounts/workspace/helpers";
import nameof from "@/framework/data/nameof";
import routes from "@/accounts/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the AccountWorkspaceCards component.
 */
interface AccountWorkspaceCardsProps {
  readonly data: AccountWithBalance[] | null;
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

  const actionParamName = nameof<AccountWorkspaceSearchParams>("action");
  const searchParamName = nameof<AccountWorkspaceSearchParams>("search");
  const accountTypeParamName =
    nameof<AccountWorkspaceSearchParams>("accountType");

  const clearFilters = function (): void {
    updateParams((params) => {
      params.delete(searchParamName);
      params.delete(accountTypeParamName);
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
  const accounts = data ?? [];

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
          Clear filters
        </Button>
      ) : null}
    </Stack>
  ) : (
    <Box
      sx={{
        display: "grid",
        gap: 2,
        justifyContent: "start",
        justifyItems: "stretch",
        alignItems: "start",
        gridTemplateColumns: {
          xs: "minmax(0, 1fr)",
          sm: "repeat(auto-fit, minmax(280px, max-content))",
        },
      }}
    >
      {accounts.map((account) => (
        <ButtonBase
          key={account.id}
          onClick={() => {
            openAccount(account.id);
          }}
          sx={{
            display: "flex",
            width: "100%",
            minWidth: 0,
            borderRadius: 5,
            textAlign: "left",
            "& .MuiPaper-root": {
              width: "100%",
            },
          }}
        >
          <Frame
            title={account.name}
            color={getAccountCardColor(account.type)}
            headerContent={
              <KeyboardArrowRight
                sx={{ color: "text.secondary", fontSize: 22 }}
              />
            }
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
          </Frame>
        </ButtonBase>
      ))}
    </Box>
  );
};

export default AccountWorkspaceCards;
