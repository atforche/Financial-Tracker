import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/accountTypeFilterHelpers";
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
 * Displays the account workspace with card-backed navigation.
 */
const AccountWorkspace = async function ({
  searchParams,
}: AccountWorkspaceProps): Promise<JSX.Element> {
  const { search, accountType } = await searchParams;
  const apiClient = await createApiClient();
  const currentAccountTypes = normalizeAccountTypes(
    toRepeatedSearchParams(accountType),
  );
  const hasAccountTypeFilter = shouldPersistAccountTypes(currentAccountTypes);

  const [boundsResponse, accountsResponse] = await Promise.all([
    apiClient.GET("/accounting-periods/bounds"),
    apiClient.GET("/accounts/with-balances", {
      params: {
        query: {
          "Filter.NameSearch": search ?? "",
          ...(hasAccountTypeFilter
            ? { "Filter.Types": [...currentAccountTypes] }
            : {}),
        },
      },
    }),
  ]);
  const isInOnboardingMode =
    unwrapApiResponse(
      boundsResponse,
      "Failed to fetch the accounting period bounds",
    ).latest === null;
  const accounts = unwrapApiResponse(
    accountsResponse,
    "Failed to fetch accounts",
  ).items;

  return (
    <PageLayout>
      <AccountWorkspaceFilter isInOnboardingMode={isInOnboardingMode} />
      <AccountWorkspaceCards
        data={accounts}
        filters={{
          ...(search === undefined ? {} : { search }),
          ...(hasAccountTypeFilter ? { accountType: currentAccountTypes } : {}),
        }}
        hasActiveFilters={(search ?? "").trim() !== "" || hasAccountTypeFilter}
        isInOnboardingMode={isInOnboardingMode}
      />
    </PageLayout>
  );
};

export default AccountWorkspace;
