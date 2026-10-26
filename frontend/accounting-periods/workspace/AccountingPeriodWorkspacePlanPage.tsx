import AccountingPeriodPlanView from "@/accounting-periods/workspace/AccountingPeriodPlanView";
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
 * Props for the AccountingPeriodWorkspacePlanPage component.
 */
interface AccountingPeriodWorkspacePlanPageProps {
  readonly params: Promise<{ accountingPeriodId: string }>;
  readonly searchParams: Promise<AccountingPeriodWorkspaceSearchParams>;
}

/**
 * Displays the planning workflow for one accounting period.
 */
const AccountingPeriodWorkspacePlanPage = async function ({
  params,
  searchParams,
}: AccountingPeriodWorkspacePlanPageProps): Promise<JSX.Element> {
  const { accountingPeriodId } = await params;
  const resolvedSearchParams = await searchParams;
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
  const apiClient = await createApiClient();
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
  const { previousPeriod, nextPeriod } = await getAdjacentAccountingPeriods(
    period.id,
  );
  const cashFlowHref = routes.workspaceDetail(period.id, workspaceParams);
  const adjacentPlanParams = {
    ...workspaceParams,
    ...(typeof resolvedSearchParams.planTab !== "undefined"
      ? { planTab: resolvedSearchParams.planTab }
      : {}),
    ...(typeof resolvedSearchParams.incomeSourceSort !== "undefined"
      ? { incomeSourceSort: resolvedSearchParams.incomeSourceSort }
      : {}),
    ...(typeof resolvedSearchParams.fundGoalSort !== "undefined"
      ? { fundGoalSort: resolvedSearchParams.fundGoalSort }
      : {}),
    ...(typeof resolvedSearchParams.accountGoalSort !== "undefined"
      ? { accountGoalSort: resolvedSearchParams.accountGoalSort }
      : {}),
  } satisfies AccountingPeriodWorkspaceSearchParams;
  const currentUrl = routes.workspacePlan(period.id, {
    ...workspaceParams,
    ...(typeof resolvedSearchParams.incomeSourcePage !== "undefined"
      ? { incomeSourcePage: resolvedSearchParams.incomeSourcePage }
      : {}),
    ...(typeof resolvedSearchParams.incomeSourceSort !== "undefined"
      ? { incomeSourceSort: resolvedSearchParams.incomeSourceSort }
      : {}),
    ...(typeof resolvedSearchParams.fundGoalPage !== "undefined"
      ? { fundGoalPage: resolvedSearchParams.fundGoalPage }
      : {}),
    ...(typeof resolvedSearchParams.fundGoalSort !== "undefined"
      ? { fundGoalSort: resolvedSearchParams.fundGoalSort }
      : {}),
    ...(typeof resolvedSearchParams.accountGoalPage !== "undefined"
      ? { accountGoalPage: resolvedSearchParams.accountGoalPage }
      : {}),
    ...(typeof resolvedSearchParams.accountGoalSort !== "undefined"
      ? { accountGoalSort: resolvedSearchParams.accountGoalSort }
      : {}),
    ...(typeof resolvedSearchParams.planTab !== "undefined"
      ? { planTab: resolvedSearchParams.planTab }
      : {}),
  });

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
                href: routes.workspacePlan(
                  previousPeriod.id,
                  adjacentPlanParams,
                ),
              }
        }
        nextPeriod={
          nextPeriod === null
            ? null
            : {
                name: nextPeriod.name,
                href: routes.workspacePlan(nextPeriod.id, adjacentPlanParams),
              }
        }
        activeView="plan"
        cashFlowHref={cashFlowHref}
        planHref={currentUrl}
        redirectUrl={currentUrl}
      />
      <AccountingPeriodPlanView
        accountingPeriod={period}
        currentUrl={currentUrl}
        pageSize={resolvedSearchParams.pageSize}
        fundGoalPage={resolvedSearchParams.fundGoalPage}
        fundGoalSort={resolvedSearchParams.fundGoalSort}
        accountGoalPage={resolvedSearchParams.accountGoalPage}
        accountGoalSort={resolvedSearchParams.accountGoalSort}
      />
    </PageLayout>
  );
};

export default AccountingPeriodWorkspacePlanPage;
