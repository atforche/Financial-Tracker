import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import type { Fund, FundAmount, FundIdentifier } from "@/funds/types";
import formatCurrency from "@/framework/formatCurrency";
import { getUnassignedFund } from "@/funds/helpers";

/**
 * Returns the explicit assignments, excluding the automatic unassigned remainder.
 */
const getExplicitFundAssignments = function (
  unassignedFund: Fund | null,
  fundAmounts: readonly FundAmount[],
): FundAmount[] {
  return fundAmounts.filter(
    (fundAmount) =>
      fundAmount.fundId !== unassignedFund?.id &&
      fundAmount.fundName !== "Unassigned",
  );
};

/**
 * Sums the amounts explicitly assigned to named funds.
 */
const getAssignedFundAmount = function (
  unassignedFund: Fund | null,
  fundAmounts: readonly FundAmount[],
): number {
  return getExplicitFundAssignments(unassignedFund, fundAmounts).reduce(
    (acc, fundAmount) => acc + fundAmount.amount,
    0,
  );
};

/**
 * Calculates the amount that remains unassigned after explicit allocations.
 */
const getRemainingFundAmount = function (
  unassignedFund: Fund | null,
  totalAmountToAssign: number | null,
  fundAmounts: readonly FundAmount[],
): number | null {
  if (totalAmountToAssign === null) {
    return null;
  }
  return (
    totalAmountToAssign - getAssignedFundAmount(unassignedFund, fundAmounts)
  );
};

/**
 * Rebuilds the assignments so the unassigned fund always reflects the remaining amount.
 */
const updateUnassignedFundAmount = function (
  unassignedFund: Fund | null,
  totalAmountToAssign: number | null,
  fundAmounts: readonly FundAmount[],
): FundAmount[] {
  const assignedFundAmounts = getExplicitFundAssignments(
    unassignedFund,
    fundAmounts,
  );
  if (totalAmountToAssign === null || unassignedFund === null) {
    return assignedFundAmounts;
  }
  const assignedAmount = assignedFundAmounts.reduce(
    (acc, fundAmount) => acc + fundAmount.amount,
    0,
  );
  return [
    {
      fundId: unassignedFund.id,
      fundName: unassignedFund.name,
      amount: Math.max(totalAmountToAssign - assignedAmount, 0),
    },
    ...assignedFundAmounts,
  ];
};

/**
 * Gets the collection of funds that still can be assigned to.
 */
const getAvailableFundsToAssign = function (
  funds: readonly Fund[],
  fundAmounts: readonly FundAmount[],
): Fund[] {
  const unassignedFund = getUnassignedFund(funds);
  const explicitAssignments = getExplicitFundAssignments(
    unassignedFund,
    fundAmounts,
  );
  return funds
    .filter(
      (fund) =>
        fund.id !== unassignedFund?.id &&
        !explicitAssignments.some(
          (assignment) => assignment.fundId === fund.id,
        ),
    )
    .sort((left, right) => left.name.localeCompare(right.name));
};

/**
 * Determines the remaining amount for a goal before the current assignment is applied.
 */
const getGoalRemainingBeforeCurrentAssignment = function (
  tone: "income" | "spending",
  fundId: string,
  assignmentGoals: readonly AssignmentGoal[],
  spendingGoals: readonly SpendingGoal[],
  baselineValue: readonly FundAmount[],
): number | null {
  const goal =
    tone === "income"
      ? assignmentGoals.find(
          (assignmentGoal) => assignmentGoal.fundId === fundId,
        )
      : spendingGoals.find((spendingGoal) => spendingGoal.fundId === fundId);
  if (typeof goal === "undefined") {
    return null;
  }
  const baselineAssignedAmount =
    baselineValue.find((assignment) => assignment.fundId === fundId)?.amount ??
    0;
  return "remainingAmountToAssignIncludingPending" in goal
    ? goal.remainingAmountToAssignIncludingPending + baselineAssignedAmount
    : goal.remainingAmountToSpendIncludingPending + baselineAssignedAmount;
};

/**
 * Computes the projected remaining amount for a goal after applying a specific assignment amount.
 */
const getProjectedGoalRemainingAmount = function (
  tone: "income" | "spending",
  fundId: string,
  assignmentGoals: readonly AssignmentGoal[],
  spendingGoals: readonly SpendingGoal[],
  baselineValue: readonly FundAmount[],
  amount: number,
): number | null {
  const goalRemainingBeforeCurrentAssignment =
    getGoalRemainingBeforeCurrentAssignment(
      tone,
      fundId,
      assignmentGoals,
      spendingGoals,
      baselineValue,
    );
  if (goalRemainingBeforeCurrentAssignment === null) {
    return null;
  }
  return goalRemainingBeforeCurrentAssignment - amount;
};

/**
 * Gets the secondary label for the provided fund option.
 */
const getFundOptionSecondaryLabel = function (
  tone: "income" | "spending",
  fundId: string,
  assignmentGoals: readonly AssignmentGoal[],
  spendingGoals: readonly SpendingGoal[],
  baselineValue: readonly FundAmount[],
): string | null {
  const goalRemainingAmount = getGoalRemainingBeforeCurrentAssignment(
    tone,
    fundId,
    assignmentGoals,
    spendingGoals,
    baselineValue,
  );
  if (goalRemainingAmount === null) {
    return "No goal";
  }
  return tone === "income"
    ? `Remaining to assign ${formatCurrency(goalRemainingAmount)}`
    : `Remaining to spend ${formatCurrency(goalRemainingAmount)}`;
};

/**
 * Sorts funds by their remaining amount.
 */
const sortFundsByRemainingAmount = function (
  tone: "income" | "spending",
  left: FundIdentifier,
  right: FundIdentifier,
  assignmentGoals: readonly AssignmentGoal[],
  spendingGoals: readonly SpendingGoal[],
  baselineValue: readonly FundAmount[],
): number {
  const leftRemainingAmount = getGoalRemainingBeforeCurrentAssignment(
    tone,
    left.id,
    assignmentGoals,
    spendingGoals,
    baselineValue,
  );
  const rightRemainingAmount = getGoalRemainingBeforeCurrentAssignment(
    tone,
    right.id,
    assignmentGoals,
    spendingGoals,
    baselineValue,
  );
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
  value: FundAmount[],
  index: number,
  unassignedFund: Fund | null,
): number {
  const explicitFundAssignments = getExplicitFundAssignments(
    unassignedFund,
    value,
  );
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
 * Gets the suggested amount to assign to a fund based on the remaining transaction amount and the fund's goal.
 */
const getSuggestedAmount = function (
  tone: "income" | "spending",
  funds: Fund[],
  value: FundAmount[],
  assignmentGoals: readonly AssignmentGoal[],
  spendingGoals: readonly SpendingGoal[],
  baselineValue: readonly FundAmount[],
  index: number,
  fundId: string,
  totalAmountToAssign: number | null,
): number {
  const unassignedFund = getUnassignedFund(funds);
  const transactionRemainingAmount = getTransactionRemainingAmount(
    totalAmountToAssign,
    value,
    index,
    unassignedFund,
  );
  if (tone === "income" && typeof fundId === "string" && fundId !== "") {
    const goalRemainingAmount = getGoalRemainingBeforeCurrentAssignment(
      tone,
      fundId,
      assignmentGoals,
      spendingGoals,
      baselineValue,
    );
    if (goalRemainingAmount !== null) {
      return Math.min(
        transactionRemainingAmount,
        Math.max(goalRemainingAmount, 0),
      );
    }
  }
  return transactionRemainingAmount;
};

export {
  getAssignedFundAmount,
  getExplicitFundAssignments,
  getRemainingFundAmount,
  updateUnassignedFundAmount,
  getAvailableFundsToAssign,
  getSuggestedAmount,
  getProjectedGoalRemainingAmount,
  getFundOptionSecondaryLabel,
  sortFundsByRemainingAmount,
};
