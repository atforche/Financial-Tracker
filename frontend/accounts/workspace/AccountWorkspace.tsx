import { Box, Stack } from "@mui/material";
import type { AccountSortOrder } from "@/accounts/types";
import AccountWorkspaceActions from "@/accounts/workspace/AccountWorkspaceActions";
import AccountWorkspaceFilter from "@/accounts/workspace/AccountWorkspaceFilter";
import AccountWorkspaceListFrame from "@/accounts/workspace/AccountWorkspaceListFrame";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounts/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

type AccountWorkspaceAction = "create" | "onboard" | "update" | "delete";

/**
 * Search parameters supported by the Accounts workspace.
 */
interface AccountWorkspaceSearchParams {
  search?: string;
  sort?: AccountSortOrder;
  page?: number;
  selectedAccountId?: string;
  action?: AccountWorkspaceAction;
}

/**
 * Props for the AccountWorkspace component.
 */
interface AccountWorkspaceProps {
  readonly searchParams: Promise<AccountWorkspaceSearchParams>;
}

/**
 * Displays the account workspace with list-backed inline actions.
 */
const AccountWorkspace = async function ({
  searchParams,
}: AccountWorkspaceProps): Promise<JSX.Element> {
  const { search, sort, page, selectedAccountId, action } = await searchParams;
  const apiClient = getApiClient();

  const openAccountingPeriodsPromise = apiClient.GET(
    "/accounting-periods/open",
  );
  const accountsPromise = apiClient.GET("/accounts", {
    params: {
      query: {
        Search: search ?? "",
        Sort: sort ?? null,
        Limit: rowsPerPage,
        Offset: ((page ?? 1) - 1) * rowsPerPage,
      },
    },
  });

  const [{ data: openAccountingPeriods }, { data: accounts }] =
    await Promise.all([openAccountingPeriodsPromise, accountsPromise]);

  if (typeof openAccountingPeriods === "undefined") {
    throw new Error("Failed to fetch open accounting periods");
  }
  if (typeof accounts === "undefined") {
    throw new Error("Failed to fetch accounts");
  }

  const selectedAccount = accounts.items.find(
          (account) => account.id === selectedAccountId,
        ) ?? null;

  if (
    typeof selectedAccountId === "string" &&
    selectedAccount === null
  ) {
    const { ...remainingSearchParams } = resolvedSearchParams;
    const nextSearchParams =
      remainingSearchParams.action === "update" ||
      remainingSearchParams.action === "delete"
        ? (({ ...searchParamsWithoutAction }): AccountWorkspaceSearchParams =>
            searchParamsWithoutAction)(remainingSearchParams)
        : remainingSearchParams;
    redirect(routes.workspace(nextSearchParams));
  }

  const isInOnboardingMode = openAccountingPeriods.length === 0;
  const redirectSearchParams = {
    ...(typeof resolvedSearchParams.search === "string"
      ? { search: resolvedSearchParams.search }
      : {}),
    ...(typeof resolvedSearchParams.sort === "string"
      ? { sort: resolvedSearchParams.sort }
      : {}),
    ...(currentPage > 1 ? { page: currentPage } : {}),
  };
  const updateRedirectUrl =
    selectedAccount === null
      ? routes.workspace(redirectSearchParams)
      : routes.workspace({
          ...redirectSearchParams,
          selectedAccountId: selectedAccount.id,
        });

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <AccountWorkspaceFilter />
      </Stack>
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns:
            "repeat(auto-fit, minmax(min(100%, 600px), 1fr))",
        }}
      >
        <AccountWorkspaceListFrame
          data={accounts.items}
          totalCount={accounts.totalCount}
          selectedAccountId={selectedAccount?.id ?? null}
          isInOnboardingMode={isInOnboardingMode}
        />
        <AccountWorkspaceActions
          accountingPeriods={openAccountingPeriods}
          isInOnboardingMode={isInOnboardingMode}
          selectedAccount={selectedAccount}
          requestedAction={resolvedSearchParams.action ?? null}
          createRedirectUrl={routes.workspace(redirectSearchParams)}
          onboardRedirectUrl={routes.workspace(redirectSearchParams)}
          updateRedirectUrl={updateRedirectUrl}
          deleteRedirectUrl={routes.workspace(redirectSearchParams)}
        />
      </Box>
    </Stack>
  );
};

export type { AccountWorkspaceAction, AccountWorkspaceSearchParams };
export default AccountWorkspace;
