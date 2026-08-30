import {
  type AccountGoalPeriodProgress,
  buildAccountGoalTrendPoints,
} from "@/account-goals/trends/accountGoalProgressTrends";
import AccountGoalAchievementTrendChart from "@/account-goals/trends/AccountGoalAchievementTrendChart";
import AccountGoalStatusTrendChart from "@/account-goals/trends/AccountGoalStatusTrendChart";
import AccountGoalTrendsFilter from "@/account-goals/trends/AccountGoalTrendsFilter";
import type { AccountGoalTrendsSearchParams } from "@/account-goals/trends/helpers";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import createApiClient from "@/framework/data/createApiClient";
import { isNullOrUndefined } from "@/framework/nullHelpers";
import loadAllPages from "@/framework/data/loadAllPages";
import { redirect } from "next/navigation";
import routes from "@/account-goals/routes";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Entry point for Account Goal trends.
 */
interface AccountGoalTrendsProps {
  readonly searchParams: Promise<AccountGoalTrendsSearchParams>;
}

/**
 * Displays Account Goal progress trends for the selected range.
 */
const AccountGoalTrends = async function ({
  searchParams,
}: AccountGoalTrendsProps): Promise<JSX.Element> {
  const params = await searchParams;
  const apiClient = await createApiClient();
  const periods = await loadAllPages(async (limit, offset) =>
    unwrapApiResponse(
      await apiClient.GET("/accounting-periods", {
        params: {
          query: {
            Sort: AccountingPeriodSort.DateDescending,
            Limit: limit,
            Offset: offset,
          },
        },
      }),
      "Failed to load accounting periods",
    ),
  );
  const latest = periods[0] ?? null;
  if (
    (isNullOrUndefined(params.startAccountingPeriodId) ||
      isNullOrUndefined(params.endAccountingPeriodId)) &&
    latest
  ) {
    redirect(
      routes.trends({
        startAccountingPeriodId: latest.id,
        endAccountingPeriodId: latest.id,
      }),
    );
  }
  const start = params.startAccountingPeriodId ?? latest?.id;
  const end = params.endAccountingPeriodId ?? latest?.id;
  const startIndex = periods.findIndex((period) => period.id === start);
  const endIndex = periods.findIndex((period) => period.id === end);
  const selectedPeriods =
    startIndex >= 0 && endIndex >= 0
      ? periods.slice(
          Math.min(startIndex, endIndex),
          Math.max(startIndex, endIndex) + 1,
        )
      : [];
  const accountGoals = await loadAllPages(async (limit, offset) =>
    unwrapApiResponse(
      await apiClient.GET("/account-goals", {
        params: {
          query: {
            "Filter.AccountingPeriodIds": selectedPeriods.map(
              (period) => period.id,
            ),
            Limit: limit,
            Offset: offset,
          },
        },
      }),
      "Failed to load Account Goals",
    ),
  );
  const accountNames = toRepeatedSearchParams(params.accountName);
  const filteredAccountGoals = accountGoals.filter(
    (goal) =>
      accountNames.length === 0 || accountNames.includes(goal.account.name),
  );
  const progressById = new Map(
    (
      await Promise.all(
        selectedPeriods.map(async (period) =>
          unwrapApiResponse(
            await apiClient.GET(
              "/account-goals/progress/{accountingPeriodId}",
              { params: { path: { accountingPeriodId: period.id } } },
            ),
            "Failed to load Account Goal progress",
          ),
        ),
      )
    )
      .flatMap((results) => results)
      .map((result) => [result.accountGoalId, result.progress]),
  );
  const progressByPeriod = new Map<string, AccountGoalPeriodProgress[]>();
  filteredAccountGoals.forEach((accountGoal) => {
    const progress = progressById.get(accountGoal.id);
    const periodId = accountGoal.accountingPeriod?.id;
    if (progress !== undefined && periodId !== undefined) {
      const entries = progressByPeriod.get(periodId) ?? [];
      entries.push({ accountGoal, progress });
      progressByPeriod.set(periodId, entries);
    }
  });
  const chartPoints = buildAccountGoalTrendPoints(
    [...selectedPeriods].reverse(),
    progressByPeriod,
  );

  return (
    <PageLayout>
      <ConstrainedContent>
        <AccountGoalTrendsFilter
          accountingPeriods={periods}
          availableAccountNames={[
            ...new Set(accountGoals.map((goal) => goal.account.name)),
          ]}
          defaultAccountingPeriodId={latest?.id ?? null}
        />
      </ConstrainedContent>
      <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
        <AccountGoalAchievementTrendChart chartPoints={chartPoints} />
        <AccountGoalStatusTrendChart chartPoints={chartPoints} />
      </ResponsiveGrid>
    </PageLayout>
  );
};

export default AccountGoalTrends;
