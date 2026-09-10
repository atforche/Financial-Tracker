import { Box, Button, Stack, Typography } from "@mui/material";
import AccountingPeriodDetailActions from "@/accounting-periods/workspace/AccountingPeriodDetailActions";
import AccountingPeriodProgressView from "@/accounting-periods/workspace/AccountingPeriodProgressView";
import AccountingPeriodSummaryFrame from "@/accounting-periods/workspace/AccountingPeriodSummaryFrame";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import ArrowBack from "@mui/icons-material/ArrowBack";
import type { JSX } from "react";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the AccountingPeriodWorkspaceDetailPage component.
 */
interface AccountingPeriodWorkspaceDetailPageProps {
  readonly params: Promise<{ accountingPeriodId: string }>;
  readonly searchParams: Promise<AccountingPeriodWorkspaceSearchParams>;
}

/**
 * Displays the detailed plan, progress, activity, and actions for one accounting period.
 */
const AccountingPeriodWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: AccountingPeriodWorkspaceDetailPageProps): Promise<JSX.Element> {
  const { accountingPeriodId } = await params;
  const resolvedSearchParams = await searchParams;
  const apiClient = await createApiClient();
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
  if (resolvedSearchParams.view === "plan") {
    redirect(
      routes.workspacePlan(period.id, {
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
      }),
    );
  }
  const currentUrl = routes.workspaceDetail(period.id, workspaceParams);
  const planHref = routes.workspacePlan(period.id, {
    ...workspaceParams,
  });
  return (
    <PageLayout>
      <Box sx={{ maxWidth: 1200, width: "100%" }}>
        <Stack spacing={2.5}>
          <Link
            href={workspaceUrl}
            style={{ alignSelf: "flex-start", textDecoration: "none" }}
          >
            <Button component="span" startIcon={<ArrowBack />}>
              Back to Workspace
            </Button>
          </Link>
          <Typography variant="h4">{period.name}</Typography>
          <AccountingPeriodSummaryFrame
            accountingPeriod={period}
            headerContent={
              <AccountingPeriodDetailActions
                accountingPeriod={period}
                planHref={planHref}
                redirectUrl={currentUrl}
                deleteRedirectUrl={workspaceUrl}
              />
            }
          />
        </Stack>
      </Box>
      <AccountingPeriodProgressView accountingPeriod={period} />
    </PageLayout>
  );
};

export default AccountingPeriodWorkspaceDetailPage;
