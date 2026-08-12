import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import {
  isNotNullOrUndefined,
  isNullOrUndefined,
} from "@/framework/nullHelpers";
import { FundGoalBalanceEventSort } from "@/fund-goals/types";
import FundGoalWorkspacePageHeader from "@/fund-goals/workspace/FundGoalWorkspacePageHeader";
import type { FundGoalWorkspaceSearchParams } from "@/fund-goals/workspace/FundGoalWorkspace";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsivePageSize from "@/framework/listframe/ResponsivePageSize";
import ViewFundGoalForm from "@/fund-goals/workspace/ViewFundGoalForm";
import createApiClient from "@/framework/data/createApiClient";
import dayjs from "dayjs";
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
    pageSize,
    balanceEventSort,
    returnUrl,
  } = await searchParams;
  const rowsPerPage = getRowsPerPage(pageSize);
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
    ...(isNotNullOrUndefined(returnUrl) ? { returnUrl } : {}),
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
  if (isNullOrUndefined(fundGoal.accountingPeriod)) {
    redirect(workspaceUrl);
  }
  const recentActivityStartDate = dayjs()
    .year(fundGoal.accountingPeriod.year)
    .month(fundGoal.accountingPeriod.month - 1)
    .startOf("month")
    .format("YYYY-MM-DD");
  const recentActivityEndDate = dayjs(recentActivityStartDate)
    .endOf("month")
    .format("YYYY-MM-DD");
  const [
    progressResponse,
    eventsResponse,
    recentActivityResponse,
    recentActivityBalancesResponse,
  ] = await Promise.all([
    apiClient.GET("/fund-goals/{fundGoalId}/progress/{accountingPeriodId}", {
      params: {
        path: { fundGoalId: fundGoal.id, accountingPeriodId: periodId },
      },
    }),
    apiClient.GET("/fund-goals/balance-events/accounting-period-range", {
      params: {
        query: {
          "Range.Start": periodId,
          "Range.End": periodId,
          "Filter.FundIds": [fundId],
          "Filter.AccountingPeriodIds": [periodId],
          Limit: rowsPerPage,
          Offset: getPageOffset(
            normalizePageValue(balanceEventPage),
            rowsPerPage,
          ),
          ...(isNotNullOrUndefined(balanceEventSort)
            ? { Sort: balanceEventSort }
            : {}),
        },
      },
    }),
    apiClient.GET("/fund-goals/balance-events/accounting-period-range", {
      params: {
        query: {
          "Range.Start": periodId,
          "Range.End": periodId,
          "Filter.FundIds": [fundId],
          "Filter.AccountingPeriodIds": [periodId],
          Sort: FundGoalBalanceEventSort.Date,
          Limit: 500,
          Offset: 0,
        },
      },
    }),
    apiClient.GET("/funds/date-range", {
      params: {
        query: {
          "Range.Start": recentActivityStartDate,
          "Range.End": recentActivityEndDate,
          "Filter.Names": [fundGoal.fund.name],
          Limit: 1,
          Offset: 0,
        },
      },
    }),
  ]);
  const progress = unwrapApiResponse(
    progressResponse,
    "Failed to fetch Fund Goal progress",
  );
  const events = unwrapApiResponse(
    eventsResponse,
    "Failed to fetch Fund Goal balance events",
  );
  const recentActivity = unwrapApiResponse(
    recentActivityResponse,
    "Failed to fetch recent Fund Goal activity",
  );
  const recentActivityBalances = unwrapApiResponse(
    recentActivityBalancesResponse,
    "Failed to fetch recent Fund Goal balance history",
  );
  const currentUrl = routes.workspaceDetail(fundId, {
    accountingPeriodId: periodId,
    ...(selectedFundIds.length ? { fundIds: selectedFundIds } : {}),
    ...(isNotNullOrUndefined(search) ? { search } : {}),
    ...(isNotNullOrUndefined(balanceEventPage) ? { balanceEventPage } : {}),
    ...(isNotNullOrUndefined(balanceEventSort) ? { balanceEventSort } : {}),
    ...(isNotNullOrUndefined(returnUrl) ? { returnUrl } : {}),
  });
  return (
    <PageLayout>
      <ResponsivePageSize desktopBreakpoint="lg" />
      <FundGoalWorkspacePageHeader
        backHref={returnUrl ?? workspaceUrl}
        title="Goal Details"
      />
      <ViewFundGoalForm
        fundGoal={fundGoal}
        progress={progress}
        redirectUrl={currentUrl}
        recentBalanceEvents={events.items}
        recentBalanceEventCount={events.totalCount}
        recentActivityEvents={recentActivity.items}
        recentActivityBalances={recentActivityBalances.dates}
        trendsHref={routes.trends({
          fundName: [fundGoal.fund.name],
          startAccountingPeriodId: periodId,
          endAccountingPeriodId: periodId,
        })}
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
