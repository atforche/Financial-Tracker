import type {
  FundBalanceEvent,
  FundsInAccountingPeriodRange,
} from "@/funds/types";
import type { FundGoal, FundGoalProgress } from "@/fund-goals/types";
import resolveEntityHistory, {
  periodTab,
} from "@/balance-events/resolveEntityHistory";
import type { BalanceEventTotals } from "@/balance-events/types";
import BalanceHistoryFilter from "@/balance-events/BalanceHistoryFilter";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import EntityHistoryLayout from "@/balance-events/EntityHistoryLayout";
import EntityTransactionList from "@/balance-events/EntityTransactionList";
import FundDetailActions from "@/funds/workspace/FundDetailActions";
import FundGoalHistory from "@/fund-goals/FundGoalHistory";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/types";
import type { JSX } from "react";
import { buildBalanceTrendChartPoints } from "@/framework/charts/balanceTrendHelpers";
import createApiClient from "@/framework/data/createApiClient";
import { getCurrencyDifference } from "@/framework/currencyHelpers";
import getDateSummary from "@/balance-events/getDateSummary";
import getDetailViewParams from "@/balance-events/getDetailViewParams";
import isEntityId from "@/framework/data/isEntityId";
import isFundGoalMet from "@/fund-goals/isFundGoalMet";
import { redirect } from "next/navigation";
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
 * The API client used to fetch a fund's activity.
 */
type ApiClient = Awaited<ReturnType<typeof createApiClient>>;

/**
 * A fund's activity across a date range.
 */
interface DateActivity {
  readonly events: FundBalanceEvent[];
  readonly totals: BalanceEventTotals;
}

/**
 * A fund's totals, balances, and goals across an accounting period range.
 */
interface PeriodActivity {
  readonly totals: Pick<BalanceEventTotals, "totalInflow" | "totalOutflow">;
  readonly balances: FundsInAccountingPeriodRange["accountingPeriods"];
  readonly entries: readonly {
    readonly goal: FundGoal;
    readonly progress: FundGoalProgress;
  }[];
}

/**
 * Fetches a fund's activity across a date range.
 */
const getDateActivity = async function (
  apiClient: ApiClient,
  fundId: string,
  dates: { readonly start: string; readonly end: string },
): Promise<DateActivity> {
  const { events, totals } = unwrapApiResponse(
    await apiClient.GET("/funds/balance-events/date-range/with-totals", {
      params: {
        query: {
          "Range.Start": dates.start,
          "Range.End": dates.end,
          "Filter.Ids": [fundId],
          IncludePending: true,
        },
      },
    }),
    "Failed to fetch fund activity",
  );
  return { events, totals };
};

/**
 * Fetches a fund's totals, balances, and goals across an accounting period range.
 */
const getPeriodActivity = async function (
  apiClient: ApiClient,
  fund: { readonly id: string },
  periods: { readonly start: string; readonly end: string },
): Promise<PeriodActivity> {
  const range = { "Range.Start": periods.start, "Range.End": periods.end };
  const eventQuery = { ...range, "Filter.FundIds": [fund.id] };
  const [totalsResponse, balancesResponse, goalsResponse] = await Promise.all([
    apiClient.GET("/fund-goals/balance-events/accounting-period-range/totals", {
      params: { query: eventQuery },
    }),
    apiClient.GET("/funds/accounting-period-range", {
      params: { query: { ...range, "Filter.Ids": [fund.id] } },
    }),
    apiClient.GET("/fund-goals/fund/{fundId}/history", {
      params: { path: { fundId: fund.id }, query: range },
    }),
  ]);
  const entries = unwrapApiResponse(
    goalsResponse,
    "Failed to fetch fund goal history",
  );
  return {
    totals: unwrapApiResponse(
      totalsResponse,
      "Failed to fetch fund activity totals",
    ),
    balances: unwrapApiResponse(
      balancesResponse,
      "Failed to fetch fund periods",
    ).accountingPeriods,
    entries,
  };
};

/**
 * Displays one fund across a range of dates or accounting periods. Only the period view shows goals.
 */
const FundWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: FundWorkspaceDetailPageProps): Promise<JSX.Element> {
  const { fundId } = await params;
  const resolvedSearchParams = await searchParams;
  const { search } = resolvedSearchParams;
  const workspaceParams = search === undefined ? {} : { search };
  const workspaceUrl = routes.workspace(workspaceParams);
  if (!isEntityId(fundId)) {
    redirect(workspaceUrl);
  }
  const apiClient = await createApiClient();
  // Only the By Period view needs every period, for its range picker and default range
  const [periodsResponse, fundsResponse] = await Promise.all([
    resolvedSearchParams.tab === periodTab
      ? apiClient.GET("/accounting-periods", {
          params: {
            query: { Limit: 500 },
          },
        })
      : null,
    apiClient.GET("/funds/with-balances", {
      params: { query: { "Filter.Ids": [fundId] } },
    }),
  ]);
  const periods =
    periodsResponse === null
      ? []
      : unwrapApiResponse(periodsResponse, "Failed to fetch accounting periods")
          .items;
  const [fund] = unwrapApiResponse(
    fundsResponse,
    "Failed to fetch funds",
  ).items;
  if (fund === undefined) {
    redirect(workspaceUrl);
  }

  const currentUrl = routes.workspaceDetail(fundId, {
    ...workspaceParams,
    ...getDetailViewParams(resolvedSearchParams),
  });
  const {
    view,
    dates,
    periods: periodRange,
  } = resolveEntityHistory(resolvedSearchParams, periods);
  const dateActivity =
    view === "date" ? await getDateActivity(apiClient, fundId, dates) : null;
  const periodActivity =
    view === "period" && periodRange !== null
      ? await getPeriodActivity(apiClient, fund, periodRange)
      : null;
  const firstPeriod = periodActivity?.balances.at(0);
  const lastPeriod = periodActivity?.balances.at(-1);
  const summary =
    dateActivity !== null
      ? getDateSummary(dateActivity.totals, fund.currentBalance.postedBalance)
      : periodActivity === null ||
          firstPeriod === undefined ||
          lastPeriod === undefined
        ? null
        : {
            openingBalance: firstPeriod.openingBalance.totalBalance,
            closingBalance: lastPeriod.closingBalance.totalBalance,
            totals: periodActivity.totals,
            goalsMet: {
              met: periodActivity.entries.filter(({ progress }) =>
                isFundGoalMet(progress),
              ).length,
              total: periodActivity.entries.length,
            },
          };

  return (
    <EntityHistoryLayout
      entityName="fund"
      name={fund.name}
      subtitle={fund.description === "" ? null : fund.description}
      back={{ href: workspaceUrl, label: "Back to Funds" }}
      returnUrl={resolvedSearchParams.returnUrl}
      actions={
        <FundDetailActions
          fund={fund}
          addTransactionHref={transactionRoutes.workspaceCreate({
            fundIds: [fund.id],
            returnUrl: currentUrl,
          })}
          trendsHref={routes.trends({
            mode: "date",
            fundName: [fund.name],
            returnUrl: currentUrl,
          })}
          redirectUrl={currentUrl}
          deleteRedirectUrl={workspaceUrl}
        />
      }
      view={view}
      dateFilter={<BalanceHistoryFilter start={dates.start} end={dates.end} />}
      periodFilter={
        periodRange === null ? null : (
          <BalanceHistoryFilter
            start={periodRange.start}
            end={periodRange.end}
            accountingPeriods={periods}
          />
        )
      }
      summary={summary}
    >
      {dateActivity === null ? null : (
        <BalanceTrendChart
          chartPoints={buildBalanceTrendChartPoints({
            mode: "Date",
            accountingPeriods: null,
            dates: dateActivity.totals.dates ?? [],
            ...(dateActivity.totals.granularity === null ||
            dateActivity.totals.granularity === undefined
              ? {}
              : { granularity: dateActivity.totals.granularity }),
          })}
          title="Balance"
          emptyMessage="No posted balance movement is available in this range."
          xAxisLabel="Date"
          yAxisLabel="Fund Balance"
        />
      )}
      {periodActivity === null ? null : (
        <FundGoalHistory
          entries={periodActivity.entries}
          changes={
            new Map(
              periodActivity.balances.map((item) => [
                item.accountingPeriod.id,
                getCurrencyDifference(
                  item.closingBalance.totalBalance,
                  item.openingBalance.totalBalance,
                ),
              ]),
            )
          }
          redirectUrl={currentUrl}
        />
      )}
      {dateActivity === null ? null : (
        <EntityTransactionList
          entityName="fund"
          events={dateActivity.events}
          returnUrl={currentUrl}
        />
      )}
    </EntityHistoryLayout>
  );
};

export default FundWorkspaceDetailPage;
