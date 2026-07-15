import { AssignmentGoalType, SpendingGoalType } from "@/goals/types";

/**
 * Formats an assignment goal type for display.
 */
const formatAssignmentGoalType = function (
  goalType: AssignmentGoalType,
): string {
  switch (goalType) {
    case AssignmentGoalType.MonthlyTarget:
      return "Monthly Target";
    case AssignmentGoalType.RecurringContribution:
      return "Recurring Contribution";
    default:
      return String(goalType);
  }
};

/**
 * Describes how an assignment goal type behaves.
 */
const describeAssignmentGoalType = function (
  goalType: AssignmentGoalType,
): string {
  switch (goalType) {
    case AssignmentGoalType.MonthlyTarget:
      return "Ensure that you always have a certain amount available to spend during the period.";
    case AssignmentGoalType.RecurringContribution:
      return "Always assign a fixed amount to the fund regardless of the opening balance.";
    default:
      return String(goalType);
  }
};

/**
 * Formats a spending goal type for display.
 */
const formatSpendingGoalType = function (goalType: SpendingGoalType): string {
  switch (goalType) {
    case SpendingGoalType.Standard:
      return "Standard";
    case SpendingGoalType.Debt:
      return "Debt";
    default:
      return String(goalType);
  }
};

/**
 * Describes how a spending goal type behaves.
 */
const describeSpendingGoalType = function (goalType: SpendingGoalType): string {
  switch (goalType) {
    case SpendingGoalType.Standard:
      return "Treats the fund like a normal spending category. Spending stays on track as long as you do not spend more than the money available in the fund.";
    case SpendingGoalType.Debt:
      return "Treats the fund like debt payoff. The goal is met only when all money available to the fund has been applied toward the debt.";
    default:
      return String(goalType);
  }
};

export {
  formatAssignmentGoalType,
  describeAssignmentGoalType,
  formatSpendingGoalType,
  describeSpendingGoalType,
};
