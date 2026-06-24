import {
  AssignmentGoalSortOrderModel,
  AssignmentGoalTypeModel,
  GoalTrendsBalanceEventSortOrderModel,
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

type CurrentGoals = components["schemas"]["CurrentGoalsModel"];

type CurrentGoal = components["schemas"]["CurrentGoalModel"];

type CurrentGoalProgress = components["schemas"]["CurrentGoalProgressModel"];

type CurrentGoalBalanceEvent =
  components["schemas"]["CurrentGoalBalanceEventModel"];

type GoalTrends = components["schemas"]["GoalTrendsModel"];

type GoalTrendsBalanceEvent =
  components["schemas"]["GoalTrendsBalanceEventModel"];

type GoalTrendsAccountingPeriodSummaryModel =
  components["schemas"]["GoalTrendsAccountingPeriodSummaryModel"];

type GoalTrendsAssignmentGoalTypeSummary =
  components["schemas"]["GoalTrendsAssignmentGoalTypeSummaryModel"];

type GoalTrendsSpendingGoalTypeSummary =
  components["schemas"]["GoalTrendsSpendingGoalTypeSummaryModel"];

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
 * Describes how an assignment goal type behaves.
 */
const describeAssignmentGoalType = function (
  goalType: AssignmentGoalTypeModel,
): string {
  switch (goalType) {
    case AssignmentGoalTypeModel.MonthlyTarget:
      return "Ensure that you always have a certain amount available to spend during the period.";
    case AssignmentGoalTypeModel.RecurringContribution:
      return "Always assign a fixed amount to the fund regardless of the opening balance.";
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

/**
 * Describes how a spending goal type behaves.
 */
const describeSpendingGoalType = function (
  goalType: SpendingGoalTypeModel,
): string {
  switch (goalType) {
    case SpendingGoalTypeModel.Standard:
      return "Treats the fund like a normal spending category. Spending stays on track as long as you do not spend more than the money available in the fund.";
    case SpendingGoalTypeModel.Debt:
      return "Treats the fund like debt payoff. The goal is met only when all money available to the fund has been applied toward the debt.";
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
  type CurrentGoals,
  type CurrentGoal,
  type CurrentGoalBalanceEvent,
  type CurrentGoalProgress,
  SpendingGoalTypeModel as SpendingGoalType,
  type UpdateAssignmentGoalRequest,
  type UpdateSpendingGoalRequest,
  type GoalTrends,
  type GoalTrendsAssignmentGoalTypeSummary,
  type GoalTrendsBalanceEvent,
  GoalTrendsBalanceEventSortOrderModel as GoalTrendsBalanceEventSortOrder,
  type GoalTrendsAccountingPeriodSummaryModel,
  type GoalTrendsSpendingGoalTypeSummary,
  formatAssignmentGoalType,
  describeAssignmentGoalType,
  formatSpendingGoalType,
  describeSpendingGoalType,
};
