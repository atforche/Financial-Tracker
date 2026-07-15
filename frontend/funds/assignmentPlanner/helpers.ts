import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import type { Fund, FundIdentifier } from "@/funds/types";
import formatCurrency from "@/framework/formatCurrency";
import { isUnassignedFund } from "@/funds/helpers";

/**
 * Represents the goal balance of a fund assignment draft.
 */
interface FundAssignmentGoalBalance {
  readonly remainingAmountToAssignIncludingPending: number;
  readonly remainingAmountToSpendIncludingPending: number;
}

/**
 * Represents a draft of a fund assignment.
 */
interface FundAssignmentDraft {
  readonly fundId: string;
  readonly fundName: string;
  readonly amount: number;
  readonly previousFundBalance: number;
  readonly newFundBalance: number;
  readonly previousGoalBalance: FundAssignmentGoalBalance;
  readonly newGoalBalance: FundAssignmentGoalBalance;
}

/**
 * Represents a goal associated with a fund assignment.
 */
interface FundAssignmentGoal {
  readonly fundId: string;
  readonly remainingAmountToAssignIncludingPending?: number;
  readonly remainingAmountToSpendIncludingPending?: number;
}

/**
 * Returns the explicit assignments, excluding the automatic unassigned remainder.
 */
const getExplicitFundAssignments = function (
  fundAssignments: readonly FundAssignmentDraft[],
): FundAssignmentDraft[] {
  return fundAssignments.filter(
    (fundAssignment) => !isUnassignedFund(fundAssignment.fundName),
  );
};

/**
 * Sums the amounts explicitly assigned to named funds.
 */
const getAssignedFundAmount = function (
  fundAssignments: readonly FundAssignmentDraft[],
): number {
  return getExplicitFundAssignments(fundAssignments).reduce(
    (acc, fundAssignment) => acc + fundAssignment.amount,
    0,
  );
};

/**
 * Calculates the amount that remains unassigned after explicit allocations.
 */
const getRemainingFundAmount = function (
  totalAmountToAssign: number | null,
  fundAssignments: readonly FundAssignmentDraft[],
): number | null {
  if (totalAmountToAssign === null) {
    return null;
  }

  return totalAmountToAssign - getAssignedFundAmount(fundAssignments);
};

/**
 * Rebuilds the assignments so the unassigned fund always reflects the remaining amount.
 */
const updateUnassignedFundAmount = function (
  unassignedFund: Fund | null,
  totalAmountToAssign: number | null,
  fundAssignments: readonly FundAssignmentDraft[],
): FundAssignmentDraft[] {
  const explicitFundAssignments = getExplicitFundAssignments(fundAssignments);
  if (totalAmountToAssign === null || unassignedFund === null) {
    return explicitFundAssignments;
  }

  const assignedAmount = explicitFundAssignments.reduce(
    (acc, fundAssignment) => acc + fundAssignment.amount,
    0,
  );

  return [
    {
      fundId: unassignedFund.id,
      fundName: unassignedFund.name,
      amount: Math.max(totalAmountToAssign - assignedAmount, 0),
      previousFundBalance: 0,
      newFundBalance: 0,
      previousGoalBalance: {
        remainingAmountToAssignIncludingPending: 0,
        remainingAmountToSpendIncludingPending: 0,
      },
      newGoalBalance: {
        remainingAmountToAssignIncludingPending: 0,
        remainingAmountToSpendIncludingPending: 0,
      },
    },
    ...explicitFundAssignments,
  ];
};

/**
 * Gets the collection of funds that still can be assigned to.
 */
const getAvailableFundsToAssign = function (
  funds: readonly Fund[],
  fundAssignments: readonly FundAssignmentDraft[],
): Fund[] {
  const explicitAssignments = getExplicitFundAssignments(fundAssignments);
  return funds
    .filter(
      (fund) =>
        !isUnassignedFund(fund.name) &&
        !explicitAssignments.some(
          (assignment) => assignment.fundId === fund.id,
        ),
    )
    .sort((left, right) => left.name.localeCompare(right.name));
};

/**
 * Determines the remaining goal amount for a fund before the current assignment is applied.
 */
const getGoalRemainingAmount = function (
  goalRemainingAmount: number | null | undefined,
  baselineFundAssignments: readonly FundAssignmentDraft[],
  fundId: string,
): number | null {
  if (
    goalRemainingAmount === null ||
    typeof goalRemainingAmount === "undefined"
  ) {
    return null;
  }
  const baselineAssignedAmount =
    baselineFundAssignments.find((assignment) => assignment.fundId === fundId)
      ?.amount ?? 0;
  return goalRemainingAmount + baselineAssignedAmount;
};

/**
 * Gets the remaining amount for an income goal.
 */
const getIncomeGoalRemainingAmount = function (
  fundId: string,
  assignmentGoals: AssignmentGoal[],
  baselineFundAssignments: readonly FundAssignmentDraft[],
): number | null {
  const goal = assignmentGoals.find(
    (assignmentGoal) => assignmentGoal.fund.id === fundId,
  );
  return getGoalRemainingAmount(
    goal?.remainingAmountToAssignIncludingPending,
    baselineFundAssignments,
    fundId,
  );
};

/**
 * Gets the remaining amount for a spending goal.
 */
const getSpendingGoalRemainingAmount = function (
  fundId: string,
  spendingGoals: SpendingGoal[],
  baselineFundAssignments: readonly FundAssignmentDraft[],
): number | null {
  const goal = spendingGoals.find(
    (spendingGoal) => spendingGoal.fund.id === fundId,
  );
  return getGoalRemainingAmount(
    goal?.remainingAmountToSpendIncludingPending,
    baselineFundAssignments,
    fundId,
  );
};

/**
 * Computes the projected remaining amount for a goal after applying a specific assignment amount.
 */
const getProjectedGoalRemainingAmount = function (
  goalRemainingAmount: number | null,
  amount: number,
): number | null {
  if (goalRemainingAmount === null) {
    return null;
  }

  return goalRemainingAmount - amount;
};

/**
 * Gets the secondary label for the provided fund option.
 */
const getFundOptionSecondaryLabel = function (
  prefix: string,
  goalRemainingAmount: number | null,
): string {
  if (goalRemainingAmount === null) {
    return "No goal";
  }

  return `${prefix} ${formatCurrency(goalRemainingAmount)}`;
};

/**
 * Sorts funds by their remaining amount.
 */
const sortFundsByRemainingAmount = function (
  left: FundIdentifier,
  right: FundIdentifier,
  getRemainingAmount: (fundId: string) => number | null,
): number {
  const leftRemainingAmount = getRemainingAmount(left.id);
  const rightRemainingAmount = getRemainingAmount(right.id);

  if (leftRemainingAmount === null && rightRemainingAmount === null) {
    return left.name.localeCompare(right.name);
  }

  if (leftRemainingAmount === null) {
    return 1;
  }

  if (rightRemainingAmount === null) {
    return -1;
  }

  if (leftRemainingAmount !== rightRemainingAmount) {
    return rightRemainingAmount - leftRemainingAmount;
  }

  return left.name.localeCompare(right.name);
};

/**
 * Gets the remaining amount to be assigned for the transaction.
 */
const getTransactionRemainingAmount = function (
  totalAmountToAssign: number | null,
  fundAssignments: readonly FundAssignmentDraft[],
  index: number,
): number {
  const explicitFundAssignments = getExplicitFundAssignments(fundAssignments);
  if (totalAmountToAssign === null) {
    return explicitFundAssignments[index]?.amount ?? 0;
  }

  const amountAssignedElsewhere = explicitFundAssignments.reduce(
    (acc, assignment, assignmentIndex) =>
      assignmentIndex === index ? acc : acc + assignment.amount,
    0,
  );

  return Math.max(totalAmountToAssign - amountAssignedElsewhere, 0);
};

/**
 * Gets the suggested amount to assign to a fund.
 */
const getSuggestedAmount = function (
  totalAmountToAssign: number | null,
  fundAssignments: readonly FundAssignmentDraft[],
  index: number,
  maximumAmount: number | null = null,
): number {
  const transactionRemainingAmount = getTransactionRemainingAmount(
    totalAmountToAssign,
    fundAssignments,
    index,
  );

  if (maximumAmount === null) {
    return transactionRemainingAmount;
  }

  return Math.min(transactionRemainingAmount, Math.max(maximumAmount, 0));
};

export type {
  FundAssignmentDraft,
  FundAssignmentGoal,
  FundAssignmentGoalBalance,
};
export {
  getAssignedFundAmount,
  getAvailableFundsToAssign,
  getExplicitFundAssignments,
  getFundOptionSecondaryLabel,
  getIncomeGoalRemainingAmount,
  getSpendingGoalRemainingAmount,
  getProjectedGoalRemainingAmount,
  getRemainingFundAmount,
  getSuggestedAmount,
  sortFundsByRemainingAmount,
  updateUnassignedFundAmount,
};
