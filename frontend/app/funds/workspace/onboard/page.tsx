import FundWorkspacePageHeader from "@/funds/workspace/FundWorkspacePageHeader";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/FundWorkspace";
import type { JSX } from "react";
import OnboardFundForm from "@/funds/workspace/OnboardFundForm";
import { Stack } from "@mui/material";
import getApiClient from "@/framework/data/getApiClient";
import getApiData from "@/framework/data/apiResponse";
import { redirect } from "next/navigation";
import routes from "@/funds/routes";

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
  const apiClient = getApiClient();
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

  const accountingPeriods = getApiData(accountingPeriodsResponse, "Failed to fetch accounting periods");
  const funds = getApiData(fundsResponse, "Failed to fetch funds");
  if (accountingPeriods.items.length > 0) {
    redirect(workspaceUrl);
  }

  const unassignedBalance =
    funds.items.find((fund) => fund.name === "Unassigned")?.currentBalance
      .postedBalance ?? null;

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <FundWorkspacePageHeader backHref={workspaceUrl} title="Onboard Fund" />
      <OnboardFundForm
        redirectUrl={workspaceUrl}
        unassignedBalance={unassignedBalance}
      />
    </Stack>
  );
};

export const dynamic = "force-dynamic";
export default FundWorkspaceOnboardPage;
