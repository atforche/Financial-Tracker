import { Box, Button, Stack, Typography } from "@mui/material";
import AccountingPeriodDetailActions from "@/accounting-periods/workspace/AccountingPeriodDetailActions";
import AccountingPeriodSummaryFrame from "@/accounting-periods/workspace/AccountingPeriodSummaryFrame";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import ActualIncomeCard from "@/transactions/ActualIncomeCard";
import ArrowBack from "@mui/icons-material/ArrowBack";
import ExpectedFundGoalContributionsActualCard from "@/accounting-periods/workspace/ExpectedFundGoalContributionsActualCard";
import ExpectedIncomeActualCard from "@/accounting-periods/workspace/ExpectedIncomeActualCard";
import ExpectedIncomeFundGoalContributionsCard from "@/accounting-periods/workspace/ExpectedIncomeFundGoalContributionsCard";
import ExpectedIncomeSourcesFrame from "@/accounting-periods/workspace/ExpectedIncomeSourcesFrame";
import IncomeSpendingCard from "@/transactions/IncomeSpendingCard";
import type { JSX } from "react";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import ResponsivePageSize from "@/framework/listframe/ResponsivePageSize";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the AccountingPeriodWorkspaceDetailPage component.
 */
interface AccountingPeriodWorkspaceDetailPageProps {
  readonly params: Promise<{ accountingPeriodId: string }>;
  readonly searchParams: Promise<AccountingPeriodWorkspaceSearchParams>;
}

/**
 * Displays the detailed plan, results, activity, and actions for one accounting period.
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
  const [periodResponse, transactionsResponse] = await Promise.all([
    apiClient.GET("/accounting-periods/{accountingPeriodId}", {
      params: { path: { accountingPeriodId } },
    }),
    apiClient.GET("/accounting-periods/{accountingPeriodId}/transactions", {
      params: {
        path: { accountingPeriodId },
        query: { Limit: 1 },
      },
    }),
  ]);
  if (periodResponse.error) {
    redirect(workspaceUrl);
  }
  const period = unwrapApiResponse(
    periodResponse,
    "Failed to fetch accounting period",
  );
  const transactionSnapshot = unwrapApiResponse(
    transactionsResponse,
    "Failed to fetch accounting period transactions",
  );
  const currentUrl = routes.workspaceDetail(period.id, workspaceParams);
  const addTransactionHref = transactionRoutes.workspaceCreate({
    accountingPeriodIds: [period.id],
    returnUrl: currentUrl,
  });

  return (
    <PageLayout>
      <ResponsivePageSize desktopBreakpoint="lg" />
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
                addTransactionHref={addTransactionHref}
                redirectUrl={currentUrl}
                deleteRedirectUrl={workspaceUrl}
              />
            }
          />
        </Stack>
      </Box>
      <ResponsiveGrid columns={{ xs: 1, lg: 2 }} spacing={3}>
        <IncomeSpendingCard
          totalIncome={transactionSnapshot.totalIncome}
          totalSpending={transactionSnapshot.totalSpending}
        />
        <ExpectedFundGoalContributionsActualCard
          expectedFundGoalContributions={period.expectedGoalContributions}
          actualFundGoalContributions={period.actualGoalContributions}
        />
        <ExpectedIncomeFundGoalContributionsCard
          expectedIncome={period.expectedIncome}
          plannedFundGoalContributions={period.plannedGoalContributions}
          expectedFundGoalContributions={period.expectedGoalContributions}
        />
        <ExpectedIncomeActualCard
          expectedIncome={period.expectedIncome}
          actualIncome={period.actualIncome}
        />
        <ActualIncomeCard totalIncome={transactionSnapshot.totalIncome} />
      </ResponsiveGrid>
      <ExpectedIncomeSourcesFrame
        accountingPeriod={period}
        redirectUrl={currentUrl}
      />
    </PageLayout>
  );
};

export default AccountingPeriodWorkspaceDetailPage;
