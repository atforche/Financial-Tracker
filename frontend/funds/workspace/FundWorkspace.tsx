import FundWorkspaceCards from "@/funds/workspace/FundWorkspaceCards";
import FundWorkspaceFilter from "@/funds/workspace/FundWorkspaceFilter";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import getApiClient from "@/framework/data/getApiClient";
import getApiData from "@/framework/data/apiResponse";

/**
 * Search parameters supported by the Funds workspace.
 */
interface FundWorkspaceSearchParams {
  search?: string;
  balanceEventPage?: number | string | null;
}

/**
 * Props for the FundWorkspace component.
 */
interface FundWorkspaceProps {
  readonly searchParams: Promise<FundWorkspaceSearchParams>;
}

/**
 * Displays the fund workspace with card-backed navigation.
 */
const FundWorkspace = async function ({
  searchParams,
}: FundWorkspaceProps): Promise<JSX.Element> {
  const { search } = await searchParams;
  const apiClient = getApiClient();
  const anyAccountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: { query: { Limit: 1 } },
  });
  const fundsPromise = apiClient.GET("/funds/with-balances", {
    params: { query: { "Filter.NameSearch": search ?? "" } },
  });
  const [accountingPeriodResponse, fundsResponse] = await Promise.all([
    anyAccountingPeriodsPromise,
    fundsPromise,
  ]);

  const accountingPeriod = getApiData(accountingPeriodResponse, "Failed to fetch accounting periods");
  const funds = getApiData(fundsResponse, "Failed to fetch funds");

  const visibleFunds = funds.items.filter((fund) => fund.name !== "Unassigned");
  const isInOnboardingMode = accountingPeriod.items.length === 0;

  return (
    <PageLayout>
      <FundWorkspaceFilter isInOnboardingMode={isInOnboardingMode} />
      <FundWorkspaceCards
        data={visibleFunds}
        isInOnboardingMode={isInOnboardingMode}
      />
    </PageLayout>
  );
};

export type { FundWorkspaceSearchParams };
export default FundWorkspace;
