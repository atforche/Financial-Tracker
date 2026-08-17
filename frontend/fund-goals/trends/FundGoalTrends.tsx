import {
  type FundGoalPeriodProgress,
  buildFundGoalTrendPoints,
} from "@/fund-goals/trends/fundGoalProgressTrends";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import FundGoalAchievementTrendChart from "@/fund-goals/trends/FundGoalAchievementTrendChart";
import FundGoalContributionTrendChart from "@/fund-goals/trends/FundGoalContributionTrendChart";
import FundGoalTrendsFilter from "@/fund-goals/trends/FundGoalTrendsFilter";
import type { FundGoalTrendsSearchParams } from "@/fund-goals/trends/helpers";
import FundGoalTrendsSummaryCards from "@/fund-goals/trends/FundGoalTrendsSummaryCards";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import createApiClient from "@/framework/data/createApiClient";
import { isNullOrUndefined } from "@/framework/nullHelpers";
import loadAllPages from "@/framework/data/loadAllPages";
import { redirect } from "next/navigation";
import routes from "@/fund-goals/routes";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the FundGoalTrends component.
 */
interface FundGoalTrendsProps {
  readonly searchParams: Promise<FundGoalTrendsSearchParams>;
}

/**
 * Displays Fund Goal trends.
 */
const FundGoalTrends = async function ({
  searchParams,
}: FundGoalTrendsProps): Promise<JSX.Element> {
  const params = await searchParams;
  const apiClient = await createApiClient();
  const periods = await loadAllPages(async (limit, offset) =>
    unwrapApiResponse(
      await apiClient.GET("/accounting-periods", {
        params: {
          query: { Sort: AccountingPeriodSort.DateDescending, limit, offset },
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
  const fundNames = toRepeatedSearchParams(params.fundName);
  const fundGoals = await loadAllPages(async (limit, offset) =>
    unwrapApiResponse(
      await apiClient.GET("/fund-goals", {
        params: {
          query: {
            "Filter.AccountingPeriodIds": selectedPeriods.map(
              (period) => period.id,
            ),
            limit,
            offset,
          },
        },
      }),
      "Failed to load Fund Goals",
    ),
  );
  const filteredFundGoals = fundGoals.filter(
    (fundGoal) =>
      fundNames.length === 0 || fundNames.includes(fundGoal.fund.name),
  );
  const selectedFundIds = [
    ...new Set(filteredFundGoals.map((fundGoal) => fundGoal.fund.id)),
  ];
  const hasResolvedSelectedFundIds =
    selectedFundIds.length === fundNames.length;
  const progressResultsByFundGoalId = new Map(
    (
      await Promise.all(
        selectedPeriods.map(async (accountingPeriod) =>
          unwrapApiResponse(
            await apiClient.GET("/fund-goals/progress/{accountingPeriodId}", {
              params: { path: { accountingPeriodId: accountingPeriod.id } },
            }),
            "Failed to load Fund Goal progress",
          ),
        ),
      )
    )
      .flatMap((results) => results)
      .map((result) => [result.fundGoalId, result.progress]),
  );
  const progressWithAccountingPeriod = filteredFundGoals.flatMap((fundGoal) => {
    const progress = progressResultsByFundGoalId.get(fundGoal.id);
    return typeof progress === "undefined" ? [] : [{ fundGoal, progress }];
  });
  const progressByAccountingPeriodId = new Map<
    string,
    FundGoalPeriodProgress[]
  >();
  progressWithAccountingPeriod.forEach((goalProgress) => {
    const accountingPeriodId = goalProgress.fundGoal.accountingPeriod?.id;
    if (typeof accountingPeriodId === "string") {
      const progress =
        progressByAccountingPeriodId.get(accountingPeriodId) ?? [];
      progress.push(goalProgress);
      progressByAccountingPeriodId.set(accountingPeriodId, progress);
    }
  });
  const chartPeriods = [...selectedPeriods].reverse();
  const chartPoints = buildFundGoalTrendPoints(
    chartPeriods,
    progressByAccountingPeriodId,
  );
  const transactionWorkspaceHref =
    selectedPeriods.length === 0
      ? null
      : transactionRoutes.workspace({
          accountingPeriodIds: selectedPeriods.map((period) => period.id),
          ...(hasResolvedSelectedFundIds && selectedFundIds.length > 0
            ? { fundIds: selectedFundIds }
            : fundNames.length === 0
              ? {}
              : { fundNames }),
          returnUrl: routes.trends({
            ...(fundNames.length === 0 ? {} : { fundName: fundNames }),
            ...(typeof start === "undefined"
              ? {}
              : { startAccountingPeriodId: start }),
            ...(typeof end === "undefined"
              ? {}
              : { endAccountingPeriodId: end }),
          }),
        });

  return (
    <PageLayout>
      <ConstrainedContent>
        <FundGoalTrendsFilter
          accountingPeriods={periods}
          availableFundNames={[
            ...new Set(fundGoals.map((fundGoal) => fundGoal.fund.name)),
          ]}
          defaultAccountingPeriodId={periods[0]?.id ?? null}
          transactionWorkspaceHref={transactionWorkspaceHref}
        />
      </ConstrainedContent>
      <FundGoalTrendsSummaryCards
        progress={selectedPeriods.flatMap(
          (period) => progressByAccountingPeriodId.get(period.id) ?? [],
        )}
      />
      <ResponsiveGrid columns={{ xs: 1, lg: 2 }}>
        <FundGoalAchievementTrendChart chartPoints={chartPoints} />
        <FundGoalContributionTrendChart chartPoints={chartPoints} />
      </ResponsiveGrid>
    </PageLayout>
  );
};
export type { FundGoalTrendsSearchParams };
export default FundGoalTrends;
