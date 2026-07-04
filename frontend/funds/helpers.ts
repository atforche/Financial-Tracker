import type { Fund, FundAmount } from "@/funds/types";

/**
 * Determines if a given fund name corresponds to the special "Unassigned" fund.
 */
const isUnassignedFund = function (fundName: string): boolean {
  return fundName === "Unassigned";
};

/**
 * Gets the special unassigned fund when it exists.
 */
const getUnassignedFund = function (funds: readonly Fund[]): Fund | null {
  return funds.find((fund) => isUnassignedFund(fund.name)) ?? null;
};

/**
 * Determines whether any fund assignments are incomplete.
 */
const hasIncompleteFundAssignments = function (
  fundAssignments: FundAmount[],
): boolean {
  return fundAssignments.some(
    (fundAmount) =>
      fundAmount.fundId === "" ||
      fundAmount.fundName === "" ||
      fundAmount.amount < 0 ||
      (fundAmount.amount === 0 && !isUnassignedFund(fundAmount.fundName)),
  );
};

export { isUnassignedFund, hasIncompleteFundAssignments, getUnassignedFund };
