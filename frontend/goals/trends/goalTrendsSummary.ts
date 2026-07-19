import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import type {
  AssignmentGoalTypeSummary,
  GoalAccountingPeriodSummary,
  GoalRangeSummary,
  GoalsMetSummary,
  SpendingGoalTypeSummary,
} from "@/goals/trends/goalTrendsTypes";

/**
 * Gets the summary of goals met.
 */
const getGoalsMetSummary = function (
  goals: readonly { readonly isGoalMet: boolean }[],
): GoalsMetSummary {
  const metCount = goals.filter((goal) => goal.isGoalMet).length;
  return {
    metCount,
    totalCount: goals.length,
    percentageMet: goals.length === 0 ? 0 : (metCount / goals.length) * 100,
  };
};

/**
 * Summarizes the assignment goal types.
 */
const summarizeAssignmentGoalTypes = function (
  goals: readonly AssignmentGoal[],
): AssignmentGoalTypeSummary[] {
  return Array.from(new Set(goals.map((goal) => goal.type))).map((type) => {
    const matchingGoals = goals.filter((goal) => goal.type === type);
    return {
      assignmentGoalType: type,
      totalAmountToAssign: matchingGoals.reduce(
        (total, goal) => total + goal.totalAmountToAssign,
        0,
      ),
      totalAmountAssigned: matchingGoals.reduce(
        (total, goal) => total + goal.totalAmountAssigned,
        0,
      ),
      percentageOfGoalsMet: getGoalsMetSummary(matchingGoals),
    };
  });
};

/**
 * Summarizes the spending goal types.
 */
const summarizeSpendingGoalTypes = function (
  goals: readonly SpendingGoal[],
): SpendingGoalTypeSummary[] {
  return Array.from(new Set(goals.map((goal) => goal.type))).map((type) => {
    const matchingGoals = goals.filter((goal) => goal.type === type);
    return {
      spendingGoalType: type,
      totalAmountToSpend: matchingGoals.reduce(
        (total, goal) => total + goal.totalAmountToSpend,
        0,
      ),
      totalAmountSpent: matchingGoals.reduce(
        (total, goal) => total + goal.totalAmountSpent,
        0,
      ),
      percentageOfGoalsMet: getGoalsMetSummary(matchingGoals),
    };
  });
};

/**
 * Summarizes assignment and spending goals across a requested range.
 */
const summarizeGoalRange = function (
  assignmentGoals: readonly AssignmentGoal[],
  spendingGoals: readonly SpendingGoal[],
): GoalRangeSummary {
  return {
    totalAmountToAssign: assignmentGoals.reduce(
      (total, goal) => total + goal.totalAmountToAssign,
      0,
    ),
    totalAmountAssigned: assignmentGoals.reduce(
      (total, goal) => total + goal.totalAmountAssigned,
      0,
    ),
    percentageOfAssignmentGoalsMet: getGoalsMetSummary(assignmentGoals),
    assignmentGoalTypes: summarizeAssignmentGoalTypes(assignmentGoals),
    totalAmountToSpend: spendingGoals.reduce(
      (total, goal) => total + goal.totalAmountToSpend,
      0,
    ),
    totalAmountSpent: spendingGoals.reduce(
      (total, goal) => total + goal.totalAmountSpent,
      0,
    ),
    percentageOfSpendingGoalsMet: getGoalsMetSummary(spendingGoals),
    spendingGoalTypes: summarizeSpendingGoalTypes(spendingGoals),
  };
};

/**
 * Summarizes assignment and spending goals for each Accounting Period.
 */
const summarizeGoalsByAccountingPeriod = function (
  accountingPeriods: readonly { readonly id: string; readonly name: string }[],
  assignmentGoals: readonly AssignmentGoal[],
  spendingGoals: readonly SpendingGoal[],
): GoalAccountingPeriodSummary[] {
  const assignmentGoalsByPeriod = new Map<string, AssignmentGoal[]>();
  const spendingGoalsByPeriod = new Map<string, SpendingGoal[]>();
  assignmentGoals.forEach((goal) => {
    if (goal.accountingPeriod !== null) {
      const goals = assignmentGoalsByPeriod.get(goal.accountingPeriod.id) ?? [];
      goals.push(goal);
      assignmentGoalsByPeriod.set(goal.accountingPeriod.id, goals);
    }
  });
  spendingGoals.forEach((goal) => {
    if (goal.accountingPeriod !== null) {
      const goals = spendingGoalsByPeriod.get(goal.accountingPeriod.id) ?? [];
      goals.push(goal);
      spendingGoalsByPeriod.set(goal.accountingPeriod.id, goals);
    }
  });

  return accountingPeriods.map((accountingPeriod) => {
    const periodAssignmentGoals =
      assignmentGoalsByPeriod.get(accountingPeriod.id) ?? [];
    const periodSpendingGoals =
      spendingGoalsByPeriod.get(accountingPeriod.id) ?? [];
    const summary = summarizeGoalRange(
      periodAssignmentGoals,
      periodSpendingGoals,
    );
    return {
      accountingPeriodId: accountingPeriod.id,
      accountingPeriodName: accountingPeriod.name,
      totalAmountToAssign: summary.totalAmountToAssign,
      totalAmountAssigned: summary.totalAmountAssigned,
      percentageOfAssignmentGoalsMet: summary.percentageOfAssignmentGoalsMet,
      totalAmountToSpend: summary.totalAmountToSpend,
      totalAmountSpent: summary.totalAmountSpent,
      percentageOfSpendingGoalsMet: summary.percentageOfSpendingGoalsMet,
    };
  });
};

export { summarizeGoalRange, summarizeGoalsByAccountingPeriod };
