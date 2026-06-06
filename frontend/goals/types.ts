import {
  GoalDashboardBalanceEventSortOrderModel,
  GoalDashboardBalanceEventTypeModel,
  GoalSortOrderModel,
  GoalTypeModel,
  type components,
} from "@/framework/data/api";

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

type GoalDashboard = components["schemas"]["GoalDashboardModel"];

type GoalDashboardGoal = components["schemas"]["GoalModel"];

type GoalDashboardBalanceEvent =
  components["schemas"]["GoalDashboardBalanceEventModel"];

type GoalDashboardAccountingPeriodSummaryModel =
  components["schemas"]["GoalDashboardAccountingPeriodSummaryModel"];

type GoalTypeSummary =
  components["schemas"]["GoalDashboardGoalTypeSummaryModel"];

/**
 * Formats the goal type to be displayed.
 */
const formatGoalType = function (goalType: GoalTypeModel): string {
  switch (goalType) {
    case GoalTypeModel.Monthly:
      return "Monthly";
    case GoalTypeModel.Rolling:
      return "Rolling";
    case GoalTypeModel.Savings:
      return "Savings";
    case GoalTypeModel.Debt:
      return "Debt";
    default:
      return String(goalType);
  }
};

export {
  type Goal,
  type GoalDashboard,
  type GoalDashboardGoal,
  type GoalDashboardBalanceEvent,
  type GoalDashboardAccountingPeriodSummaryModel,
  type GoalTypeSummary,
  type CreateGoalRequest,
  type UpdateGoalRequest,
  GoalDashboardBalanceEventSortOrderModel as GoalDashboardBalanceEventSortOrder,
  GoalDashboardBalanceEventTypeModel as GoalDashboardBalanceEventType,
  GoalTypeModel as GoalType,
  GoalSortOrderModel as GoalSortOrder,
  formatGoalType,
};
