import {
  getPageOffset,
  normalizePageValue,
  rowsPerPage,
} from "@/framework/listframe/page";
import {
  isNotNullOrUndefined,
  isNullOrUndefined,
} from "@/framework/nullHelpers";
import GoalWorkspacePageHeader from "@/goals/workspace/GoalWorkspacePageHeader";
import type { GoalWorkspaceSearchParams } from "@/goals/workspace/GoalWorkspace";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ViewGoalForm from "@/goals/workspace/ViewGoalForm";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import routes from "@/goals/routes";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the GoalWorkspaceDetailPage component.
 */
interface GoalWorkspaceDetailPageProps {
  readonly params: Promise<{ fundId: string }>;
  readonly searchParams: Promise<GoalWorkspaceSearchParams>;
}

/**
 * Displays details and recent balance events for a Fund's paired Goals.
 */
const GoalWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: GoalWorkspaceDetailPageProps): Promise<JSX.Element> {
  const { fundId } = await params;
  const { accountingPeriodId, fundIds, search, balanceEventPage } =
    await searchParams;
  const selectedFundIds = toRepeatedSearchParams(fundIds);
  const apiClient = createApiClient();
  const periods = unwrapApiResponse(
    await apiClient.GET("/accounting-periods", {
      params: { query: { Limit: 1 } },
    }),
    "Failed to fetch accounting periods",
  );
  const periodId = accountingPeriodId ?? periods.items[0]?.id;
  const workspaceUrl = routes.workspace({
    ...(isNotNullOrUndefined(periodId) ? { accountingPeriodId: periodId } : {}),
    ...(selectedFundIds.length ? { fundIds: selectedFundIds } : {}),
    ...(isNotNullOrUndefined(search) ? { search } : {}),
  });
  if (isNullOrUndefined(periodId)) {
    redirect(workspaceUrl);
  }
  const planResponse = await apiClient.GET("/fund-plans/fund/{fundId}", {
    params: { path: { fundId }, query: { accountingPeriodId: periodId } },
  });
  if (planResponse.response.status === 404) {
    redirect(workspaceUrl);
  }
  const fundPlan = unwrapApiResponse(
    planResponse,
    "Failed to fetch the fund plan",
  );
  const events = unwrapApiResponse(
    await apiClient.GET("/balance-events/fund-plans/accounting-period-range", {
      params: {
        query: {
          "Range.Start": periodId,
          "Range.End": periodId,
          "Filter.FundIds": [fundId],
          "Filter.AccountingPeriodIds": [periodId],
          Limit: rowsPerPage,
          Offset: getPageOffset(normalizePageValue(balanceEventPage)),
        },
      },
    }),
    "Failed to fetch goal balance events",
  );
  const currentUrl = routes.workspaceDetail(fundId, {
    accountingPeriodId: periodId,
    ...(selectedFundIds.length ? { fundIds: selectedFundIds } : {}),
    ...(isNotNullOrUndefined(search) ? { search } : {}),
    ...(isNotNullOrUndefined(balanceEventPage) ? { balanceEventPage } : {}),
  });
  return (
    <PageLayout>
      <GoalWorkspacePageHeader backHref={workspaceUrl} title="Goal Details" />
      <ViewGoalForm
        fundPlan={fundPlan}
        redirectUrl={currentUrl}
        recentBalanceEvents={events.items}
        recentBalanceEventCount={events.totalCount}
        addTransactionHref={transactionRoutes.workspaceCreate({
          accountingPeriodIds: [periodId],
          fundIds: [fundId],
          returnUrl: currentUrl,
        })}
        accountingPeriodId={periodId}
        fundId={fundId}
      />
    </PageLayout>
  );
};
export default GoalWorkspaceDetailPage;
