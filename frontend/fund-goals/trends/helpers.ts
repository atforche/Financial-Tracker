import type {
  FundGoalBalanceEventSort,
  FundGoalSort,
} from "@/fund-goals/types";

/**
 * Search parameters supported by the fund goal trends page.
 */
interface FundGoalTrendsSearchParams {
  sort?: FundGoalSort;
  page?: number | string | null;
  balanceEventSort?: FundGoalBalanceEventSort;
  balanceEventPage?: number | string | null;
  fundName?: string | readonly string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
}

/**
 * Parameter names used by the fund goal trends page.
 */
const fundGoalTrendsParamNames = {
  sort: "sort",
  page: "page",
  balanceEventSort: "balanceEventSort",
  balanceEventPage: "balanceEventPage",
  fundName: "fundName",
  startAccountingPeriodId: "startAccountingPeriodId",
  endAccountingPeriodId: "endAccountingPeriodId",
} as const satisfies Record<keyof FundGoalTrendsSearchParams, string>;

export { type FundGoalTrendsSearchParams, fundGoalTrendsParamNames };
