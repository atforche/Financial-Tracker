import {
  type AccountGoal,
  AccountGoalEndingBalanceStatus,
  type AccountGoalProgress,
} from "@/account-goals/types";

/**
 * An Account Goal paired with its progress for one Accounting Period.
 */
interface AccountGoalPeriodProgress {
  readonly accountGoal: AccountGoal;
  readonly progress: AccountGoalProgress;
}

/**
 * A single Accounting Period point on an Account Goal progress trend.
 */
interface AccountGoalTrendPoint {
  readonly accountingPeriodId: string;
  readonly accountingPeriodName: string;
  readonly isOpen: boolean;
  readonly configuredGoalCount: number;
  readonly satisfiedGoalCount: number;
  readonly satisfiedPercentage: number;
  readonly belowMinimumCount: number;
  readonly withinRangeCount: number;
  readonly aboveMaximumCount: number;
}

/**
 * Builds the trend point for an Account Goal in the provided Accounting Period.
 */
const getAccountGoalTrendPoint = function (
  accountingPeriod: {
    readonly id: string;
    readonly name: string;
    readonly isOpen: boolean;
  },
  progress: readonly AccountGoalPeriodProgress[],
): AccountGoalTrendPoint {
  const belowMinimumCount = progress.filter(
    ({ progress: item }) =>
      item.endingBalance.status === AccountGoalEndingBalanceStatus.BelowMinimum,
  ).length;
  const aboveMaximumCount = progress.filter(
    ({ progress: item }) =>
      item.endingBalance.status === AccountGoalEndingBalanceStatus.AboveMaximum,
  ).length;
  const withinRangeCount = progress.filter(
    ({ progress: item }) =>
      item.endingBalance.status === AccountGoalEndingBalanceStatus.WithinRange,
  ).length;
  const configuredGoalCount = progress.length;
  const satisfiedGoalCount = progress.filter(
    ({ progress: item }) => item.isSatisfied,
  ).length;

  return {
    accountingPeriodId: accountingPeriod.id,
    accountingPeriodName: accountingPeriod.name,
    isOpen: accountingPeriod.isOpen,
    configuredGoalCount,
    satisfiedGoalCount,
    satisfiedPercentage:
      configuredGoalCount === 0
        ? 0
        : (satisfiedGoalCount / configuredGoalCount) * 100,
    belowMinimumCount,
    withinRangeCount,
    aboveMaximumCount,
  };
};

/**
 * Aggregates Account Goal progress into trend points.
 */
const buildAccountGoalTrendPoints = function (
  accountingPeriods: readonly {
    readonly id: string;
    readonly name: string;
    readonly isOpen: boolean;
  }[],
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
