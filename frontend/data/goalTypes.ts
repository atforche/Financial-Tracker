import {
  AccountingPeriodGoalSortOrderModel,
  GoalTypeModel,
  type components,
} from "@/data/api";

/**
 * Type representing a Goal.
 */
type Goal = components["schemas"]["GoalModel"];

/**
 * Type representing a request to create a Goal.
 */
type CreateGoalRequest = components["schemas"]["CreateGoalModel"];

/**
 * Type representing a request to update a Goal.
 */
type UpdateGoalRequest = components["schemas"]["UpdateGoalModel"];

export {
  type Goal,
  type CreateGoalRequest,
  type UpdateGoalRequest,
  AccountingPeriodGoalSortOrderModel as AccountingPeriodGoalSortOrder,
  GoalTypeModel as GoalType,
};
