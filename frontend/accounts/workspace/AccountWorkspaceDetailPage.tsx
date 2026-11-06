import resolveEntityHistory, {
  periodTab,
} from "@/balance-events/resolveEntityHistory";
import { AccountBalanceEventSortModel } from "@/framework/data/api";
import AccountDetailActions from "@/accounts/workspace/AccountDetailActions";
import AccountGoalHistory from "@/account-goals/AccountGoalHistory";
import { AccountType } from "@/accounts/types";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/types";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import BalanceHistoryFilter from "@/balance-events/BalanceHistoryFilter";
import BalancePeriodList from "@/balance-events/BalancePeriodList";
import BalanceTrendChart from "@/framework/charts/BalanceTrendChart";
import EntityHistoryLayout from "@/balance-events/EntityHistoryLayout";
import EntityTransactionList from "@/balance-events/EntityTransactionList";
import type { JSX } from "react";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import { buildBalanceTrendChartPoints } from "@/framework/charts/balanceTrendHelpers";
import createApiClient from "@/framework/data/createApiClient";
import { getCurrencyDifference } from "@/framework/currencyHelpers";
import getDateSummary from "@/balance-events/getDateSummary";
import getDetailViewParams from "@/balance-events/getDetailViewParams";
import { isDebtAccountType } from "@/accounts/helpers";
import { redirect } from "next/navigation";
import routes from "@/accounts/routes";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the AccountWorkspaceDetailPage component.
 */
interface AccountWorkspaceDetailPageProps {
  readonly params: Promise<{
    accountId: string;
  }>;
  readonly searchParams: Promise<AccountWorkspaceSearchParams>;
}

/**
 * Displays one account across a range of dates or accounting periods. Only the period view shows goals.
 */
const AccountWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: AccountWorkspaceDetailPageProps): Promise<JSX.Element> {
  const { accountId } = await params;
  const resolvedSearchParams = await searchParams;
  const { search, accountType } = resolvedSearchParams;
  const apiClient = await createApiClient();
  // Only the By Period view needs every period, for its range picker and default range
  const [periodsResponse, accountsResponse] = await Promise.all([
    resolvedSearchParams.tab === periodTab
      ? apiClient.GET("/accounting-periods", {
          params: {
            query: { Sort: AccountingPeriodSort.DateDescending, Limit: 500 },
          },
        })
      : null,
    apiClient.GET("/accounts/with-balances", {
      params: { query: { "Filter.Ids": [accountId] } },
    }),
  ]);
  const periods =
    periodsResponse === null
      ? []
      : unwrapApiResponse(periodsResponse, "Failed to fetch accounting periods")
          .items;
  const [account] = unwrapApiResponse(
    accountsResponse,
    "Failed to fetch accounts",
  ).items;
  const workspaceUrl = routes.workspace({
    ...(search === undefined ? {} : { search }),
    ...(accountType === undefined ? {} : { accountType }),
  });
  if (account === undefined) {
    redirect(workspaceUrl);
  }

  const currentUrl = routes.workspaceDetail(accountId, {
    ...(search === undefined ? {} : { search }),
    ...(accountType === undefined ? {} : { accountType }),
    ...getDetailViewParams(resolvedSearchParams),
  });
  const {
    view,
    dates,
    periods: periodRange,
  } = resolveEntityHistory(resolvedSearchParams, periods);
  const isIncreaseGood = !isDebtAccountType(account.type);
  // Range summaries are net balances for adding accounts together, where debt is negative. One account shows what it holds.
  const toAccountBalance = (balance: number): number =>
    isIncreaseGood ? balance : 0 - balance;
  const hasGoals = account.type === AccountType.Standard;
  const eventQuery =
    view === "date"
      ? { "Range.Start": dates.start, "Range.End": dates.end }
      : { AccountingPeriodIds: [...(periodRange?.ids ?? [])] };

  const [eventsResponse, totalsResponse] =
    view === "period" && periodRange === null
      ? [null, null]
      : await Promise.all([
          view === "date"
            ? apiClient.GET("/accounts/{accountId}/balance-events", {
                params: {
                  path: { accountId },
                  query: {
                    ...eventQuery,
                    IncludePending: true,
                    Sort: AccountBalanceEventSortModel.DateDescending,
                  },
                },
              })
            : null,
          apiClient.GET("/accounts/{accountId}/balance-events/totals", {
            params: { path: { accountId }, query: eventQuery },
          }),
        ]);
  const events =
    eventsResponse === null
      ? []
      : unwrapApiResponse(
          eventsResponse,
          "Failed to fetch account balance events",
        ).items;
  const netTotals =
    totalsResponse === null
      ? null
      : unwrapApiResponse(
          totalsResponse,
          "Failed to fetch account activity totals",
        );
  const totals =
    netTotals === null
      ? null
      : {
          ...netTotals,
          openingBalance:
            netTotals.openingBalance === null ||
            netTotals.openingBalance === undefined
              ? null
              : toAccountBalance(netTotals.openingBalance),
          dates: (netTotals.dates ?? []).map((item) => ({
            ...item,
            totalBalance: toAccountBalance(item.totalBalance),
          })),
        };

  const [balancesResponse, goalsResponse] =
    view === "date" || periodRange === null
      ? [null, null]
      : await Promise.all([
          apiClient.GET("/accounts/accounting-period-range", {
            params: {
              query: {
                "Range.Start": periodRange.start,
                "Range.End": periodRange.end,
                "Filter.Names": [account.name],
              },
            },
          }),
          hasGoals
            ? apiClient.GET("/account-goals/account/{accountId}/history", {
                params: {
                  path: { accountId },
                  query: {
                    "Range.Start": periodRange.start,
                    "Range.End": periodRange.end,
                  },
                },
              })
            : null,
        ]);
  const periodBalances =
    balancesResponse === null
      ? []
      : unwrapApiResponse(
          balancesResponse,
          "Failed to fetch account periods",
        ).accountingPeriods.map((item) => ({
          ...item,
          openingBalance: {
            ...item.openingBalance,
            totalBalance: toAccountBalance(item.openingBalance.totalBalance),
          },
          closingBalance: {
            ...item.closingBalance,
            totalBalance: toAccountBalance(item.closingBalance.totalBalance),
          },
        }));
  const goalEntries =
    goalsResponse === null
      ? null
      : unwrapApiResponse(
          goalsResponse,
          "Failed to fetch account goal history",
        );
  const firstPeriod = periodBalances.at(0);
  const lastPeriod = periodBalances.at(-1);
  const summary =
    totals === null
      ? null
      : view === "date"
        ? getDateSummary(totals, account.currentBalance.postedBalance)
        : firstPeriod === undefined || lastPeriod === undefined
          ? null
          : {
              openingBalance: firstPeriod.openingBalance.totalBalance,
              closingBalance: lastPeriod.closingBalance.totalBalance,
              totals,
              ...(goalEntries === null
                ? {}
                : {
                    goalsMet: {
                      met: goalEntries.filter(
                        ({ progress }) => progress.isSatisfied,
                      ).length,
                      total: goalEntries.length,
                    },
                  }),
            };

  return (
    <EntityHistoryLayout
      entityName="account"
      name={account.name}
      subtitle={account.financialInstitution ?? null}
      back={{ href: workspaceUrl, label: "Back to Accounts" }}
      returnUrl={resolvedSearchParams.returnUrl}
      isIncreaseGood={isIncreaseGood}
      actions={
        <AccountDetailActions
          account={account}
          addTransactionHref={transactionRoutes.workspaceCreate({
            accountIds: [account.id],
            returnUrl: currentUrl,
          })}
          trendsHref={routes.trends({
            mode: "date",
            accountName: [account.name],
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
      {view === "date" ? (
        <BalanceTrendChart
          chartPoints={buildBalanceTrendChartPoints({
            mode: "Date",
            accountingPeriods: null,
            dates: totals?.dates ?? [],
            ...(totals?.granularity === null ||
            totals?.granularity === undefined
              ? {}
              : { granularity: totals.granularity }),
          })}
          title="Balance"
          variant="surface"
          emptyMessage="No posted balance movement is available in this range."
          xAxisLabel="Date"
          yAxisLabel="Posted Balance"
        />
      ) : goalEntries === null ? (
        <>
          <BalanceTrendChart
            chartPoints={buildBalanceTrendChartPoints({
              mode: "AccountingPeriod",
              accountingPeriods: periodBalances.map((item) => ({
                accountingPeriodId: item.accountingPeriod.id,
                accountingPeriodName: item.accountingPeriod.name,
                year: item.accountingPeriod.year,
                month: item.accountingPeriod.month,
                totalOpeningBalance: item.openingBalance.totalBalance,
                totalClosingBalance: item.closingBalance.totalBalance,
              })),
              dates: null,
            })}
            title="Balance"
            variant="surface"
            emptyMessage="No accounting periods are available in this range."
            xAxisLabel="Accounting Period"
            yAxisLabel="Closing Balance"
          />
          <BalancePeriodList
            isIncreaseGood={isIncreaseGood}
            periods={periodBalances.toReversed().map((item) => ({
              id: item.accountingPeriod.id,
              name: item.accountingPeriod.name,
              href: accountingPeriodRoutes.accountDetail(
                item.accountingPeriod.id,
                accountId,
                { returnUrl: currentUrl },
              ),
              openingBalance: item.openingBalance.totalBalance,
              closingBalance: item.closingBalance.totalBalance,
            }))}
          />
        </>
      ) : (
        <AccountGoalHistory
          entries={goalEntries}
          changes={
            new Map(
              periodBalances.map((item) => [
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
      {view === "date" ? (
        <EntityTransactionList
          entityName="account"
          events={events}
          returnUrl={currentUrl}
        />
      ) : null}
    </EntityHistoryLayout>
  );
};

export default AccountWorkspaceDetailPage;
