import { AccountBalanceEventSortModel } from "@/framework/data/api";
import type { JSX } from "react";
import PeriodAccountGoalCard from "@/accounting-periods/workspace/PeriodAccountGoalCard";
import PeriodEntityLayout from "@/accounting-periods/workspace/PeriodEntityLayout";
import PeriodEntityTransactions from "@/accounting-periods/workspace/PeriodEntityTransactions";
import accountRoutes from "@/accounts/routes";
import getPeriodEntityDetail from "@/accounting-periods/workspace/getPeriodEntityDetail";
import { isDebtAccountType } from "@/accounts/helpers";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Search parameters supported by the page for one account in one accounting period.
 */
interface PeriodAccountSearchParams {
  returnUrl?: string;
  periodTab?: string;
}

/**
 * Props for the PeriodAccountDetailPage component.
 */
interface PeriodAccountDetailPageProps {
  readonly params: Promise<{ accountingPeriodId: string; accountId: string }>;
  readonly searchParams: Promise<PeriodAccountSearchParams>;
}

/**
 * Displays one account within one accounting period: its balances, goal, daily balance, and that period's transactions.
 */
const PeriodAccountDetailPage = async function ({
  params,
  searchParams,
}: PeriodAccountDetailPageProps): Promise<JSX.Element> {
  const { accountingPeriodId, accountId } = await params;
  const resolvedSearchParams = await searchParams;
  const { apiClient, detail } = await getPeriodEntityDetail(accountingPeriodId);
  const period = detail.accountingPeriod;
  const { returnUrl, periodTab } = resolvedSearchParams;
  const periodUrl = routes.workspaceDetail(
    period.id,
    periodTab === undefined ? {} : { tab: periodTab },
  );
  const item = detail.accounts.find(({ account }) => account.id === accountId);
  if (item === undefined) {
    redirect(periodUrl);
  }
  const [goalResponse, eventsResponse, totalsResponse, balanceDatesResponse] =
    await Promise.all([
      apiClient.GET("/account-goals/account/{accountId}", {
        params: {
          path: { accountId },
          query: { accountingPeriodId: period.id },
        },
      }),
      apiClient.GET("/accounts/{accountId}/balance-events", {
        params: {
          path: { accountId },
          query: {
            AccountingPeriodId: period.id,
            IncludePending: true,
            Sort: AccountBalanceEventSortModel.DateDescending,
          },
        },
      }),
      apiClient.GET("/accounts/{accountId}/balance-events/totals", {
        params: {
          path: { accountId },
          query: { AccountingPeriodId: period.id },
        },
      }),
      apiClient.GET(
        "/accounts/{accountId}/accounting-periods/{accountingPeriodId}/balance-dates",
        { params: { path: { accountId, accountingPeriodId: period.id } } },
      ),
    ]);
  const goal =
    goalResponse.response.status === 404
      ? null
      : unwrapApiResponse(goalResponse, "Failed to fetch the account goal");
  const events = unwrapApiResponse(
    eventsResponse,
    "Failed to fetch account balance events",
  );
  const totals = unwrapApiResponse(
    totalsResponse,
    "Failed to fetch account activity totals",
  );
  const balanceDates = unwrapApiResponse(
    balanceDatesResponse,
    "Failed to fetch account daily balances",
  );
  const currentUrl = routes.accountDetail(
    period.id,
    accountId,
    resolvedSearchParams,
  );
  // A return link belongs to the period it was opened from, so a neighbor goes back to its own period instead
  const neighborParams = periodTab === undefined ? {} : { periodTab };
  const getNeighborLink = function (
    neighbor: typeof detail.previousPeriod,
  ): { name: string; href: ReturnType<typeof routes.accountDetail> } | null {
    return neighbor === null
      ? null
      : {
          name: neighbor.name,
          href: routes.accountDetail(neighbor.id, accountId, neighborParams),
        };
  };

  return (
    <PeriodEntityLayout
      name={item.account.name}
      period={period}
      previousPeriod={getNeighborLink(detail.previousPeriod)}
      nextPeriod={getNeighborLink(detail.nextPeriod)}
      periodHref={periodUrl}
      returnUrl={returnUrl}
      allPeriodsHref={accountRoutes.workspaceDetail(accountId, {})}
      openingBalance={item.openingBalance}
      closingBalance={item.closingBalance}
      totals={totals}
      isIncreaseGood={!isDebtAccountType(item.account.type)}
      goalCard={
        goal === null ||
        item.goalProgress === null ||
        item.goalProgress === undefined ? null : (
          <PeriodAccountGoalCard
            goal={goal}
            progress={item.goalProgress}
            isOpen={period.isOpen}
            redirectUrl={currentUrl}
          />
        )
      }
      dailyBalances={balanceDates.dates}
      balanceLabel="Account Balance"
      transactions={
        <PeriodEntityTransactions
          entityName="account"
          events={events.items}
          addTransactionHref={transactionRoutes.workspaceCreate({
            accountingPeriodIds: [period.id],
            returnUrl: currentUrl,
          })}
          returnUrl={currentUrl}
        />
      }
    />
  );
};

export type { PeriodAccountSearchParams };
export default PeriodAccountDetailPage;
