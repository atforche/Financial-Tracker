import {
  EndingBalanceStatusModel,
  FundPlanBalanceEventSortModel,
  FundPlanSortModel,
  FundedBalanceStatusModel,
  type components,
} from "@/framework/data/api";

/**
 * Type representing a Fund Plan.
 */
type FundPlan = components["schemas"]["FundPlanModel"];

/**
 * Type representing Fund Plan progress.
 */
type FundPlanProgress = components["schemas"]["FundPlanProgressModel"];

/**
 * A Fund Plan paired with its calculated progress for the selected period.
 */
interface FundPlanWithProgress extends FundPlan {
  readonly progress: FundPlanProgress;
}

/**
 * Type representing current Fund availability.
 */
type FundAvailability = components["schemas"]["FundAvailabilityModel"];

/**
 * Type representing a request to update a Fund Plan.
 */
type UpdateFundPlanRequest = components["schemas"]["UpdateFundPlanModel"];

/**
 * Type representing a balance event for a Fund Plan.
 */
type FundPlanBalanceEvent = components["schemas"]["FundPlanBalanceEventModel"];

/**
 * Type representing a potentially unfinished Fund Plan balance event.
 */
type FundPlanBalanceEventDraft = Partial<FundPlanBalanceEvent>;

export {
  EndingBalanceStatusModel as EndingBalanceStatus,
  type FundAvailability,
  type FundPlan,
  type FundPlanBalanceEvent,
  type FundPlanBalanceEventDraft,
  FundPlanBalanceEventSortModel as FundPlanBalanceEventSort,
  type FundPlanProgress,
  FundedBalanceStatusModel as FundedBalanceStatus,
  type FundPlanWithProgress,
  FundPlanSortModel as FundPlanSort,
  type UpdateFundPlanRequest,
};
