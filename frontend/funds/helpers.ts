import type { Fund, FundAmount } from "@/funds/types";

/**
 * Gets the special unassigned fund when it exists.
 */
const getUnassignedFund = function (funds: readonly Fund[]): Fund | null {
  return funds.find((fund) => fund.name === "Unassigned") ?? null;
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
      (fundAmount.amount === 0 && fundAmount.fundName !== "Unassigned"),
  );
};

export { hasIncompleteFundAssignments, getUnassignedFund };
