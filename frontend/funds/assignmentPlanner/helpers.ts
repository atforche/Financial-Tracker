import type { Fund, FundWithBalance } from "@/funds/types";
import {
  compareCurrencyAmounts,
  formatCurrency,
  getCurrencyDifference,
  getCurrencyTotal,
  getMaximumCurrencyAmount,
  getMinimumCurrencyAmount,
} from "@/framework/currencyHelpers";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import { isNullOrUndefined } from "@/framework/nullHelpers";
import { isUnassignedFund } from "@/funds/helpers";

/**
 * Represents a draft of a fund assignment.
 */
interface FundAssignmentDraft {
  readonly fundId: string;
  readonly fundName: string;
  readonly amount: number;
  readonly isExtraContribution: boolean;
  readonly previousFundBalance: number;
  readonly newFundBalance: number;
  readonly previousGoalAmount: number;
  readonly newGoalAmount: number;
}

/**
 * Creates an incomplete assignment draft with empty balance projections.
 */
const createFundAssignmentDraft = function (amount = 0): FundAssignmentDraft {
  return {
    fundId: "",
    fundName: "",
    amount,
    isExtraContribution: false,
    previousFundBalance: 0,
    newFundBalance: 0,
    previousGoalAmount: 0,
    newGoalAmount: 0,
  };
};

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
  return getCurrencyTotal(
    getExplicitFundAssignments(fundAssignments).map(
      (fundAssignment) => fundAssignment.amount,
    ),
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

  return getCurrencyTotal([
    totalAmountToAssign,
    -getAssignedFundAmount(fundAssignments),
  ]);
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

  const assignedAmount = getCurrencyTotal(
    explicitFundAssignments.map((fundAssignment) => fundAssignment.amount),
  );

  return [
    {
      fundId: unassignedFund.id,
      fundName: unassignedFund.name,
      amount: getMaximumCurrencyAmount(
        getCurrencyTotal([totalAmountToAssign, -assignedAmount]),
        0,
      ),
      isExtraContribution: false,
      previousFundBalance: 0,
      newFundBalance: 0,
      previousGoalAmount: 0,
      newGoalAmount: 0,
    },
    ...explicitFundAssignments,
  ];
};

/**
 * Gets the collection of funds that still can be assigned to.
 */
const getAvailableFundCount = function (
  funds: readonly Fund[],
  fundAssignments: readonly FundAssignmentDraft[],
): number {
  const assignedFundIds = new Set(
    getExplicitFundAssignments(fundAssignments).map(
      (assignment) => assignment.fundId,
    ),
  );
  return funds.filter(
    (fund) => !isUnassignedFund(fund.name) && !assignedFundIds.has(fund.id),
  ).length;
};

/**
 * Replaces one explicit assignment and restores the automatic unassigned remainder.
 */
const updateFundAssignment = function (
  unassignedFund: Fund | null,
  totalAmountToAssign: number | null,
  fundAssignments: readonly FundAssignmentDraft[],
  index: number,
  update: (assignment: FundAssignmentDraft) => FundAssignmentDraft,
): FundAssignmentDraft[] {
  const nextAssignments = getExplicitFundAssignments(fundAssignments).map(
    (assignment, assignmentIndex) =>
      assignmentIndex === index ? update(assignment) : assignment,
  );
  return updateUnassignedFundAmount(
    unassignedFund,
    totalAmountToAssign,
    nextAssignments,
  );
};

/**
 * Removes one explicit assignment and restores the automatic unassigned remainder.
 */
const deleteFundAssignment = function (
  unassignedFund: Fund | null,
  totalAmountToAssign: number | null,
  fundAssignments: readonly FundAssignmentDraft[],
  index: number,
): FundAssignmentDraft[] {
  const nextAssignments = getExplicitFundAssignments(fundAssignments).filter(
    (_, assignmentIndex) => assignmentIndex !== index,
  );
  return updateUnassignedFundAmount(
    unassignedFund,
    totalAmountToAssign,
    nextAssignments,
  );
};

/**
 * Determines the remaining amount for a fund before the current assignment is applied.
 */
const getRemainingAmount = function (
  remainingAmount: number | null | undefined,
  baselineFundAssignments: readonly FundAssignmentDraft[],
  fundId: string,
): number | null {
  if (isNullOrUndefined(remainingAmount)) {
    return null;
  }
  const baselineAssignedAmount =
    baselineFundAssignments.find((assignment) => assignment.fundId === fundId)
      ?.amount ?? 0;
  return getCurrencyTotal([remainingAmount, baselineAssignedAmount]);
};

/**
 * Gets the remaining contribution for a Fund Goal.
 */
const getContributionRemainingAmount = function (
  fundId: string,
  fundGoals: FundGoalWithProgress[],
  baselineFundAssignments: readonly FundAssignmentDraft[],
): number | null {
  const fundGoal = fundGoals.find((goal) => goal.fund.id === fundId);
  return getRemainingAmount(
    fundGoal?.progress.contribution?.remainingAmount,
    baselineFundAssignments.filter(
      (assignment) => !assignment.isExtraContribution,
    ),
    fundId,
  );
};

/**
 * Gets the remaining contribution amount for a Fund Goal.
 */
const getIncomeGoalRemainingAmount = function (
  fundId: string,
  fundGoals: FundGoalWithProgress[],
): number {
  return (
    fundGoals.find((fundGoal) => fundGoal.fund.id === fundId)?.progress
      .contribution?.remainingAmount ?? 0
  );
};

/**
 * Gets the remaining ending-balance amount for a Fund Goal.
 */
const getSpendingGoalRemainingAmount = function (
  fundId: string,
  fundGoals: FundGoalWithProgress[],
  fundBalance: number,
): number {
  const fundGoal = fundGoals.find((goal) => goal.fund.id === fundId);
  const targetEndingBalance = fundGoal?.targetEndingBalance;
  if (targetEndingBalance === null || targetEndingBalance === undefined) {
    return fundBalance;
  }
  return getCurrencyDifference(fundBalance, targetEndingBalance);
};

/**
 * Gets the remaining ending-balance variance for a Fund Goal.
 */
const getEndingBalanceVariance = function (
  fundId: string,
  fundGoals: FundGoalWithProgress[],
  baselineFundAssignments: readonly FundAssignmentDraft[],
  fundBalance: number,
): number | null {
  const fundGoal = fundGoals.find((goal) => goal.fund.id === fundId);
  if (fundGoal === undefined) {
    return null;
  }
  return getRemainingAmount(
    getSpendingGoalRemainingAmount(fundId, fundGoals, fundBalance),
    baselineFundAssignments,
    fundId,
  );
};

/**
 * Gets the secondary label for the provided fund option.
 */
const getFundOptionSecondaryLabel = function (
  prefix: string,
  goalRemainingAmount: number | null,
): string {
  if (goalRemainingAmount === null) {
    return "No Fund Goal";
  }
  return `${prefix} ${formatCurrency(goalRemainingAmount)}`;
};

/**
 * Sorts funds by their remaining amount.
 */
const sortFundsByRemainingAmount = function (
  left: Fund,
  right: Fund,
  getFundRemainingAmount: (fundId: string) => number | null,
): number {
  const leftRemainingAmount = getFundRemainingAmount(left.id);
  const rightRemainingAmount = getFundRemainingAmount(right.id);

  if (leftRemainingAmount === null && rightRemainingAmount === null) {
    return left.name.localeCompare(right.name);
  }

  if (leftRemainingAmount === null) {
    return 1;
  }

  if (rightRemainingAmount === null) {
    return -1;
  }

  if (compareCurrencyAmounts(leftRemainingAmount, rightRemainingAmount) !== 0) {
    return getCurrencyDifference(rightRemainingAmount, leftRemainingAmount);
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

  const amountAssignedElsewhere = getCurrencyTotal(
    explicitFundAssignments.flatMap((assignment, assignmentIndex) =>
      assignmentIndex === index ? [] : [assignment.amount],
    ),
  );

  return getMaximumCurrencyAmount(
    getCurrencyTotal([totalAmountToAssign, -amountAssignedElsewhere]),
    0,
  );
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

  return getMinimumCurrencyAmount(
    transactionRemainingAmount,
    getMaximumCurrencyAmount(maximumAmount, 0),
  );
};

/**
 * Appends an empty explicit assignment with the remaining transaction amount.
 */
const addFundAssignment = function (
  unassignedFund: Fund | null,
  totalAmountToAssign: number | null,
  fundAssignments: readonly FundAssignmentDraft[],
): FundAssignmentDraft[] {
  const explicitAssignments = getExplicitFundAssignments(fundAssignments);
  return updateUnassignedFundAmount(unassignedFund, totalAmountToAssign, [
    ...explicitAssignments,
    createFundAssignmentDraft(
      getSuggestedAmount(
        totalAmountToAssign,
        explicitAssignments,
        explicitAssignments.length,
      ),
    ),
  ]);
};

/**
 * Creates regular income assignments in order of largest remaining contribution.
 */
const autoAssignIncomeFundAssignments = function (
  totalAmountToAssign: number | null,
  funds: readonly FundWithBalance[],
  fundGoals: FundGoalWithProgress[],
  baselineFundAssignments: readonly FundAssignmentDraft[],
  unassignedFund: Fund | null,
): FundAssignmentDraft[] {
  if (
    totalAmountToAssign === null ||
    compareCurrencyAmounts(totalAmountToAssign, 0) <= 0
  ) {
    return updateUnassignedFundAmount(unassignedFund, totalAmountToAssign, []);
  }

  const eligibleFunds = funds.filter((fund) => {
    if (isUnassignedFund(fund.name)) {
      return false;
    }
    const remainingAmount = getContributionRemainingAmount(
      fund.id,
      fundGoals,
      baselineFundAssignments,
    );
    return (
      remainingAmount !== null && compareCurrencyAmounts(remainingAmount, 0) > 0
    );
  });

  const sortedFunds = [...eligibleFunds].sort((left, right) =>
    sortFundsByRemainingAmount(left, right, (fundId) =>
      getContributionRemainingAmount(
        fundId,
        fundGoals,
        baselineFundAssignments,
      ),
    ),
  );
  let remainingAmount = totalAmountToAssign;
  const assignments: FundAssignmentDraft[] = [];

  for (const fund of sortedFunds) {
    if (compareCurrencyAmounts(remainingAmount, 0) <= 0) {
      break;
    }

    const previousFundBalance = fund.currentBalance.postedBalance;
    const previousGoalAmount =
      getContributionRemainingAmount(
        fund.id,
        fundGoals,
        baselineFundAssignments,
      ) ?? 0;
    const amount = getMinimumCurrencyAmount(
      remainingAmount,
      previousGoalAmount,
    );

    assignments.push({
      fundId: fund.id,
      fundName: fund.name,
      amount,
      isExtraContribution: false,
      previousFundBalance,
      newFundBalance: getCurrencyTotal([previousFundBalance, amount]),
      previousGoalAmount,
      newGoalAmount: getMaximumCurrencyAmount(
        getCurrencyTotal([previousGoalAmount, -amount]),
        0,
      ),
    });
    remainingAmount = getCurrencyTotal([remainingAmount, -amount]);
  }

  return updateUnassignedFundAmount(
    unassignedFund,
    totalAmountToAssign,
    assignments,
  );
};

export type { FundAssignmentDraft };
export {
  addFundAssignment,
  autoAssignIncomeFundAssignments,
  createFundAssignmentDraft,
  deleteFundAssignment,
  getAssignedFundAmount,
  getAvailableFundCount,
  getExplicitFundAssignments,
  getFundOptionSecondaryLabel,
  getContributionRemainingAmount,
  getEndingBalanceVariance,
  getIncomeGoalRemainingAmount,
  getRemainingFundAmount,
  getSpendingGoalRemainingAmount,
  getSuggestedAmount,
  sortFundsByRemainingAmount,
  updateFundAssignment,
  updateUnassignedFundAmount,
};
