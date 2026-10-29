import AccountingPeriodProgressView from "@/accounting-periods/workspace/AccountingPeriodProgressView";
import AccountingPeriodWorkspaceHeader from "@/accounting-periods/workspace/AccountingPeriodWorkspaceHeader";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import getAdjacentAccountingPeriods from "@/accounting-periods/workspace/periodNavigation";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the AccountingPeriodWorkspaceDetailPage component.
 */
interface AccountingPeriodWorkspaceDetailPageProps {
  readonly params: Promise<{ accountingPeriodId: string }>;
  readonly searchParams: Promise<AccountingPeriodWorkspaceSearchParams>;
}

/**
 * Displays the detailed plan, progress, activity, and actions for one accounting period.
 */
const AccountingPeriodWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: AccountingPeriodWorkspaceDetailPageProps): Promise<JSX.Element> {
  const { accountingPeriodId } = await params;
  const resolvedSearchParams = await searchParams;
  const apiClient = await createApiClient();
  const workspaceParams = {
    ...(typeof resolvedSearchParams.years !== "undefined"
      ? { years: resolvedSearchParams.years }
      : {}),
    ...(typeof resolvedSearchParams.months !== "undefined"
      ? { months: resolvedSearchParams.months }
      : {}),
    ...(typeof resolvedSearchParams.sort !== "undefined"
      ? { sort: resolvedSearchParams.sort }
      : {}),
    ...(typeof resolvedSearchParams.page !== "undefined"
      ? { page: resolvedSearchParams.page }
      : {}),
    ...(typeof resolvedSearchParams.pageSize !== "undefined"
      ? { pageSize: resolvedSearchParams.pageSize }
      : {}),
  } satisfies AccountingPeriodWorkspaceSearchParams;
  const workspaceUrl = routes.workspace(workspaceParams);
  // Links between the period's own pages keep the page the user came from, so they can still return to it
  const periodParams = {
    ...workspaceParams,
    ...(typeof resolvedSearchParams.returnUrl !== "undefined"
      ? { returnUrl: resolvedSearchParams.returnUrl }
      : {}),
  } satisfies AccountingPeriodWorkspaceSearchParams;
  const periodResponse = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    { params: { path: { accountingPeriodId } } },
  );
  if (periodResponse.error) {
    redirect(workspaceUrl);
  }
  const period = unwrapApiResponse(
    periodResponse,
    "Failed to fetch accounting period",
  );
  if (resolvedSearchParams.view === "plan") {
    redirect(
      routes.workspacePlan(period.id, {
        ...periodParams,
        ...(typeof resolvedSearchParams.incomeSourcePage !== "undefined"
          ? { incomeSourcePage: resolvedSearchParams.incomeSourcePage }
          : {}),
        ...(typeof resolvedSearchParams.fundGoalPage !== "undefined"
          ? { fundGoalPage: resolvedSearchParams.fundGoalPage }
          : {}),
        ...(typeof resolvedSearchParams.accountGoalPage !== "undefined"
          ? { accountGoalPage: resolvedSearchParams.accountGoalPage }
          : {}),
      }),
    );
  }
  const { previousPeriod, nextPeriod } = await getAdjacentAccountingPeriods(
    period.id,
  );
  const currentUrl = routes.workspaceDetail(period.id, periodParams);
  const planHref = routes.workspacePlan(period.id, periodParams);
  return (
    <PageLayout>
      <AccountingPeriodWorkspaceHeader
        accountingPeriod={period}
        workspaceUrl={workspaceUrl}
        previousPeriod={
          previousPeriod === null
            ? null
            : {
                name: previousPeriod.name,
                href: routes.workspaceDetail(previousPeriod.id, periodParams),
              }
        }
        nextPeriod={
          nextPeriod === null
            ? null
            : {
                name: nextPeriod.name,
                href: routes.workspaceDetail(nextPeriod.id, periodParams),
              }
        }
        activeView="cash-flow"
        cashFlowHref={currentUrl}
        planHref={planHref}
        redirectUrl={currentUrl}
        returnUrl={resolvedSearchParams.returnUrl}
      />
      <AccountingPeriodProgressView accountingPeriod={period} />
    </PageLayout>
  );
};

export default AccountingPeriodWorkspaceDetailPage;
