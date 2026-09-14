import {
  type AccountGoalPeriodProgress,
  buildAccountGoalTrendPoints,
} from "@/account-goals/trends/accountGoalProgressTrends";
import AccountGoalHistory from "@/account-goals/trends/AccountGoalHistory";
import AccountGoalOverview from "@/account-goals/trends/AccountGoalOverview";
import AccountGoalTrendsFilter from "@/account-goals/trends/AccountGoalTrendsFilter";
import type { AccountGoalTrendsSearchParams } from "@/account-goals/trends/helpers";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import TrendsBackLink from "@/framework/view/TrendsBackLink";
import createApiClient from "@/framework/data/createApiClient";
import { getDefaultTrendAccountingPeriodRange } from "@/framework/routes/trendRange";
import { isNullOrUndefined } from "@/framework/nullHelpers";
import loadAllPages from "@/framework/data/loadAllPages";
import { redirect } from "next/navigation";
import routes from "@/account-goals/routes";
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
  const defaultRange = getDefaultTrendAccountingPeriodRange(periods);
  if (
    (isNullOrUndefined(params.startAccountingPeriodId) ||
      isNullOrUndefined(params.endAccountingPeriodId)) &&
    latest
  ) {
    redirect(
      routes.trends({
        ...params,
        startAccountingPeriodId: defaultRange?.start ?? latest.id,
        endAccountingPeriodId: defaultRange?.end ?? latest.id,
      }),
    );
  }
  const start = params.startAccountingPeriodId ?? defaultRange?.start;
  const end = params.endAccountingPeriodId ?? defaultRange?.end;
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
  const accountsInRange = [
    ...new Map(
      accountGoals.map((goal) => [
        goal.account.id,
        { id: goal.account.id, name: goal.account.name },
      ]),
    ).values(),
  ].sort((a, b) => a.name.localeCompare(b.name));
  const selectedAccountInRange = accountsInRange.find(
    (account) => account.id === params.accountId,
  );
  const selectedAccountId = params.accountId;
  const accountGoalsOutsideRange =
    selectedAccountInRange === undefined && selectedAccountId !== undefined
      ? await loadAllPages(async (limit, offset) =>
          unwrapApiResponse(
            await apiClient.GET("/account-goals", {
              params: {
                query: {
                  "Filter.AccountIds": [selectedAccountId],
                  Limit: limit,
                  Offset: offset,
                },
              },
            }),
            "Failed to load Account Goals",
          ),
        )
      : [];
  const selectedAccount =
    selectedAccountInRange ?? accountGoalsOutsideRange[0]?.account;
  const accounts =
    selectedAccount !== undefined &&
    !accountsInRange.some((account) => account.id === selectedAccount.id)
      ? [...accountsInRange, selectedAccount].sort((a, b) =>
          a.name.localeCompare(b.name),
        )
      : accountsInRange;
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
  const historyEntries: AccountGoalPeriodProgress[] = [];
  accountGoals.forEach((accountGoal) => {
    const progress = progressById.get(accountGoal.id);
    const periodId = accountGoal.accountingPeriod?.id;
    if (progress !== undefined && periodId !== undefined) {
      if (accountGoal.account.id === selectedAccount?.id) {
        historyEntries.push({ accountGoal, progress });
      }
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
      <TrendsBackLink
        returnUrl={params.returnUrl}
        workspace="account-goals"
        label="Back to Account Goal Details"
      />
      <ConstrainedContent>
        <AccountGoalTrendsFilter
          accountingPeriods={periods}
          accounts={accounts}
        />
      </ConstrainedContent>
      {selectedAccount === undefined ? (
        <AccountGoalOverview
          points={chartPoints}
          periods={selectedPeriods}
          progressByPeriod={progressByPeriod}
          accounts={accounts}
          searchParams={params}
        />
      ) : (
        <AccountGoalHistory
          periods={[...selectedPeriods].reverse()}
          entries={historyEntries}
          accountName={selectedAccount.name}
        />
      )}
    </PageLayout>
  );
};

export default AccountGoalTrends;
