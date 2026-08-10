import { type FundGoalProgress, FundedBalanceStatus } from "@/fund-goals/types";
import { compareCurrencyAmounts } from "@/framework/currencyHelpers";

/**
 * Returns whether a configured maximum funded amount has meaningful progress.
 * A zero funded balance is still below the goal, even though it is within the
 * configured upper bound.
 */
const isMaximumFundedBalanceSatisfied = function (
  fundedBalance: FundGoalProgress["fundedBalance"],
): boolean {
  return (
    fundedBalance !== null &&
    fundedBalance !== undefined &&
    compareCurrencyAmounts(fundedBalance.balance, 0) > 0 &&
    fundedBalance.status !== FundedBalanceStatus.AboveMaximum
  );
};

export { isMaximumFundedBalanceSatisfied };
