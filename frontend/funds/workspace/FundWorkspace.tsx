import FundWorkspaceCards from "@/funds/workspace/FundWorkspaceCards";
import FundWorkspaceFilter from "@/funds/workspace/FundWorkspaceFilter";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/types";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import { isUnassignedFund } from "@/funds/helpers";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

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
  const apiClient = await createApiClient();
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

  const accountingPeriod = unwrapApiResponse(
    accountingPeriodResponse,
    "Failed to fetch accounting periods",
  );
  const funds = unwrapApiResponse(fundsResponse, "Failed to fetch funds");

  const visibleFunds = funds.items.filter(
    (fund) => !isUnassignedFund(fund.name),
  );
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

export default FundWorkspace;
