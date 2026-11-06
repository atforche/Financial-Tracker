import type { JSX } from "react";
import PeriodAccountGoalCard from "@/accounting-periods/workspace/PeriodAccountGoalCard";
import PeriodEntityLayout from "@/accounting-periods/workspace/PeriodEntityLayout";
import PeriodEntityTransactions from "@/accounting-periods/workspace/PeriodEntityTransactions";
import accountRoutes from "@/accounts/routes";
import createApiClient from "@/framework/data/createApiClient";
import { getAllPeriodsHref } from "@/accounting-periods/workspace/helpers";
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
  const { returnUrl, periodTab } = resolvedSearchParams;
  const periodUrl = routes.workspaceDetail(
    accountingPeriodId,
    periodTab === undefined ? {} : { tab: periodTab },
  );
  const apiClient = await createApiClient();
  const detailResponse = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}/accounts/{accountId}",
    { params: { path: { accountingPeriodId, accountId } } },
  );
  // The period's page handles a missing period, so a missing account only needs to get back to it
  if (detailResponse.response.status === 404) {
    redirect(periodUrl);
  }
  const detail = unwrapApiResponse(
    detailResponse,
    "Failed to fetch the account in the accounting period",
  );
  const period = detail.accountingPeriod;
  const item = detail.account;
  const { goal } = detail;
  const currentUrl = routes.accountDetail(
    period.id,
    accountId,
    resolvedSearchParams,
  );
  // A return link belongs to the period it was opened from, so a neighbor goes back to its own period instead
  const neighborParams = periodTab === undefined ? {} : { periodTab };

  return (
    <PeriodEntityLayout
      name={item.account.name}
      detail={detail}
      getPeriodHref={(id) =>
        routes.accountDetail(id, accountId, neighborParams)
      }
      periodHref={periodUrl}
      returnUrl={returnUrl}
      allPeriodsHref={getAllPeriodsHref(
        (entityParams) =>
          accountRoutes.workspaceDetail(accountId, entityParams),
        returnUrl,
        currentUrl,
      )}
      openingBalance={item.openingBalance}
      closingBalance={item.closingBalance}
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
      balanceLabel="Account Balance"
      transactions={
        <PeriodEntityTransactions
          entityName="account"
          events={detail.events}
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
