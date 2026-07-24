import type { Fund } from "@/funds/types";
import type { FundPlanWithProgress } from "@/fund-plans/types";
import { formatCurrency } from "@/framework/currencyHelpers";
import { isNullOrUndefined } from "@/framework/nullHelpers";
import { isUnassignedFund } from "@/funds/helpers";

/**
 * Represents a draft of a fund assignment.
 */
interface FundAssignmentDraft {
  readonly fundId: string;
  readonly fundName: string;
  readonly amount: number;
  readonly previousFundBalance: number;
  readonly newFundBalance: number;
  readonly previousPlanAmount: number;
  readonly newPlanAmount: number;
}

/**
 * Creates an incomplete assignment draft with empty balance projections.
 */
const createFundAssignmentDraft = function (amount = 0): FundAssignmentDraft {
  return {
    fundId: "",
    fundName: "",
    amount,
    previousFundBalance: 0,
    newFundBalance: 0,
    previousPlanAmount: 0,
    newPlanAmount: 0,
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
      previousPlanAmount: 0,
      newPlanAmount: 0,
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
  return remainingAmount + baselineAssignedAmount;
};

/**
 * Gets the remaining contribution for a Funding Plan.
 */
const getContributionRemainingAmount = function (
  fundId: string,
  fundPlans: FundPlanWithProgress[],
  baselineFundAssignments: readonly FundAssignmentDraft[],
): number | null {
  const fundPlan = fundPlans.find((plan) => plan.fund.id === fundId);
  return getRemainingAmount(
    fundPlan?.progress.contribution?.remainingAmount,
    baselineFundAssignments,
    fundId,
  );
};

/**
 * Gets the remaining contribution amount for a Funding Plan.
 */
const getIncomePlanRemainingAmount = function (
  fundId: string,
  fundPlans: FundPlanWithProgress[],
): number {
  return (
    fundPlans.find((plan) => plan.fund.id === fundId)?.progress.contribution
      ?.remainingAmount ?? 0
  );
};

/**
 * Gets the remaining ending-balance amount for a Funding Plan.
 */
const getSpendingPlanRemainingAmount = function (
  fundId: string,
  fundPlans: FundPlanWithProgress[],
  fundBalance: number,
): number {
  const fundPlan = fundPlans.find((plan) => plan.fund.id === fundId);
  if (fundPlan === undefined) {
    return 0;
  }
  return fundPlan.progress.endingBalance?.variance ?? fundBalance;
};

/**
 * Gets the remaining ending-balance variance for a Funding Plan.
 */
const getEndingBalanceVariance = function (
  fundId: string,
  fundPlans: FundPlanWithProgress[],
  baselineFundAssignments: readonly FundAssignmentDraft[],
  fundBalance: number,
): number | null {
  const fundPlan = fundPlans.find((plan) => plan.fund.id === fundId);
  if (fundPlan === undefined) {
    return null;
  }
  return getRemainingAmount(
    fundPlan.progress.endingBalance?.variance ?? fundBalance,
    baselineFundAssignments,
    fundId,
  );
};

/**
 * Gets the secondary label for the provided fund option.
 */
const getFundOptionSecondaryLabel = function (
  prefix: string,
  planRemainingAmount: number | null,
): string {
  if (planRemainingAmount === null) {
    return "No Funding Plan";
  }
  return `${prefix} ${formatCurrency(planRemainingAmount)}`;
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

export type { FundAssignmentDraft };
export {
  addFundAssignment,
  createFundAssignmentDraft,
  deleteFundAssignment,
  getAssignedFundAmount,
  getAvailableFundCount,
  getExplicitFundAssignments,
  getFundOptionSecondaryLabel,
  getContributionRemainingAmount,
  getEndingBalanceVariance,
  getIncomePlanRemainingAmount,
  getRemainingFundAmount,
  getSpendingPlanRemainingAmount,
  getSuggestedAmount,
  sortFundsByRemainingAmount,
  updateFundAssignment,
  updateUnassignedFundAmount,
};
