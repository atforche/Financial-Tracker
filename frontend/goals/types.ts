import {
  AssignmentGoalSortModel,
  AssignmentGoalTypeModel,
  GoalBalanceEventSortModel,
  SpendingGoalSortModel,
  SpendingGoalTypeModel,
  type components,
} from "@/framework/data/api";

/**
 * Type representing an Assignment Goal.
 */
type AssignmentGoal = components["schemas"]["AssignmentGoalModel"];

/**
 * Type representing a Spending Goal.
 */
type SpendingGoal = components["schemas"]["SpendingGoalModel"];

/**
 * Type representing a request to update an Assignment Goal.
 */
type UpdateAssignmentGoalRequest =
  components["schemas"]["UpdateAssignmentGoalModel"];

/**
 * Type representing a request to update a Spending Goal.
 */
type UpdateSpendingGoalRequest =
  components["schemas"]["UpdateSpendingGoalModel"];

/**
 * Type representing a balance event for a goal.
 */
type GoalBalanceEvent =
  components["schemas"]["GoalBalanceEventModel"];

export {
  type AssignmentGoal,
  AssignmentGoalSortModel as AssignmentGoalSort,
  AssignmentGoalTypeModel as AssignmentGoalType,
  type SpendingGoal,
  SpendingGoalSortModel as SpendingGoalSort,
  SpendingGoalTypeModel as SpendingGoalType,
  type UpdateAssignmentGoalRequest,
  type UpdateSpendingGoalRequest,
  type GoalBalanceEvent,
  GoalBalanceEventSortModel as GoalBalanceEventSort,
};
