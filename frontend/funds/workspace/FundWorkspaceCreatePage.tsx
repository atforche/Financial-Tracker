import CreateFundForm from "@/funds/workspace/CreateFundForm";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/types";
import type { JSX } from "react";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import routes from "@/funds/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the FundWorkspaceCreatePage component.
 */
interface FundWorkspaceCreatePageProps {
  readonly searchParams: Promise<FundWorkspaceSearchParams>;
}

/**
 * Page for creating a fund within the workspace.
 */
const FundWorkspaceCreatePage = async function ({
  searchParams,
}: FundWorkspaceCreatePageProps): Promise<JSX.Element> {
  const resolvedSearchParams = await searchParams;
  const { search } = resolvedSearchParams;
  const workspaceSearchParams: FundWorkspaceSearchParams = {
    ...(typeof search !== "undefined" ? { search } : {}),
  };
  const workspaceUrl = routes.workspace(workspaceSearchParams);
  const apiClient = await createApiClient();

  const accountingPeriodsResponse = await apiClient.GET(
    "/accounting-periods/with-balances",
    {
      params: { query: { "Filter.IsOpen": true } },
    },
  );

  const openAccountingPeriods = unwrapApiResponse(
    accountingPeriodsResponse,
    "Failed to fetch accounting periods",
  ).items;
  // Only without an open period can there be no periods at all, so only then is the extra request needed
  if (
    openAccountingPeriods.length === 0 &&
    unwrapApiResponse(
      await apiClient.GET("/accounting-periods/bounds"),
      "Failed to fetch the accounting period bounds",
    ).latest === null
  ) {
    redirect(workspaceUrl);
  }

  return (
    <CreateFundForm
      accountingPeriods={openAccountingPeriods}
      redirectUrl={workspaceUrl}
    />
  );
};

export default FundWorkspaceCreatePage;
