import { FundGoalBalanceEventSortModel } from "@/framework/data/api";
import type { JSX } from "react";
import PeriodEntityLayout from "@/accounting-periods/workspace/PeriodEntityLayout";
import PeriodEntityTransactions from "@/accounting-periods/workspace/PeriodEntityTransactions";
import PeriodFundGoalCard from "@/accounting-periods/workspace/PeriodFundGoalCard";
import fundRoutes from "@/funds/routes";
import getPeriodEntityDetail from "@/accounting-periods/workspace/getPeriodEntityDetail";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Search parameters supported by the page for one fund in one accounting period.
 */
interface PeriodFundSearchParams {
  returnUrl?: string;
  periodTab?: string;
}

/**
 * Props for the PeriodFundDetailPage component.
 */
interface PeriodFundDetailPageProps {
  readonly params: Promise<{ accountingPeriodId: string; fundId: string }>;
  readonly searchParams: Promise<PeriodFundSearchParams>;
}

/**
 * Displays one fund within one accounting period: its balances, goal, daily balance, and that period's transactions.
 */
const PeriodFundDetailPage = async function ({
  params,
  searchParams,
}: PeriodFundDetailPageProps): Promise<JSX.Element> {
  const { accountingPeriodId, fundId } = await params;
  const resolvedSearchParams = await searchParams;
  const { apiClient, detail } = await getPeriodEntityDetail(accountingPeriodId);
  const period = detail.accountingPeriod;
  const { returnUrl, periodTab } = resolvedSearchParams;
  const periodUrl = routes.workspaceDetail(
    period.id,
    periodTab === undefined ? {} : { tab: periodTab },
  );
  const item = detail.funds.find(({ fund }) => fund.id === fundId);
  if (item === undefined) {
    redirect(periodUrl);
  }
  const periodRange = { "Range.Start": period.id, "Range.End": period.id };
  const [goalResponse, eventsResponse, totalsResponse, balanceDatesResponse] =
    await Promise.all([
      apiClient.GET("/fund-goals/fund/{fundId}", {
        params: {
          path: { fundId },
          query: { accountingPeriodId: period.id },
        },
      }),
      apiClient.GET("/fund-goals/balance-events/accounting-period-range", {
        params: {
          query: {
            ...periodRange,
            "Filter.FundIds": [fundId],
            Sort: FundGoalBalanceEventSortModel.DateDescending,
          },
        },
      }),
      apiClient.GET(
        "/fund-goals/balance-events/accounting-period-range/totals",
        {
          params: { query: { ...periodRange, "Filter.FundIds": [fundId] } },
        },
      ),
      apiClient.GET(
        "/funds/{fundId}/accounting-periods/{accountingPeriodId}/balance-dates",
        { params: { path: { fundId, accountingPeriodId: period.id } } },
      ),
    ]);
  const goal =
    goalResponse.response.status === 404
      ? null
      : unwrapApiResponse(goalResponse, "Failed to fetch the fund goal");
  const events = unwrapApiResponse(
    eventsResponse,
    "Failed to fetch fund balance events",
  );
  const totals = unwrapApiResponse(
    totalsResponse,
    "Failed to fetch fund activity totals",
  );
  const balanceDates = unwrapApiResponse(
    balanceDatesResponse,
    "Failed to fetch fund daily balances",
  );
  const currentUrl = routes.fundDetail(period.id, fundId, resolvedSearchParams);
  // A return link belongs to the period it was opened from, so a neighbor goes back to its own period instead
  const neighborParams = periodTab === undefined ? {} : { periodTab };
  const getNeighborLink = function (
    neighbor: typeof detail.previousPeriod,
  ): { name: string; href: ReturnType<typeof routes.fundDetail> } | null {
    return neighbor === null
      ? null
      : {
          name: neighbor.name,
          href: routes.fundDetail(neighbor.id, fundId, neighborParams),
        };
  };

  return (
    <PeriodEntityLayout
      name={item.fund.name}
      period={period}
      previousPeriod={getNeighborLink(detail.previousPeriod)}
      nextPeriod={getNeighborLink(detail.nextPeriod)}
      periodHref={periodUrl}
      returnUrl={returnUrl}
      allPeriodsHref={fundRoutes.workspaceDetail(fundId, {})}
      openingBalance={item.openingBalance}
      closingBalance={item.closingBalance}
      totals={totals}
      goalCard={
        goal === null ||
        item.goalProgress === null ||
        item.goalProgress === undefined ? null : (
          <PeriodFundGoalCard
            goal={goal}
            progress={item.goalProgress}
            accountingPeriod={period}
            redirectUrl={currentUrl}
          />
        )
      }
      dailyBalances={balanceDates.dates}
      balanceLabel="Fund Balance"
      transactions={
        <PeriodEntityTransactions
          entityName="fund"
          events={events.items}
          addTransactionHref={transactionRoutes.workspaceCreate({
            accountingPeriodIds: [period.id],
            fundIds: [fundId],
            returnUrl: currentUrl,
          })}
          returnUrl={currentUrl}
        />
      }
    />
  );
};

export type { PeriodFundSearchParams };
export default PeriodFundDetailPage;
