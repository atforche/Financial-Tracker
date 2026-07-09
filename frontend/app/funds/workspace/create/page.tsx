import CreateFundForm from "@/funds/workspace/CreateFundForm";
import FundWorkspacePageHeader from "@/funds/workspace/FundWorkspacePageHeader";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/FundWorkspace";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import getApiClient from "@/framework/data/getApiClient";
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

  const [{ data: accountingPeriods }, { data: openAccountingPeriods }] =
    await Promise.all([
      apiClient.GET("/accounting-periods", {
        params: {
          query: {
            Limit: 1,
          },
        },
      }),
      apiClient.GET("/accounting-periods/open"),
    ]);

  if (typeof accountingPeriods === "undefined") {
    throw new Error("Failed to fetch accounting periods");
  }
  if (typeof openAccountingPeriods === "undefined") {
    throw new Error("Failed to fetch open accounting periods");
  }
  if (accountingPeriods.items.length === 0) {
    redirect(workspaceUrl);
  }

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
