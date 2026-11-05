import {
  FundGoalEndingBalanceStatus,
  type FundGoalProgress,
} from "@/fund-goals/types";

/**
 * Determines whether a fund's ending balance and contribution both met its goal.
 */
const isFundGoalMet = function (progress: FundGoalProgress): boolean {
  return (
    progress.endingBalance.status === FundGoalEndingBalanceStatus.WithinRange &&
    (progress.contribution?.isSatisfied ?? true)
  );
};

export default isFundGoalMet;
