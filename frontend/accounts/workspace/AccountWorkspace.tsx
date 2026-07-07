import { Box, Stack } from "@mui/material";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import type { AccountSortOrder } from "@/accounts/types";
import AccountWorkspaceActions from "@/accounts/workspace/AccountWorkspaceActions";
import AccountWorkspaceFilter from "@/accounts/workspace/AccountWorkspaceFilter";
import AccountWorkspaceListFrame from "@/accounts/workspace/AccountWorkspaceListFrame";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounts/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

type AccountWorkspaceAction =
  "create" | "onboard" | "view" | "update" | "delete";

/**
 * Search parameters supported by the Accounts workspace.
 */
interface AccountWorkspaceSearchParams {
  search?: string;
  sort?: AccountSortOrder;
  page?: number | string | null;
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
  const currentPage = normalizePageValue(page);

  const anyAccountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Limit: 1,
      },
    },
  });
  const openAccountingPeriodsPromise = apiClient.GET(
    "/accounting-periods/open",
  );
  const accountsPromise = apiClient.GET("/accounts", {
    params: {
      query: {
        Search: search ?? "",
        Sort: sort ?? null,
        Limit: rowsPerPage,
        Offset: getPageOffset(currentPage),
      },
    },
  });

  const [
    { data: accountingPeriod },
    { data: openAccountingPeriods },
    { data: accounts },
  ] = await Promise.all([
    anyAccountingPeriodsPromise,
    openAccountingPeriodsPromise,
    accountsPromise,
  ]);

  if (typeof accountingPeriod === "undefined") {
    throw new Error("Failed to fetch accounting periods");
  }
  if (typeof openAccountingPeriods === "undefined") {
    throw new Error("Failed to fetch open accounting periods");
  }
  if (typeof accounts === "undefined") {
    throw new Error("Failed to fetch accounts");
  }

  const selectedAccount =
    accounts.items.find((account) => account.id === selectedAccountId) ?? null;
  const isInOnboardingMode = accountingPeriod.items.length === 0;

  if (typeof selectedAccountId === "string" && selectedAccount === null) {
    redirect(
      routes.workspace({
        search: search ?? "",
        ...(typeof sort !== "undefined" ? { sort } : {}),
        ...(typeof page !== "undefined" ? { page: currentPage } : {}),
        ...(typeof action !== "undefined" ? { action } : {}),
      }),
    );
  }

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
          requestedAction={action ?? null}
        />
      </Box>
    </Stack>
  );
};

export type { AccountWorkspaceAction, AccountWorkspaceSearchParams };
export default AccountWorkspace;
