import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import type { GoalWorkspaceBalanceEvent } from "@/goals/types";
import GoalWorkspacePageHeader from "@/goals/workspace/GoalWorkspacePageHeader";
import type { GoalWorkspaceSearchParams } from "@/goals/workspace/GoalWorkspace";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import ViewGoalForm from "@/goals/workspace/ViewGoalForm";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/goals/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";
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
  const { accountingPeriodId, fundIds, balanceEventPage } = await searchParams;
  const selectedFundIds = Array.isArray(fundIds)
    ? fundIds
    : typeof fundIds === "string"
      ? [fundIds]
      : [];
  const apiClient = getApiClient();
  const { data: periods } = await apiClient.GET("/accounting-periods", {
    params: { query: { Limit: 1 } },
  });
  const periodId = accountingPeriodId ?? periods?.items[0]?.id;
  const workspaceUrl = routes.workspace({
    ...(typeof periodId === "string" ? { accountingPeriodId: periodId } : {}),
    ...(selectedFundIds.length > 0 ? { fundIds: selectedFundIds } : {}),
  });
  if (typeof periodId === "undefined") {
    redirect(workspaceUrl);
  }

  const [{ data: assignmentGoal }, { data: spendingGoal }] = await Promise.all([
    apiClient.GET("/goals/assignment", {
      params: { query: { FundId: fundId, AccountingPeriodId: periodId } },
    }),
    apiClient.GET("/goals/spending", {
      params: { query: { FundId: fundId, AccountingPeriodId: periodId } },
    }),
  ]);
  if (
    typeof assignmentGoal === "undefined" ||
    typeof spendingGoal === "undefined"
  ) {
    redirect(workspaceUrl);
  }

  const currentBalanceEventPage = normalizePageValue(balanceEventPage);
  const { data: balanceEvents } = await apiClient.GET(
    "/goals/{fundId}/balance-events",
    {
      params: {
        path: { fundId },
        query: {
          AccountingPeriodId: periodId,
          Limit: rowsPerPage,
          Offset: getPageOffset(currentBalanceEventPage),
        },
      },
    },
  );

  const currentUrl = routes.workspaceDetail(fundId, {
    accountingPeriodId: periodId,
    ...(selectedFundIds.length > 0 ? { fundIds: selectedFundIds } : {}),
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
        assignmentGoal={assignmentGoal}
        spendingGoal={spendingGoal}
        redirectUrl={currentUrl}
        recentBalanceEvents={
          balanceEvents?.items ?? ([] as GoalWorkspaceBalanceEvent[])
        }
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
