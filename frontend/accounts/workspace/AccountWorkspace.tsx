import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/trends/accountTypeFilter";
import type { AccountType } from "@/accounts/types";
import AccountWorkspaceActions from "@/accounts/workspace/AccountWorkspaceActions";
import AccountWorkspaceCards from "@/accounts/workspace/AccountWorkspaceCards";
import AccountWorkspaceFilter from "@/accounts/workspace/AccountWorkspaceFilter";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import getApiClient from "@/framework/data/getApiClient";
import { toRepeatedSearchParam } from "@/framework/routes/helpers";

type AccountWorkspaceAction = "create" | "onboard";

/**
 * Search parameters supported by the Accounts workspace.
 */
interface AccountWorkspaceSearchParams {
  search?: string;
  accountType?: AccountType | readonly AccountType[];
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
 * Displays the account workspace with card-backed navigation and actions.
 */
const AccountWorkspace = async function ({
  searchParams,
}: AccountWorkspaceProps): Promise<JSX.Element> {
  const { search, accountType, action } = await searchParams;
  const apiClient = getApiClient();
  const currentAccountTypes = normalizeAccountTypes(
    toRepeatedSearchParam(accountType),
  );

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

  const filteredAccounts = shouldPersistAccountTypes(currentAccountTypes)
    ? accounts.items.filter((account) =>
        currentAccountTypes.includes(account.type),
      )
    : accounts.items;
  const isInOnboardingMode = accountingPeriod.items.length === 0;

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <AccountWorkspaceFilter isInOnboardingMode={isInOnboardingMode} />
      <AccountWorkspaceCards
        data={filteredAccounts}
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
