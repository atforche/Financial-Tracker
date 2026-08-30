import {
  type AccountGoal,
  AccountGoalEndingBalanceStatus,
  type AccountGoalProgress,
} from "@/account-goals/types";

interface AccountGoalPeriodProgress {
  readonly accountGoal: AccountGoal;
  readonly progress: AccountGoalProgress;
}

interface AccountGoalTrendPoint {
  readonly accountingPeriodId: string;
  readonly accountingPeriodName: string;
  readonly configuredGoalCount: number;
  readonly satisfiedGoalCount: number;
  readonly satisfiedPercentage: number;
  readonly belowZeroCount: number;
  readonly belowMinimumCount: number;
  readonly withinRangeCount: number;
  readonly aboveMaximumCount: number;
}

const getAccountGoalTrendPoint = function (
  accountingPeriod: { readonly id: string; readonly name: string },
  progress: readonly AccountGoalPeriodProgress[],
): AccountGoalTrendPoint {
  const belowZeroCount = progress.filter(
    ({ progress: item }) => !item.positiveBalance.isSatisfied,
  ).length;
  const belowMinimumCount = progress.filter(
    ({ progress: item }) =>
      item.positiveBalance.isSatisfied &&
      item.endingBalance?.status ===
        AccountGoalEndingBalanceStatus.BelowMinimum,
  ).length;
  const aboveMaximumCount = progress.filter(
    ({ progress: item }) =>
      item.positiveBalance.isSatisfied &&
      item.endingBalance?.status ===
        AccountGoalEndingBalanceStatus.AboveMaximum,
  ).length;
  const withinRangeCount = progress.filter(
    ({ progress: item }) =>
      item.positiveBalance.isSatisfied &&
      item.endingBalance?.status === AccountGoalEndingBalanceStatus.WithinRange,
  ).length;
  const configuredGoalCount = progress.length;
  const satisfiedGoalCount = progress.filter(
    ({ progress: item }) => item.isSatisfied,
  ).length;

  return {
    accountingPeriodId: accountingPeriod.id,
    accountingPeriodName: accountingPeriod.name,
    configuredGoalCount,
    satisfiedGoalCount,
    satisfiedPercentage:
      configuredGoalCount === 0
        ? 0
        : (satisfiedGoalCount / configuredGoalCount) * 100,
    belowZeroCount,
    belowMinimumCount,
    withinRangeCount,
    aboveMaximumCount,
  };
};

/**
 * Aggregates Account Goal progress into trend points.
 */
const buildAccountGoalTrendPoints = function (
  accountingPeriods: readonly { readonly id: string; readonly name: string }[],
  progressByAccountingPeriodId: ReadonlyMap<
    string,
    readonly AccountGoalPeriodProgress[]
  >,
): readonly AccountGoalTrendPoint[] {
  return accountingPeriods.map((accountingPeriod) =>
    getAccountGoalTrendPoint(
      accountingPeriod,
      progressByAccountingPeriodId.get(accountingPeriod.id) ?? [],
    ),
  );
};

export {
  buildAccountGoalTrendPoints,
  type AccountGoalPeriodProgress,
  type AccountGoalTrendPoint,
};
