import {
  getPageOffset,
  normalizePageValue,
  rowsPerPage,
} from "@/framework/listframe/page";
import {
  isNotNullOrUndefined,
  isNullOrUndefined,
} from "@/framework/nullHelpers";
import FundGoalWorkspacePageHeader from "@/fund-goals/workspace/FundGoalWorkspacePageHeader";
import type { FundGoalWorkspaceSearchParams } from "@/fund-goals/workspace/FundGoalWorkspace";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ViewFundGoalForm from "@/fund-goals/workspace/ViewFundGoalForm";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import routes from "@/fund-goals/routes";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the FundGoalWorkspaceDetailPage component.
 */
interface FundGoalWorkspaceDetailPageProps {
  readonly params: Promise<{ fundId: string }>;
  readonly searchParams: Promise<FundGoalWorkspaceSearchParams>;
}

/**
 * Displays details and recent balance events for a Fund Goal.
 */
const FundGoalWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: FundGoalWorkspaceDetailPageProps): Promise<JSX.Element> {
  const { fundId } = await params;
  const {
    accountingPeriodId,
    fundIds,
    search,
    balanceEventPage,
    balanceEventSort,
  } = await searchParams;
  const selectedFundIds = toRepeatedSearchParams(fundIds);
  const apiClient = await createApiClient();
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
  const goalResponse = await apiClient.GET("/fund-goals/fund/{fundId}", {
    params: { path: { fundId }, query: { accountingPeriodId: periodId } },
  });
  if (goalResponse.response.status === 404) {
    redirect(workspaceUrl);
  }
  const fundGoal = unwrapApiResponse(
    goalResponse,
    "Failed to fetch the fund goal",
  );
  const progress = unwrapApiResponse(
    await apiClient.GET(
      "/fund-goals/{fundGoalId}/progress/{accountingPeriodId}",
      {
        params: {
          path: { fundGoalId: fundGoal.id, accountingPeriodId: periodId },
        },
      },
    ),
    "Failed to fetch Fund Goal progress",
  );
  const events = unwrapApiResponse(
    await apiClient.GET("/fund-goals/balance-events/accounting-period-range", {
      params: {
        query: {
          "Range.Start": periodId,
          "Range.End": periodId,
          "Filter.FundIds": [fundId],
          "Filter.AccountingPeriodIds": [periodId],
          Limit: rowsPerPage,
          Offset: getPageOffset(normalizePageValue(balanceEventPage)),
          ...(isNotNullOrUndefined(balanceEventSort)
            ? { Sort: balanceEventSort }
            : {}),
        },
      },
    }),
    "Failed to fetch Fund Goal balance events",
  );
  const currentUrl = routes.workspaceDetail(fundId, {
    accountingPeriodId: periodId,
    ...(selectedFundIds.length ? { fundIds: selectedFundIds } : {}),
    ...(isNotNullOrUndefined(search) ? { search } : {}),
    ...(isNotNullOrUndefined(balanceEventPage) ? { balanceEventPage } : {}),
    ...(isNotNullOrUndefined(balanceEventSort) ? { balanceEventSort } : {}),
  });
  return (
    <PageLayout>
      <FundGoalWorkspacePageHeader
        backHref={workspaceUrl}
        title="Goal Details"
      />
      <ViewFundGoalForm
        fundGoal={fundGoal}
        progress={progress}
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
export default FundGoalWorkspaceDetailPage;
