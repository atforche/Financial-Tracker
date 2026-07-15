import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import GoalWorkspacePageHeader from "@/goals/workspace/GoalWorkspacePageHeader";
import type { GoalWorkspaceSearchParams } from "@/goals/workspace/GoalWorkspace";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import ViewGoalForm from "@/goals/workspace/ViewGoalForm";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/goals/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";
import { toRepeatedSearchParam } from "@/framework/routes/helpers";
import transactionRoutes from "@/transactions/routes";

/**
 * Props for the GoalWorkspaceDetailPage component.
 */
interface GoalWorkspaceDetailPageProps {
  readonly params: Promise<{ fundId: string }>;
  readonly searchParams: Promise<GoalWorkspaceSearchParams>;
}

/** Displays details and recent balance events for a Fund's paired Goals. */
const GoalWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: GoalWorkspaceDetailPageProps): Promise<JSX.Element> {
  const { fundId } = await params;
  const { accountingPeriodId, fundIds, search, balanceEventPage } =
    await searchParams;
  const selectedFundIds = toRepeatedSearchParam(fundIds);
  const apiClient = getApiClient();
  const { data: periods } = await apiClient.GET("/accounting-periods", {
    params: { query: { Limit: 1 } },
  });
  const periodId = accountingPeriodId ?? periods?.items[0]?.id;
  const workspaceUrl = routes.workspace({
    ...(typeof periodId === "string" ? { accountingPeriodId: periodId } : {}),
    ...(selectedFundIds.length > 0 ? { fundIds: selectedFundIds } : {}),
    ...(typeof search === "string" ? { search } : {}),
  });
  if (typeof periodId === "undefined") {
    redirect(workspaceUrl);
  }

  const [{ data: assignmentGoal }, { data: spendingGoal }] = await Promise.all([
    apiClient.GET("/goals/assignment", {
      params: {
        query: {
          "Filter.FundIds": [fundId],
          "Filter.AccountingPeriodIds": [periodId],
          Limit: 1,
        },
      },
    }),
    apiClient.GET("/goals/spending", {
      params: {
        query: {
          "Filter.FundIds": [fundId],
          "Filter.AccountingPeriodIds": [periodId],
          Limit: 1,
        },
      },
    }),
  ]);
  if (
    typeof assignmentGoal?.items[0] === "undefined" ||
    typeof spendingGoal?.items[0] === "undefined"
  ) {
    redirect(workspaceUrl);
  }

  const currentBalanceEventPage = normalizePageValue(balanceEventPage);
  const { data: balanceEvents } = await apiClient.GET(
    "/balance-events/goals/accounting-period-range",
    {
      params: {
        query: {
          "Range.Start": periodId,
          "Range.End": periodId,
          "Filter.FundIds": [fundId],
          "Filter.AccountingPeriodIds": [periodId],
          Limit: rowsPerPage,
          Offset: getPageOffset(currentBalanceEventPage),
        },
      },
    },
  );

  const currentUrl = routes.workspaceDetail(fundId, {
    accountingPeriodId: periodId,
    ...(selectedFundIds.length > 0 ? { fundIds: selectedFundIds } : {}),
    ...(typeof search === "string" ? { search } : {}),
    ...(typeof balanceEventPage === "undefined" ? {} : { balanceEventPage }),
  });
  const addTransactionHref = transactionRoutes.workspaceCreate({
    accountingPeriodIds: [periodId],
    fundIds: [fundId],
    returnUrl: currentUrl,
  });

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <GoalWorkspacePageHeader backHref={workspaceUrl} title="Goal Details" />
      <ViewGoalForm
        assignmentGoal={assignmentGoal.items[0]}
        spendingGoal={spendingGoal.items[0]}
        redirectUrl={currentUrl}
        recentBalanceEvents={balanceEvents?.items ?? []}
        recentBalanceEventCount={balanceEvents?.totalCount ?? 0}
        addTransactionHref={addTransactionHref}
        accountingPeriodId={periodId}
        fundId={fundId}
      />
    </Stack>
  );
};

export const dynamic = "force-dynamic";
export default GoalWorkspaceDetailPage;
