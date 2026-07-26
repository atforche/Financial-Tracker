import {
  EndingBalanceStatusModel,
  FundGoalBalanceEventSortModel,
  FundGoalSortModel,
  FundedBalanceStatusModel,
  type components,
} from "@/framework/data/api";

/**
 * Type representing a Fund Goal.
 */
type FundGoal = components["schemas"]["FundGoalModel"];

/**
 * Type representing Fund Goal progress.
 */
type FundGoalProgress = components["schemas"]["FundGoalProgressModel"];

/**
 * A Fund Goal paired with its calculated progress for the selected period.
 */
interface FundGoalWithProgress extends FundGoal {
  readonly progress: FundGoalProgress;
}

/**
 * Type representing current Fund availability.
 */
type FundAvailability = components["schemas"]["FundAvailabilityModel"];

/**
 * Type representing a request to update a Fund Goal.
 */
type UpdateFundGoalRequest = components["schemas"]["UpdateFundGoalModel"];

/**
 * Type representing a balance event for a Fund Goal.
 */
type FundGoalBalanceEvent = components["schemas"]["FundGoalBalanceEventModel"];

/**
 * Type representing a potentially unfinished Fund Goal balance event.
 */
type FundGoalBalanceEventDraft = Partial<FundGoalBalanceEvent>;

export {
  EndingBalanceStatusModel as EndingBalanceStatus,
  type FundAvailability,
  type FundGoal,
  type FundGoalBalanceEvent,
  type FundGoalBalanceEventDraft,
  FundGoalBalanceEventSortModel as FundGoalBalanceEventSort,
  type FundGoalProgress,
  FundedBalanceStatusModel as FundedBalanceStatus,
  type FundGoalWithProgress,
  FundGoalSortModel as FundGoalSort,
  type UpdateFundGoalRequest,
};
