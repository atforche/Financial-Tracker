import CreateFundForm from "@/funds/workspace/CreateFundForm";
import FundWorkspacePageHeader from "@/funds/workspace/FundWorkspacePageHeader";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/FundWorkspace";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import getApiClient from "@/framework/data/getApiClient";
import getApiData from "@/framework/data/apiResponse";
import { redirect } from "next/navigation";
import routes from "@/funds/routes";

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
  const apiClient = getApiClient();

  const accountingPeriodsResponse = await apiClient.GET("/accounting-periods", {
    params: { query: { Limit: 500 } },
  });

  const accountingPeriods = getApiData(
    accountingPeriodsResponse,
    "Failed to fetch accounting periods",
  );
  if (accountingPeriods.items.length === 0) {
    redirect(workspaceUrl);
  }
  const openAccountingPeriods = accountingPeriods.items.filter(
    (period) => period.isOpen,
  );

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <FundWorkspacePageHeader backHref={workspaceUrl} title="Create Fund" />
      <CreateFundForm
        accountingPeriods={openAccountingPeriods}
        redirectUrl={workspaceUrl}
      />
    </Stack>
  );
};

export const dynamic = "force-dynamic";
export default FundWorkspaceCreatePage;
