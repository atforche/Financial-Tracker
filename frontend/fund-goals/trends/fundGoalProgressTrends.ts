import {
  type FundGoal,
  FundGoalEndingBalanceStatus,
  type FundGoalProgress,
} from "@/fund-goals/types";
import { getCurrencyTotal } from "@/framework/currencyHelpers";

/**
 * A Fund Goal paired with its progress for the Fund Goal's Accounting Period.
 */
interface FundGoalPeriodProgress {
  readonly fundGoal: FundGoal;
  readonly progress: FundGoalProgress;
}

/**
 * Aggregated Fund Goal progress for one Accounting Period.
 */
interface FundGoalTrendPoint {
  readonly accountingPeriodId: string;
  readonly accountingPeriodName: string;
  readonly configuredGoalCount: number;
  readonly satisfiedGoalCount: number;
  readonly satisfiedPercentage: number;
  readonly assignedContribution: number;
  readonly expectedContribution: number;
}

/**
 * Summarizes the health of Fund Goals for selected Accounting Periods.
 */
interface FundGoalHealthSummary {
  readonly configuredGoalCount: number;
  readonly satisfiedGoalCount: number;
  readonly assignedContribution: number;
  readonly expectedContribution: number;
}

const sum = (values: readonly number[]): number => getCurrencyTotal(values);

/**
 * Returns whether every configured metric for a Fund Goal is satisfied.
 */
const isFundGoalSatisfied = function ({
  progress,
}: FundGoalPeriodProgress): boolean {
  const checks = [progress.availableBalance.isSatisfied];
  if (progress.contribution !== null && progress.contribution !== undefined) {
    checks.push(progress.contribution.isSatisfied);
  }
  if (
    progress.endingBalance?.minimumBalance !== null &&
    progress.endingBalance?.minimumBalance !== undefined
  ) {
    checks.push(
      progress.endingBalance.status !==
        FundGoalEndingBalanceStatus.BelowMinimum,
    );
  }
  if (
    progress.endingBalance?.maximumBalance !== null &&
    progress.endingBalance?.maximumBalance !== undefined
  ) {
    checks.push(
      progress.endingBalance.status !==
        FundGoalEndingBalanceStatus.AboveMaximum,
    );
  }
  return checks.every((isSatisfied) => isSatisfied);
};

/**
 * Summarizes the selected Fund Goals across Accounting Periods.
 */
const getFundGoalHealthSummary = function (
  progress: readonly FundGoalPeriodProgress[],
): FundGoalHealthSummary {
  const contribution = progress.flatMap(({ progress: goalProgress }) =>
    goalProgress.contribution === null ||
    goalProgress.contribution === undefined
      ? []
      : [goalProgress.contribution],
  );
  return {
    configuredGoalCount: progress.length,
    satisfiedGoalCount: progress.filter(isFundGoalSatisfied).length,
    assignedContribution: sum(
      contribution.map(({ assignedAmount }) => assignedAmount),
    ),
    expectedContribution: sum(
      contribution.map(({ expectedAmount }) => expectedAmount),
    ),
  };
};

/**
 * Aggregates Fund Goal health and contributions for every Accounting Period in the range.
 */
const buildFundGoalTrendPoints = function (
  accountingPeriods: readonly { id: string; name: string }[],
  progressByAccountingPeriodId: ReadonlyMap<
    string,
    readonly FundGoalPeriodProgress[]
  >,
): readonly FundGoalTrendPoint[] {
  return accountingPeriods.map((accountingPeriod) => {
    const summary = getFundGoalHealthSummary(
      progressByAccountingPeriodId.get(accountingPeriod.id) ?? [],
    );
    return {
      accountingPeriodId: accountingPeriod.id,
      accountingPeriodName: accountingPeriod.name,
      configuredGoalCount: summary.configuredGoalCount,
      satisfiedGoalCount: summary.satisfiedGoalCount,
      satisfiedPercentage:
        summary.configuredGoalCount === 0
          ? 0
          : (summary.satisfiedGoalCount / summary.configuredGoalCount) * 100,
      assignedContribution: summary.assignedContribution,
      expectedContribution: summary.expectedContribution,
    };
  });
};

export {
  buildFundGoalTrendPoints,
  getFundGoalHealthSummary,
  type FundGoalPeriodProgress,
  type FundGoalHealthSummary,
  type FundGoalTrendPoint,
};
