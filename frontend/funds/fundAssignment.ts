import type { Fund, FundAmount } from "@/funds/types";

/**
 * Gets the special unassigned fund when it exists.
 */
const getUnassignedFund = function (funds: readonly Fund[]): Fund | null {
  return funds.find((fund) => fund.name === "Unassigned") ?? null;
};

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

  return Math.max(
    totalAmountToAssign - getAssignedFundAmount(unassignedFund, fundAmounts),
    0,
  );
};

/**
 * Determines whether the current explicit allocations exceed the transaction total.
 */
const hasOverAllocatedFundAssignments = function (
  unassignedFund: Fund | null,
  totalAmountToAssign: number | null,
  fundAmounts: readonly FundAmount[],
): boolean {
  if (totalAmountToAssign === null) {
    return false;
  }

  return (
    getAssignedFundAmount(unassignedFund, fundAmounts) > totalAmountToAssign
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

export {
  getAssignedFundAmount,
  getExplicitFundAssignments,
  getRemainingFundAmount,
  getUnassignedFund,
  hasOverAllocatedFundAssignments,
  updateUnassignedFundAmount,
};
