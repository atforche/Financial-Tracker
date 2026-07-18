import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/accountTypeFilterHelpers";
import type { AccountType } from "@/accounts/types";
import AccountWorkspaceActions from "@/accounts/workspace/AccountWorkspaceActions";
import AccountWorkspaceCards from "@/accounts/workspace/AccountWorkspaceCards";
import AccountWorkspaceFilter from "@/accounts/workspace/AccountWorkspaceFilter";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import getApiClient from "@/framework/data/getApiClient";
import getApiData from "@/framework/data/apiResponse";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";

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
    toRepeatedSearchParams(accountType),
  );

  const anyAccountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Limit: 1,
      },
    },
  });
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: { query: { Limit: 500 } },
  });
  const accountsPromise = apiClient.GET("/accounts/with-balances", {
    params: {
      query: {
        "Filter.NameSearch": search ?? "",
      },
    },
  });

  const [
    accountingPeriodResponse,
    accountingPeriodsResponse,
    accountsResponse,
  ] = await Promise.all([
    anyAccountingPeriodsPromise,
    accountingPeriodsPromise,
    accountsPromise,
  ]);

  const accountingPeriod = getApiData(
    accountingPeriodResponse,
    "Failed to fetch accounting periods",
  );
  const accountingPeriods = getApiData(
    accountingPeriodsResponse,
    "Failed to fetch accounting periods",
  );
  const accounts = getApiData(accountsResponse, "Failed to fetch accounts");

  const filteredAccounts = shouldPersistAccountTypes(currentAccountTypes)
    ? accounts.items.filter((account) =>
        currentAccountTypes.includes(account.type),
      )
    : accounts.items;
  const isInOnboardingMode = accountingPeriod.items.length === 0;
  const openAccountingPeriods = accountingPeriods.items.filter(
    (period) => period.isOpen,
  );

  return (
    <PageLayout>
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
    </PageLayout>
  );
};

export type { AccountWorkspaceAction, AccountWorkspaceSearchParams };
export default AccountWorkspace;
