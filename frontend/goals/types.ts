import {
  AssignmentGoalSortOrderModel,
  AssignmentGoalTypeModel,
  GoalDashboardBalanceEventSortOrderModel,
  SpendingGoalSortOrderModel,
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

type GoalDashboard = components["schemas"]["GoalDashboardModel"];

type GoalDashboardBalanceEvent =
  components["schemas"]["GoalDashboardBalanceEventModel"];

type GoalDashboardAccountingPeriodSummaryModel =
  components["schemas"]["GoalDashboardAccountingPeriodSummaryModel"];

type GoalDashboardAssignmentGoalTypeSummary =
  components["schemas"]["GoalDashboardAssignmentGoalTypeSummaryModel"];

type GoalDashboardSpendingGoalTypeSummary =
  components["schemas"]["GoalDashboardSpendingGoalTypeSummaryModel"];

/**
 * Formats an assignment goal type for display.
 */
const formatAssignmentGoalType = function (
  goalType: AssignmentGoalTypeModel,
): string {
  switch (goalType) {
    case AssignmentGoalTypeModel.MonthlyTarget:
      return "Monthly Target";
    case AssignmentGoalTypeModel.RecurringContribution:
      return "Recurring Contribution";
    default:
      return String(goalType);
  }
};

/**
 * Formats a spending goal type for display.
 */
const formatSpendingGoalType = function (
  goalType: SpendingGoalTypeModel,
): string {
  switch (goalType) {
    case SpendingGoalTypeModel.Standard:
      return "Standard";
    case SpendingGoalTypeModel.Debt:
      return "Debt";
    default:
      return String(goalType);
  }
};

export {
  type AssignmentGoal,
  AssignmentGoalSortOrderModel as AssignmentGoalSortOrder,
  AssignmentGoalTypeModel as AssignmentGoalType,
  type SpendingGoal,
  SpendingGoalSortOrderModel as SpendingGoalSortOrder,
  SpendingGoalTypeModel as SpendingGoalType,
  type UpdateAssignmentGoalRequest,
  type UpdateSpendingGoalRequest,
  type GoalDashboard,
  type GoalDashboardAssignmentGoalTypeSummary,
  type GoalDashboardBalanceEvent,
  GoalDashboardBalanceEventSortOrderModel as GoalDashboardBalanceEventSortOrder,
  type GoalDashboardAccountingPeriodSummaryModel,
  type GoalDashboardSpendingGoalTypeSummary,
  formatAssignmentGoalType,
  formatSpendingGoalType,
};
