import type {
  AssignmentGoal,
  AssignmentGoalSort,
  AssignmentGoalType,
  GoalBalanceEvent,
  SpendingGoal,
  SpendingGoalSort,
  SpendingGoalType,
} from "@/goals/types";

type GoalTrendsView = "assignment" | "spending";

type GoalTrendsGoal = AssignmentGoal | SpendingGoal;

type GoalTrendsSortOrder = AssignmentGoalSort | SpendingGoalSort;

type GoalTrendsGoalType = AssignmentGoalType | SpendingGoalType;

const defaultGoalTrendsView: GoalTrendsView = "assignment";

interface GoalsMetSummary {
  readonly metCount: number;
  readonly totalCount: number;
  readonly percentageMet: number;
}

interface AssignmentGoalTypeSummary {
  readonly assignmentGoalType: AssignmentGoalType;
  readonly totalAmountToAssign: number;
  readonly totalAmountAssigned: number;
  readonly percentageOfGoalsMet: GoalsMetSummary;
}

interface SpendingGoalTypeSummary {
  readonly spendingGoalType: SpendingGoalType;
  readonly totalAmountToSpend: number;
  readonly totalAmountSpent: number;
  readonly percentageOfGoalsMet: GoalsMetSummary;
}

interface GoalAccountingPeriodSummary {
  readonly accountingPeriodId: string;
  readonly accountingPeriodName: string;
  readonly totalAmountToAssign: number;
  readonly totalAmountAssigned: number;
  readonly percentageOfAssignmentGoalsMet: GoalsMetSummary;
  readonly totalAmountToSpend: number;
  readonly totalAmountSpent: number;
  readonly percentageOfSpendingGoalsMet: GoalsMetSummary;
}

interface GoalRangeSummary {
  readonly totalAmountToAssign: number;
  readonly totalAmountAssigned: number;
  readonly percentageOfAssignmentGoalsMet: GoalsMetSummary;
  readonly assignmentGoalTypes: readonly AssignmentGoalTypeSummary[];
  readonly totalAmountToSpend: number;
  readonly totalAmountSpent: number;
  readonly percentageOfSpendingGoalsMet: GoalsMetSummary;
  readonly spendingGoalTypes: readonly SpendingGoalTypeSummary[];
}

/**
 * Determines whether the provided URL value is a supported goal trends view.
 */
const isGoalTrendsView = function (
  value: string | null | undefined,
): value is GoalTrendsView {
  return value === "assignment" || value === "spending";
};

export type {
  AssignmentGoalTypeSummary,
  GoalAccountingPeriodSummary,
  GoalRangeSummary,
  GoalsMetSummary,
  GoalBalanceEvent,
  GoalTrendsGoal,
  GoalTrendsGoalType,
  GoalTrendsSortOrder,
  GoalTrendsView,
  SpendingGoalTypeSummary,
};
export { defaultGoalTrendsView, isGoalTrendsView };
