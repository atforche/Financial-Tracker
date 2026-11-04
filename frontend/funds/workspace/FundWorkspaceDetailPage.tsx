import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import BalanceHistoryFilter from "@/balance-events/BalanceHistoryFilter";
import BalancePeriodList from "@/balance-events/BalancePeriodList";
import { FundBalanceEventSort } from "@/funds/types";
import FundGoalTab from "@/fund-goals/FundGoalTab";
import FundWorkspacePageHeader from "@/funds/workspace/FundWorkspacePageHeader";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/types";
import GoalPeriodRangeFilter from "@/framework/goals/GoalPeriodRangeFilter";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import { Stack } from "@mui/material";
import ViewFundForm from "@/funds/workspace/ViewFundForm";
import createApiClient from "@/framework/data/createApiClient";
import getDetailViewParams from "@/balance-events/getDetailViewParams";
import { redirect } from "next/navigation";
import resolveBalanceHistory from "@/balance-events/resolveBalanceHistory";
import resolveGoalRange from "@/framework/goals/resolveGoalRange";
import routes from "@/funds/routes";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the FundWorkspaceDetailPage component.
 */
interface FundWorkspaceDetailPageProps {
  readonly params: Promise<{ fundId: string }>;
  readonly searchParams: Promise<FundWorkspaceSearchParams>;
}

/**
 * Page for viewing details of a single fund within the workspace.
 */
const FundWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: FundWorkspaceDetailPageProps): Promise<JSX.Element> {
  const { fundId } = await params;
  const resolvedSearchParams = await searchParams;
  const { search, balanceEventPage, balanceEventSort, pageSize } =
    resolvedSearchParams;
  const apiClient = await createApiClient();
  const currentBalanceEventPage = normalizePageValue(balanceEventPage);
  const rowsPerPage = getRowsPerPage(pageSize);
  const balanceEventOffset = getPageOffset(
    currentBalanceEventPage,
    rowsPerPage,
  );
  const periods = unwrapApiResponse(
    await apiClient.GET("/accounting-periods", {
      params: {
        query: { Sort: AccountingPeriodSort.DateDescending, Limit: 500 },
      },
    }),
    "Failed to fetch accounting periods",
  ).items;
  const range = resolveBalanceHistory(resolvedSearchParams, periods);
  const recentActivityStartDate = range.rangeStart;
  const recentActivityEndDate = range.rangeEnd;
  const fundsResponse = await apiClient.GET("/funds/with-balances");
  const funds = unwrapApiResponse(fundsResponse, "Failed to fetch funds");
  const fund = funds.items.find((item) => item.id === fundId);

  const workspaceSearchParams: FundWorkspaceSearchParams = {
    ...(typeof search !== "undefined" ? { search } : {}),
  };
  const detailSearchParams: FundWorkspaceSearchParams = {
    ...workspaceSearchParams,
    ...(typeof balanceEventPage !== "undefined" ? { balanceEventPage } : {}),
    ...(typeof balanceEventSort !== "undefined" ? { balanceEventSort } : {}),
    ...getDetailViewParams(resolvedSearchParams),
  };
  const workspaceUrl = routes.workspace(workspaceSearchParams);

  if (typeof fund === "undefined") {
    redirect(workspaceUrl);
  }

  const [balanceEventsResponse, activityTotalsResponse] = await Promise.all([
    apiClient.GET("/funds/balance-events/date-range", {
      params: {
        query: {
          "Range.Start": recentActivityStartDate,
          "Range.End": recentActivityEndDate,
          "Filter.Names": [fund.name],
          IncludePending: true,
          Sort: balanceEventSort ?? FundBalanceEventSort.DateDescending,
          Limit: rowsPerPage,
          Offset: balanceEventOffset,
        },
      },
    }),
    apiClient.GET("/funds/balance-events/date-range/totals", {
      params: {
        query: {
          "Range.Start": recentActivityStartDate,
          "Range.End": recentActivityEndDate,
          "Filter.Names": [fund.name],
        },
      },
    }),
  ]);
  const balanceEvents = unwrapApiResponse(
    balanceEventsResponse,
    "Failed to fetch fund balance events",
  );
  const activityTotals = unwrapApiResponse(
    activityTotalsResponse,
    "Failed to fetch fund activity totals",
  );

  const currentUrl = routes.workspaceDetail(fund.id, detailSearchParams);
  const addTransactionHref = transactionRoutes.workspaceCreate({
    fundIds: [fund.id],
    returnUrl: currentUrl,
  });

  const goalRange = resolveGoalRange(resolvedSearchParams, periods);
  const goalEntries =
    resolvedSearchParams.tab !== "goal" || goalRange === null
      ? []
      : unwrapApiResponse(
          await apiClient.GET("/fund-goals/fund/{fundId}/history", {
            params: {
              path: { fundId },
              query: {
                "Range.Start": goalRange.start,
                "Range.End": goalRange.end,
              },
            },
          }),
          "Failed to fetch fund goal history",
        );
  const openPeriods = await Promise.all(
    goalEntries.flatMap(({ goal }) =>
      goal.accountingPeriod?.isOpen === true
        ? [
            apiClient
              .GET("/accounting-periods/{accountingPeriodId}", {
                params: {
                  path: { accountingPeriodId: goal.accountingPeriod.id },
                },
              })
              .then((response) =>
                unwrapApiResponse(
                  response,
                  "Failed to fetch accounting period",
                ),
              ),
          ]
        : [],
    ),
  );
  const periodRange =
    range.mode === "accounting-period" && resolvedSearchParams.tab === "history"
      ? unwrapApiResponse(
          await apiClient.GET("/funds/accounting-period-range", {
            params: {
              query: {
                "Range.Start": range.startAccountingPeriodId,
                "Range.End": range.endAccountingPeriodId,
                "Filter.Names": [fund.name],
              },
            },
          }),
          "Failed to fetch fund periods",
        ).accountingPeriods
      : null;

  return (
    <PageLayout>
      <FundWorkspacePageHeader backHref={workspaceUrl} title={fund.name} />
      <ViewFundForm
        fund={fund}
        redirectUrl={currentUrl}
        deleteRedirectUrl={workspaceUrl}
        recentBalanceEvents={balanceEvents.items}
        recentBalanceEventCount={balanceEvents.totalCount}
        activityTotals={activityTotals}
        recentActivityBalances={activityTotals.dates ?? []}
        trendsHref={routes.trends({
          mode: "date",
          fundName: [fund.name],
          startDate: recentActivityStartDate,
          endDate: recentActivityEndDate,
          returnUrl: currentUrl,
        })}
        addTransactionHref={addTransactionHref}
        historyFilter={
          <BalanceHistoryFilter accountingPeriods={periods} range={range} />
        }
        historyPeriods={
          periodRange === null ? null : (
            <BalancePeriodList
              periods={periodRange.map((item) => ({
                id: item.accountingPeriod.id,
                name: item.accountingPeriod.name,
                openingBalance: item.openingBalance.totalBalance,
                closingBalance: item.closingBalance.totalBalance,
              }))}
            />
          )
        }
        goalContent={
          <Stack spacing={2}>
            {goalRange === null ? null : (
              <GoalPeriodRangeFilter
                accountingPeriods={periods}
                start={goalRange.start}
                end={goalRange.end}
              />
            )}
            <FundGoalTab
              entries={goalEntries}
              openPeriods={openPeriods}
              redirectUrl={currentUrl}
            />
          </Stack>
        }
      />
    </PageLayout>
  );
};

export default FundWorkspaceDetailPage;
