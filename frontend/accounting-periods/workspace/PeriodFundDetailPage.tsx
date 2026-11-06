import type { JSX } from "react";
import PeriodEntityLayout from "@/accounting-periods/workspace/PeriodEntityLayout";
import PeriodEntityTransactions from "@/accounting-periods/workspace/PeriodEntityTransactions";
import PeriodFundGoalCard from "@/accounting-periods/workspace/PeriodFundGoalCard";
import createApiClient from "@/framework/data/createApiClient";
import fundRoutes from "@/funds/routes";
import { getAllPeriodsHref } from "@/accounting-periods/workspace/helpers";
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
  const { returnUrl, periodTab } = resolvedSearchParams;
  const periodUrl = routes.workspaceDetail(
    accountingPeriodId,
    periodTab === undefined ? {} : { tab: periodTab },
  );
  const apiClient = await createApiClient();
  const detailResponse = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}/funds/{fundId}",
    { params: { path: { accountingPeriodId, fundId } } },
  );
  // The period's page handles a missing period, so a missing fund only needs to get back to it
  if (detailResponse.response.status === 404) {
    redirect(periodUrl);
  }
  const detail = unwrapApiResponse(
    detailResponse,
    "Failed to fetch the fund in the accounting period",
  );
  const period = detail.accountingPeriod;
  const item = detail.fund;
  const { goal } = detail;
  const currentUrl = routes.fundDetail(period.id, fundId, resolvedSearchParams);
  // A return link belongs to the period it was opened from, so a neighbor goes back to its own period instead
  const neighborParams = periodTab === undefined ? {} : { periodTab };

  return (
    <PeriodEntityLayout
      name={item.fund.name}
      detail={detail}
      getPeriodHref={(id) => routes.fundDetail(id, fundId, neighborParams)}
      periodHref={periodUrl}
      returnUrl={returnUrl}
      allPeriodsHref={getAllPeriodsHref(
        (entityParams) => fundRoutes.workspaceDetail(fundId, entityParams),
        returnUrl,
        currentUrl,
      )}
      openingBalance={item.openingBalance}
      closingBalance={item.closingBalance}
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
      balanceLabel="Fund Balance"
      transactions={
        <PeriodEntityTransactions
          entityName="fund"
          events={detail.events}
          addTransactionHref={transactionRoutes.workspaceCreate({
            accountingPeriodIds: [period.id],
            fundIds: [fundId],
            returnUrl: currentUrl,
          })}
          isOpen={period.isOpen}
          returnUrl={currentUrl}
        />
      }
    />
  );
};

export type { PeriodFundSearchParams };
export default PeriodFundDetailPage;
