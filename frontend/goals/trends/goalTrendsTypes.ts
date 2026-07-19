import type {
  AssignmentGoalType,
  GoalBalanceEvent,
  SpendingGoalType,
} from "@/goals/types";

/**
 * Types used in the goal trends feature.
 */
type GoalTrendsView = "assignment" | "spending";

/**
 * Represents a goal in the goal trends feature, which can be either an assignment goal or a spending goal.
 */

/**
 * Represents the type of a goal in the goal trends feature, which can be either an assignment goal type or a spending goal type.
 */
type GoalTrendsGoalType = AssignmentGoalType | SpendingGoalType;

/**
 * The default view for the goal trends feature, which is "assignment".
 */
const defaultGoalTrendsView: GoalTrendsView = "assignment";

/**
 * Represents a summary of goals met.
 */
interface GoalsMetSummary {
  readonly metCount: number;
  readonly totalCount: number;
  readonly percentageMet: number;
}

/**
 * Represents a summary of an assignment goal type.
 */
interface AssignmentGoalTypeSummary {
  readonly assignmentGoalType: AssignmentGoalType;
  readonly totalAmountToAssign: number;
  readonly totalAmountAssigned: number;
  readonly percentageOfGoalsMet: GoalsMetSummary;
}

/**
 * Represents a summary of a spending goal type.
 */
interface SpendingGoalTypeSummary {
  readonly spendingGoalType: SpendingGoalType;
  readonly totalAmountToSpend: number;
  readonly totalAmountSpent: number;
  readonly percentageOfGoalsMet: GoalsMetSummary;
}

/**
 * Represents a summary of an accounting period for goals.
 */
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

/**
 * Represents a summary of goals over a range of accounting periods.
 */
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
  GoalTrendsGoalType,
  GoalTrendsView,
  SpendingGoalTypeSummary,
};
export { defaultGoalTrendsView, isGoalTrendsView };
