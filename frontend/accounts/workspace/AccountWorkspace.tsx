import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import type { AccountSortOrder } from "@/accounts/types";
import AccountWorkspaceActions from "@/accounts/workspace/AccountWorkspaceActions";
import AccountWorkspaceFilter from "@/accounts/workspace/AccountWorkspaceFilter";
import AccountWorkspaceListFrame from "@/accounts/workspace/AccountWorkspaceListFrame";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import getApiClient from "@/framework/data/getApiClient";
import { rowsPerPage } from "@/framework/listframe/Constants";

type AccountWorkspaceAction = "create" | "onboard";

/**
 * Search parameters supported by the Accounts workspace.
 */
interface AccountWorkspaceSearchParams {
  search?: string;
  sort?: AccountSortOrder;
  page?: number | string | null;
  action?: AccountWorkspaceAction;
  balanceEventPage?: number | string | null;
}

/**
 * Props for the AccountWorkspace component.
 */
interface AccountWorkspaceProps {
  readonly searchParams: Promise<AccountWorkspaceSearchParams>;
}

/**
 * Displays the account workspace with list-backed actions.
 */
const AccountWorkspace = async function ({
  searchParams,
}: AccountWorkspaceProps): Promise<JSX.Element> {
  const { search, sort, page, action } = await searchParams;
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

  const isInOnboardingMode = accountingPeriod.items.length === 0;

  return (
    <Stack spacing={3} sx={{ width: "100%", maxWidth: 1440 }}>
      <AccountWorkspaceFilter />
      <AccountWorkspaceListFrame
        data={accounts.items}
        totalCount={accounts.totalCount}
        isInOnboardingMode={isInOnboardingMode}
      />
      <AccountWorkspaceActions
        accountingPeriods={openAccountingPeriods}
        isInOnboardingMode={isInOnboardingMode}
        requestedAction={action ?? null}
      />
    </Stack>
  );
};

export type { AccountWorkspaceAction, AccountWorkspaceSearchParams };
export default AccountWorkspace;
