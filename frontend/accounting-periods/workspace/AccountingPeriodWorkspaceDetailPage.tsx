import { Button, Stack, Typography } from "@mui/material";
import AccountingPeriodDetailActions from "@/accounting-periods/workspace/AccountingPeriodDetailActions";
import AccountingPeriodSummaryFrame from "@/accounting-periods/workspace/AccountingPeriodSummaryFrame";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import ArrowBack from "@mui/icons-material/ArrowBack";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import ExpectedIncomeSourcesFrame from "@/accounting-periods/workspace/ExpectedIncomeSourcesFrame";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import FundGoalsFrame from "@/accounting-periods/workspace/FundGoalsFrame";
import IncomeSpendingCard from "@/transactions/IncomeSpendingCard";
import type { JSX } from "react";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import { formatCurrency } from "@/framework/currencyHelpers";
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
      params: { path: { accountingPeriodId }, query: { Limit: 12, Offset: 0 } },
    }),
    apiClient.GET("/fund-goals", {
      params: {
        query: {
          "Filter.AccountingPeriodIds": [accountingPeriodId],
          Limit: 500,
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
  const currentUrl = routes.workspaceDetail(period.id, workspaceParams);
  const addTransactionHref = transactionRoutes.workspaceCreate({
    accountingPeriodIds: [period.id],
    returnUrl: currentUrl,
  });

  return (
    <PageLayout>
      <ConstrainedContent maxWidth={1200}>
        <Stack spacing={2.5}>
          <Link
            href={workspaceUrl}
            style={{ alignSelf: "flex-start", textDecoration: "none" }}
          >
            <Button component="span" startIcon={<ArrowBack />}>
              Back to Workspace
            </Button>
          </Link>
          <Stack spacing={0.5}>
            <Typography variant="h4">{period.name}</Typography>
            <Typography color="text.secondary">
              Accounting period details and activity
            </Typography>
          </Stack>
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
          <ExpectedIncomeSourcesFrame
            accountingPeriod={period}
            redirectUrl={currentUrl}
          />
          <FundGoalsFrame goals={goalsWithProgress} />
          <Typography variant="h6">Recent Transactions</Typography>
          <Stack spacing={0.75}>
            {transactionSnapshot.transactions.items.length ? (
              transactionSnapshot.transactions.items.map((transaction) => (
                <Stack
                  key={transaction.id}
                  direction="row"
                  justifyContent="space-between"
                  spacing={2}
                >
                  <Typography>{transaction.description}</Typography>
                  <Typography color="text.secondary">
                    {formatCurrency(transaction.amount)}
                  </Typography>
                </Stack>
              ))
            ) : (
              <Typography color="text.secondary">
                No transactions have been recorded for this period.
              </Typography>
            )}
          </Stack>
          <Link
            href={addTransactionHref}
            style={{ alignSelf: "flex-start", textDecoration: "none" }}
          >
            <Button component="span" variant="outlined">
              Add Transaction
            </Button>
          </Link>
        </Stack>
      </ConstrainedContent>
    </PageLayout>
  );
};

export default AccountingPeriodWorkspaceDetailPage;
