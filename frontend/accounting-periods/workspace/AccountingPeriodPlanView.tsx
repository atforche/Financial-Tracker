import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import type { AccountGoalWithProgress } from "@/account-goals/types";
import AccountGoalsFrame from "@/accounting-periods/workspace/AccountGoalsFrame";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import ExpectedIncomeFundGoalContributionsCard from "@/accounting-periods/workspace/ExpectedIncomeFundGoalContributionsCard";
import ExpectedIncomeSourcesFrame from "@/accounting-periods/workspace/ExpectedIncomeSourcesFrame";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import FundGoalsFrame from "@/accounting-periods/workspace/FundGoalsFrame";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import ResponsivePageSize from "@/framework/listframe/ResponsivePageSize";
import type { Route } from "next";
import createApiClient from "@/framework/data/createApiClient";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

interface AccountingPeriodPlanViewProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly currentUrl: Route;
  readonly pageSize?: number | string | null | undefined;
  readonly fundGoalPage?: number | string | null | undefined;
  readonly accountGoalPage?: number | string | null | undefined;
}

/** Displays the inputs used to establish an accounting period's plan. */
const AccountingPeriodPlanView = async function ({
  accountingPeriod,
  currentUrl,
  pageSize,
  fundGoalPage,
  accountGoalPage,
}: AccountingPeriodPlanViewProps): Promise<JSX.Element> {
  const apiClient = await createApiClient();
  const rowsPerPage = getRowsPerPage(pageSize);
  const accountingPeriodId = accountingPeriod.id;
  const [
    goalsResponse,
    progressResponse,
    accountGoalsResponse,
    accountGoalProgressResponse,
  ] = await Promise.all([
    apiClient.GET("/fund-goals", {
      params: {
        query: {
          "Filter.AccountingPeriodIds": [accountingPeriodId],
          Limit: rowsPerPage,
          Offset: getPageOffset(normalizePageValue(fundGoalPage), rowsPerPage),
        },
      },
    }),
    apiClient.GET("/fund-goals/progress/{accountingPeriodId}", {
      params: { path: { accountingPeriodId } },
    }),
    apiClient.GET("/account-goals", {
      params: {
        query: {
          "Filter.AccountingPeriodIds": [accountingPeriodId],
          Limit: rowsPerPage,
          Offset: getPageOffset(
            normalizePageValue(accountGoalPage),
            rowsPerPage,
          ),
        },
      },
    }),
    apiClient.GET("/account-goals/progress/{accountingPeriodId}", {
      params: { path: { accountingPeriodId } },
    }),
  ]);
  const goals = unwrapApiResponse(goalsResponse, "Failed to fetch fund goals");
  const progress = unwrapApiResponse(
    progressResponse,
    "Failed to fetch fund goal progress",
  );
  const progressByGoal = new Map(
    progress.map((item) => [item.fundGoalId, item.progress]),
  );
  const goalsWithProgress: FundGoalWithProgress[] = goals.items.flatMap(
    (goal) => {
      const goalProgress = progressByGoal.get(goal.id);
      return goalProgress ? [{ ...goal, progress: goalProgress }] : [];
    },
  );
  const accountGoals = unwrapApiResponse(
    accountGoalsResponse,
    "Failed to fetch account goals",
  );
  const accountGoalProgress = unwrapApiResponse(
    accountGoalProgressResponse,
    "Failed to fetch account goal progress",
  );
  const progressByAccountGoal = new Map(
    accountGoalProgress.map((item) => [item.accountGoalId, item.progress]),
  );
  const accountGoalsWithProgress: AccountGoalWithProgress[] =
    accountGoals.items.flatMap((accountGoal) => {
      const goalProgress = progressByAccountGoal.get(accountGoal.id);
      return goalProgress ? [{ ...accountGoal, progress: goalProgress }] : [];
    });

  return (
    <>
      <ResponsivePageSize desktopBreakpoint="lg" />
      <ResponsiveGrid columns={{ xs: 1, lg: 2 }} spacing={3}>
        <ExpectedIncomeFundGoalContributionsCard
          expectedIncome={accountingPeriod.expectedIncome}
          plannedFundGoalContributions={
            accountingPeriod.plannedGoalContributions
          }
          expectedFundGoalContributions={
            accountingPeriod.expectedGoalContributions
          }
        />
      </ResponsiveGrid>
      <ExpectedIncomeSourcesFrame
        accountingPeriod={accountingPeriod}
        redirectUrl={currentUrl}
      />
      <ResponsiveGrid columns={{ xs: 1, lg: 2 }} spacing={3}>
        <FundGoalsFrame
          goals={goalsWithProgress}
          totalCount={goals.totalCount}
          accountingPeriodId={accountingPeriodId}
          returnUrl={currentUrl}
        />
        <AccountGoalsFrame
          goals={accountGoalsWithProgress}
          totalCount={accountGoals.totalCount}
          accountingPeriodId={accountingPeriodId}
          returnUrl={currentUrl}
        />
      </ResponsiveGrid>
    </>
  );
};

export default AccountingPeriodPlanView;
