import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import {
  isNotNullOrUndefined,
  isNullOrUndefined,
} from "@/framework/nullHelpers";
import AccountGoalWorkspacePageHeader from "@/account-goals/workspace/AccountGoalWorkspacePageHeader";
import type { AccountGoalWorkspaceSearchParams } from "@/account-goals/workspace/types";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ViewAccountGoalForm from "@/account-goals/workspace/ViewAccountGoalForm";
import createApiClient from "@/framework/data/createApiClient";
import dayjs from "dayjs";
import { redirect } from "next/navigation";
import routes from "@/account-goals/routes";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

interface AccountGoalWorkspaceDetailPageProps {
  readonly params: Promise<{ accountId: string }>;
  readonly searchParams: Promise<AccountGoalWorkspaceSearchParams>;
}

/**
 * Displays Account Goal details and progress for one account and period.
 */
const AccountGoalWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: AccountGoalWorkspaceDetailPageProps): Promise<JSX.Element> {
  const { accountId } = await params;
  const {
    accountingPeriodId,
    accountIds,
    returnUrl,
    balanceEventPage,
    balanceEventSort,
    pageSize,
  } = await searchParams;
  const selectedAccountIds = toRepeatedSearchParams(accountIds);
  const apiClient = await createApiClient();
  const periods = unwrapApiResponse(
    await apiClient.GET("/accounting-periods", {
      params: { query: { Limit: 500 } },
    }),
    "Failed to fetch Account Goal accounting periods",
  );
  const periodId = accountingPeriodId ?? periods.items[0]?.id;
  const workspaceUrl = routes.workspace({
    ...(isNotNullOrUndefined(periodId) ? { accountingPeriodId: periodId } : {}),
    ...(selectedAccountIds.length ? { accountIds: selectedAccountIds } : {}),
    ...(isNotNullOrUndefined(returnUrl) ? { returnUrl } : {}),
  });
  if (isNullOrUndefined(periodId)) {
    redirect(workspaceUrl);
  }
  const goalResponse = await apiClient.GET(
    "/account-goals/account/{accountId}",
    {
      params: { path: { accountId }, query: { accountingPeriodId: periodId } },
    },
  );
  if (goalResponse.response.status === 404) {
    redirect(workspaceUrl);
  }
  const accountGoal = unwrapApiResponse(
    goalResponse,
    "Failed to fetch the Account Goal",
  );
  if (isNullOrUndefined(accountGoal.accountingPeriod)) {
    redirect(workspaceUrl);
  }
  const periodStartDate = dayjs()
    .year(accountGoal.accountingPeriod.year)
    .month(accountGoal.accountingPeriod.month - 1)
    .startOf("month")
    .format("YYYY-MM-DD");
  const periodEndDate = dayjs(periodStartDate)
    .endOf("month")
    .format("YYYY-MM-DD");
  const rowsPerPage = getRowsPerPage(pageSize);
  const [progressResponse, eventsResponse, balanceDatesResponse] =
    await Promise.all([
      apiClient.GET(
        "/account-goals/{accountGoalId}/progress/{accountingPeriodId}",
        {
          params: {
            path: {
              accountGoalId: accountGoal.id,
              accountingPeriodId: periodId,
            },
          },
        },
      ),
      apiClient.GET("/accounts/{accountId}/balance-events", {
        params: {
          path: { accountId },
          query: {
            "Range.Start": periodStartDate,
            "Range.End": periodEndDate,
            AccountingPeriodId: periodId,
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
      apiClient.GET(
        "/accounts/{accountId}/accounting-periods/{accountingPeriodId}/balance-dates",
        {
          params: { path: { accountId, accountingPeriodId: periodId } },
        },
      ),
    ]);
  const progress = unwrapApiResponse(
    progressResponse,
    "Failed to fetch Account Goal progress",
  );
  const events = unwrapApiResponse(
    eventsResponse,
    "Failed to fetch account balance events",
  );
  const balanceDates = unwrapApiResponse(
    balanceDatesResponse,
    "Failed to fetch Account Goal daily balances",
  );
  const currentUrl = routes.workspaceDetail(accountId, {
    accountingPeriodId: periodId,
    ...(selectedAccountIds.length ? { accountIds: selectedAccountIds } : {}),
    ...(isNotNullOrUndefined(balanceEventPage) ? { balanceEventPage } : {}),
    ...(isNotNullOrUndefined(balanceEventSort) ? { balanceEventSort } : {}),
    ...(isNotNullOrUndefined(pageSize) ? { pageSize } : {}),
    ...(isNotNullOrUndefined(returnUrl) ? { returnUrl } : {}),
  });
  return (
    <PageLayout>
      <AccountGoalWorkspacePageHeader backHref={returnUrl ?? workspaceUrl} />
      <ViewAccountGoalForm
        accountGoal={accountGoal}
        progress={progress}
        redirectUrl={currentUrl}
        isReadOnly={!accountGoal.accountingPeriod.isOpen}
        recentBalanceEvents={events.items}
        recentBalanceEventCount={events.totalCount}
        recentActivityBalances={balanceDates.dates}
        periodOpeningBalance={balanceDates.openingBalance}
        accountingPeriodId={periodId}
        accountId={accountId}
        trendsHref={routes.trends({
          accountName: [accountGoal.account.name],
          startAccountingPeriodId: periodId,
          endAccountingPeriodId: periodId,
          returnUrl: currentUrl,
        })}
        addTransactionHref={transactionRoutes.workspaceCreate({
          accountingPeriodIds: [periodId],
          accountIds: [accountId],
          returnUrl: currentUrl,
        })}
      />
    </PageLayout>
  );
};

export default AccountGoalWorkspaceDetailPage;
