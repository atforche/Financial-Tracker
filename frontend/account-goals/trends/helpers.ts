/**
 * Search parameters supported by the Account Goal trends page.
 */
interface AccountGoalTrendsSearchParams {
  accountName?: string | readonly string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
}

/**
 * Parameter names used by the Account Goal trends page.
 */
const accountGoalTrendsParamNames = {
  accountName: "accountName",
  startAccountingPeriodId: "startAccountingPeriodId",
  endAccountingPeriodId: "endAccountingPeriodId",
} as const satisfies Record<keyof AccountGoalTrendsSearchParams, string>;

export { type AccountGoalTrendsSearchParams, accountGoalTrendsParamNames };
