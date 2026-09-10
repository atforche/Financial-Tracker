import { Button, Stack, Typography } from "@mui/material";
import AccountingPeriodPlanView from "@/accounting-periods/workspace/AccountingPeriodPlanView";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import ArrowBack from "@mui/icons-material/ArrowBack";
import type { JSX } from "react";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

interface AccountingPeriodWorkspacePlanPageProps {
  readonly params: Promise<{ accountingPeriodId: string }>;
  readonly searchParams: Promise<AccountingPeriodWorkspaceSearchParams>;
}

/** Displays the planning workflow for one accounting period. */
const AccountingPeriodWorkspacePlanPage = async function ({
  params,
  searchParams,
}: AccountingPeriodWorkspacePlanPageProps): Promise<JSX.Element> {
  const { accountingPeriodId } = await params;
  const resolvedSearchParams = await searchParams;
  const workspaceParams = {
    ...(typeof resolvedSearchParams.years !== "undefined"
      ? { years: resolvedSearchParams.years }
      : {}),
    ...(typeof resolvedSearchParams.months !== "undefined"
      ? { months: resolvedSearchParams.months }
      : {}),
    ...(typeof resolvedSearchParams.sort !== "undefined"
      ? { sort: resolvedSearchParams.sort }
      : {}),
    ...(typeof resolvedSearchParams.page !== "undefined"
      ? { page: resolvedSearchParams.page }
      : {}),
    ...(typeof resolvedSearchParams.pageSize !== "undefined"
      ? { pageSize: resolvedSearchParams.pageSize }
      : {}),
  } satisfies AccountingPeriodWorkspaceSearchParams;
  const workspaceUrl = routes.workspace(workspaceParams);
  const apiClient = await createApiClient();
  const periodResponse = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    { params: { path: { accountingPeriodId } } },
  );
  if (periodResponse.error) {
    redirect(workspaceUrl);
  }
  const period = unwrapApiResponse(
    periodResponse,
    "Failed to fetch accounting period",
  );
  const cashFlowHref = routes.workspaceDetail(period.id, workspaceParams);
  const currentUrl = routes.workspacePlan(period.id, {
    ...workspaceParams,
    ...(typeof resolvedSearchParams.incomeSourcePage !== "undefined"
      ? { incomeSourcePage: resolvedSearchParams.incomeSourcePage }
      : {}),
    ...(typeof resolvedSearchParams.fundGoalPage !== "undefined"
      ? { fundGoalPage: resolvedSearchParams.fundGoalPage }
      : {}),
    ...(typeof resolvedSearchParams.accountGoalPage !== "undefined"
      ? { accountGoalPage: resolvedSearchParams.accountGoalPage }
      : {}),
  });

  return (
    <PageLayout>
      <Stack spacing={2.5} sx={{ maxWidth: 1200, width: "100%" }}>
        <Link
          href={cashFlowHref}
          style={{ alignSelf: "flex-start", textDecoration: "none" }}
        >
          <Button component="span" startIcon={<ArrowBack />}>
            Back to Cash Flow
          </Button>
        </Link>
        <Typography variant="h4">Plan for {period.name}</Typography>
      </Stack>
      <AccountingPeriodPlanView
        accountingPeriod={period}
        currentUrl={currentUrl}
        pageSize={resolvedSearchParams.pageSize}
        fundGoalPage={resolvedSearchParams.fundGoalPage}
        accountGoalPage={resolvedSearchParams.accountGoalPage}
      />
    </PageLayout>
  );
};

export default AccountingPeriodWorkspacePlanPage;
