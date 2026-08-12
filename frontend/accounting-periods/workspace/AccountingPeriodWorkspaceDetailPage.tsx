import { Box, Button, Stack, Typography } from "@mui/material";
import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import AccountingPeriodDetailActions from "@/accounting-periods/workspace/AccountingPeriodDetailActions";
import AccountingPeriodSummaryFrame from "@/accounting-periods/workspace/AccountingPeriodSummaryFrame";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import ArrowBack from "@mui/icons-material/ArrowBack";
import ExpectedIncomeSourcesFrame from "@/accounting-periods/workspace/ExpectedIncomeSourcesFrame";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import FundGoalsFrame from "@/accounting-periods/workspace/FundGoalsFrame";
import IncomeSpendingCard from "@/transactions/IncomeSpendingCard";
import type { JSX } from "react";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import RecentTransactionsFrame from "@/accounting-periods/workspace/RecentTransactionsFrame";
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
  const rowsPerPage = getRowsPerPage(resolvedSearchParams.pageSize);
  const fundGoalPage = normalizePageValue(resolvedSearchParams.fundGoalPage);
  const transactionPage = normalizePageValue(
    resolvedSearchParams.transactionPage,
  );
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
  const [
    periodResponse,
    transactionsResponse,
    goalsResponse,
    progressResponse,
  ] = await Promise.all([
    apiClient.GET("/accounting-periods/{accountingPeriodId}", {
      params: { path: { accountingPeriodId } },
    }),
    apiClient.GET("/accounting-periods/{accountingPeriodId}/transactions", {
      params: {
        path: { accountingPeriodId },
        query: {
          Limit: rowsPerPage,
          Offset: getPageOffset(transactionPage, rowsPerPage),
        },
      },
    }),
    apiClient.GET("/fund-goals", {
      params: {
        query: {
          "Filter.AccountingPeriodIds": [accountingPeriodId],
          Limit: rowsPerPage,
          Offset: getPageOffset(fundGoalPage, rowsPerPage),
        },
      },
    }),
    apiClient.GET("/fund-goals/progress/{accountingPeriodId}", {
      params: { path: { accountingPeriodId } },
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
  const goals = unwrapApiResponse(goalsResponse, "Failed to fetch fund goals");
  const progress = unwrapApiResponse(
    progressResponse,
    "Failed to fetch fund goal progress",
  );
  const progressByGoal = new Map(
    progress.map((item) => [item.fundGoalId, item.progress]),
  );
  const goalsWithProgress: FundGoalWithProgress[] = goals.items.flatMap(
    (goal) => {
      const goalProgress = progressByGoal.get(goal.id);
      return goalProgress ? [{ ...goal, progress: goalProgress }] : [];
    },
  );
  const currentUrl = routes.workspaceDetail(period.id, {
    ...workspaceParams,
    ...(typeof resolvedSearchParams.fundGoalPage !== "undefined"
      ? { fundGoalPage: resolvedSearchParams.fundGoalPage }
      : {}),
    ...(typeof resolvedSearchParams.transactionPage !== "undefined"
      ? { transactionPage: resolvedSearchParams.transactionPage }
      : {}),
  });
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
                redirectUrl={currentUrl}
                deleteRedirectUrl={workspaceUrl}
              />
            }
          />
          <IncomeSpendingCard
            totalIncome={transactionSnapshot.totalIncome}
            totalSpending={transactionSnapshot.totalSpending}
          />
        </Stack>
      </Box>
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns: { xs: "1fr", lg: "repeat(2, minmax(0, 1fr))" },
          width: "100%",
        }}
      >
        <ExpectedIncomeSourcesFrame
          accountingPeriod={period}
          routeSearchParams={resolvedSearchParams}
        />
        <FundGoalsFrame
          goals={goalsWithProgress}
          totalCount={goals.totalCount}
          accountingPeriodId={period.id}
          returnUrl={currentUrl}
        />
        <RecentTransactionsFrame
          transactions={transactionSnapshot.transactions.items}
          totalCount={transactionSnapshot.transactions.totalCount}
          returnUrl={currentUrl}
          headerContent={
            <Link href={addTransactionHref} style={{ textDecoration: "none" }}>
              <Button component="span" variant="contained">
                Add Transaction
              </Button>
            </Link>
          }
        />
      </Box>
    </PageLayout>
  );
};

export default AccountingPeriodWorkspaceDetailPage;
