import {
  EndingBalanceStatus,
  type FundGoal,
  type FundGoalProgress,
  FundedBalanceStatus,
} from "@/fund-goals/types";
import {
  compareCurrencyAmounts,
  getCurrencyTotal,
} from "@/framework/currencyHelpers";

/**
 * A Fund Goal paired with its progress for the Fund Goal's Accounting Period.
 */
interface FundGoalPeriodProgress {
  readonly fundGoal: FundGoal;
  readonly progress: FundGoalProgress;
}

/**
 * Aggregated progress for one Fund Goal metric in an Accounting Period.
 */
interface FundGoalMetricTrendPoint {
  readonly accountingPeriodId: string;
  readonly accountingPeriodName: string;
  readonly currentAmount: number;
  readonly targetAmount: number;
  readonly configuredGoalCount: number;
  readonly satisfiedGoalCount: number;
  readonly satisfiedPercentage: number;
}

/**
 * Fund Goal metrics that can be displayed across an Accounting Period range.
 */
type FundGoalMetric =
  | "availableBalance"
  | "contribution"
  | "minimumFundedBalance"
  | "maximumFundedBalance"
  | "endingBalance";

/**
 * Details used to render one Fund Goal metric trend.
 */
interface FundGoalMetricDefinition {
  readonly title: string;
  readonly yAxisLabel: string;
  readonly emptyMessage: string;
  readonly presentation: "amount" | "percentage";
  readonly currentLabel: string;
  readonly targetLabel: string;
}

const fundGoalMetricDefinitions: Readonly<
  Record<FundGoalMetric, FundGoalMetricDefinition>
> = {
  availableBalance: {
    title: "Positive Available Balance",
    yAxisLabel: "Fund Goals Satisfied",
    emptyMessage:
      "No available-balance progress is available for the selected trends range.",
    presentation: "percentage",
    currentLabel: "Positive available balance",
    targetLabel: "Positive balance",
  },
  contribution: {
    title: "Monthly Contribution",
    yAxisLabel: "Contribution",
    emptyMessage:
      "No monthly contribution goals are configured for the selected trends range.",
    presentation: "amount",
    currentLabel: "Assigned",
    targetLabel: "Recommended contribution",
  },
  minimumFundedBalance: {
    title: "Minimum Funded Amount",
    yAxisLabel: "Fund Goals Satisfied",
    emptyMessage:
      "No minimum funded amounts are configured for the selected trends range.",
    presentation: "percentage",
    currentLabel: "Minimum funded amount satisfied",
    targetLabel: "Minimum funded amount",
  },
  maximumFundedBalance: {
    title: "Maximum Funded Amount",
    yAxisLabel: "Fund Goals Satisfied",
    emptyMessage:
      "No maximum funded amounts are configured for the selected trends range.",
    presentation: "percentage",
    currentLabel: "Maximum funded amount satisfied",
    targetLabel: "Maximum funded amount",
  },
  endingBalance: {
    title: "Target Ending Balance",
    yAxisLabel: "Fund Goals Satisfied",
    emptyMessage:
      "No target ending balances are configured for the selected trends range.",
    presentation: "percentage",
    currentLabel: "Target ending balance satisfied",
    targetLabel: "Target ending balance",
  },
};

const sum = (values: readonly number[]): number => getCurrencyTotal(values);

/**
 * Aggregates a configured Fund Goal metric for every Accounting Period in the range.
 */
const buildFundGoalMetricTrendPoints = function (
  metric: FundGoalMetric,
  accountingPeriods: readonly { id: string; name: string }[],
  progressByAccountingPeriodId: ReadonlyMap<
    string,
    readonly FundGoalPeriodProgress[]
  >,
): readonly FundGoalMetricTrendPoint[] {
  return accountingPeriods.flatMap((accountingPeriod) => {
    const progress =
      progressByAccountingPeriodId.get(accountingPeriod.id) ?? [];
    const values = progress.flatMap(({ progress: goalProgress }) => {
      switch (metric) {
        case "availableBalance":
          return [
            {
              currentAmount: goalProgress.availableBalance.currentBalance,
              targetAmount: goalProgress.availableBalance.minimumBalance,
              isSatisfied:
                compareCurrencyAmounts(
                  goalProgress.availableBalance.currentBalance,
                  0,
                ) > 0,
            },
          ];
        case "contribution":
          return goalProgress.contribution
            ? [
                {
                  currentAmount: goalProgress.contribution.assignedAmount,
                  targetAmount: goalProgress.contribution.targetAmount,
                  isSatisfied: goalProgress.contribution.isSatisfied,
                },
              ]
            : [];
        case "minimumFundedBalance":
          return goalProgress.fundedBalance?.minimumBalance === null ||
            goalProgress.fundedBalance?.minimumBalance === undefined
            ? []
            : [
                {
                  currentAmount: goalProgress.fundedBalance.balance,
                  targetAmount: goalProgress.fundedBalance.minimumBalance,
                  isSatisfied:
                    goalProgress.fundedBalance.status !==
                    FundedBalanceStatus.BelowMinimum,
                },
              ];
        case "maximumFundedBalance":
          return goalProgress.fundedBalance?.maximumBalance === null ||
            goalProgress.fundedBalance?.maximumBalance === undefined
            ? []
            : [
                {
                  currentAmount: goalProgress.fundedBalance.balance,
                  targetAmount: goalProgress.fundedBalance.maximumBalance,
                  isSatisfied:
                    goalProgress.fundedBalance.status !==
                    FundedBalanceStatus.AboveMaximum,
                },
              ];
        case "endingBalance":
          return goalProgress.endingBalance
            ? [
                {
                  currentAmount: goalProgress.endingBalance.currentBalance,
                  targetAmount: goalProgress.endingBalance.targetBalance,
                  isSatisfied:
                    goalProgress.endingBalance.status ===
                    EndingBalanceStatus.AtTarget,
                },
              ]
            : [];
        default:
          return [];
      }
    });

    return values.length === 0
      ? []
      : [
          {
            accountingPeriodId: accountingPeriod.id,
            accountingPeriodName: accountingPeriod.name,
            currentAmount: sum(values.map((value) => value.currentAmount)),
            targetAmount: sum(values.map((value) => value.targetAmount)),
            configuredGoalCount: values.length,
            satisfiedGoalCount: values.filter((value) => value.isSatisfied)
              .length,
            satisfiedPercentage:
              (values.filter((value) => value.isSatisfied).length /
                values.length) *
              100,
          },
        ];
  });
};

export {
  buildFundGoalMetricTrendPoints,
  fundGoalMetricDefinitions,
  type FundGoalMetric,
  type FundGoalMetricDefinition,
  type FundGoalMetricTrendPoint,
  type FundGoalPeriodProgress,
};
