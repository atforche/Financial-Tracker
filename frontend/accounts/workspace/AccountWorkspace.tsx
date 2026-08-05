import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/accountTypeFilterHelpers";
import AccountWorkspaceActions from "@/accounts/workspace/AccountWorkspaceActions";
import AccountWorkspaceCards from "@/accounts/workspace/AccountWorkspaceCards";
import AccountWorkspaceFilter from "@/accounts/workspace/AccountWorkspaceFilter";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/types";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

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
  const apiClient = await createApiClient();
  const currentAccountTypes = normalizeAccountTypes(
    toRepeatedSearchParams(accountType),
  );

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
  const financialInstitutionsPromise = apiClient.GET(
    "/accounts/financial-institutions",
  );

  const [
    accountingPeriodsResponse,
    accountsResponse,
    financialInstitutionsResponse,
  ] = await Promise.all([
    accountingPeriodsPromise,
    accountsPromise,
    financialInstitutionsPromise,
  ]);
  const accountingPeriods = unwrapApiResponse(
    accountingPeriodsResponse,
    "Failed to fetch accounting periods",
  );
  const accounts = unwrapApiResponse(
    accountsResponse,
    "Failed to fetch accounts",
  );
  const financialInstitutions = unwrapApiResponse(
    financialInstitutionsResponse,
    "Failed to fetch financial institutions",
  );

  const filteredAccounts = shouldPersistAccountTypes(currentAccountTypes)
    ? accounts.items.filter((account) =>
        currentAccountTypes.includes(account.type),
      )
    : accounts.items;
  const isInOnboardingMode = accountingPeriods.items.length === 0;
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
        financialInstitutions={financialInstitutions.items}
        isInOnboardingMode={isInOnboardingMode}
        requestedAction={action ?? null}
      />
    </PageLayout>
  );
};

export default AccountWorkspace;
