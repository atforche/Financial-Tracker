import FundWorkspacePageHeader from "@/funds/workspace/FundWorkspacePageHeader";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/types";
import type { JSX } from "react";
import OnboardFundForm from "@/funds/workspace/OnboardFundForm";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import { isUnassignedFund } from "@/funds/helpers";
import { redirect } from "next/navigation";
import routes from "@/funds/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the FundWorkspaceOnboardPage component.
 */
interface FundWorkspaceOnboardPageProps {
  readonly searchParams: Promise<FundWorkspaceSearchParams>;
}

/**
 * Page for onboarding the first fund before any accounting periods exist.
 */
const FundWorkspaceOnboardPage = async function ({
  searchParams,
}: FundWorkspaceOnboardPageProps): Promise<JSX.Element> {
  const resolvedSearchParams = await searchParams;
  const { search } = resolvedSearchParams;
  const workspaceSearchParams: FundWorkspaceSearchParams = {
    ...(typeof search !== "undefined" ? { search } : {}),
  };
  const workspaceUrl = routes.workspace(workspaceSearchParams);
  const apiClient = await createApiClient();
  const [accountingPeriodsResponse, fundsResponse] = await Promise.all([
    apiClient.GET("/accounting-periods", {
      params: {
        query: {
          Limit: 1,
        },
      },
    }),
    apiClient.GET("/funds/with-balances"),
  ]);

  const accountingPeriods = unwrapApiResponse(
    accountingPeriodsResponse,
    "Failed to fetch accounting periods",
  );
  const funds = unwrapApiResponse(fundsResponse, "Failed to fetch funds");
  if (accountingPeriods.items.length > 0) {
    redirect(workspaceUrl);
  }

  const unassignedBalance =
    funds.items.find((fund) => isUnassignedFund(fund.name))?.currentBalance
      .postedBalance ?? null;

  return (
    <PageLayout>
      <FundWorkspacePageHeader backHref={workspaceUrl} title="Onboard Fund" />
      <OnboardFundForm
        redirectUrl={workspaceUrl}
        unassignedBalance={unassignedBalance}
      />
    </PageLayout>
  );
};

export default FundWorkspaceOnboardPage;
