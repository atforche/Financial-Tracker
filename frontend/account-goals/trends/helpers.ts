/**
 * Search parameters supported by the Account Goal trends page.
 */
interface AccountGoalTrendsSearchParams {
  goalHistoryPage?: string;
  goalHistorySort?: string;
  accountId?: string;
  accountName?: string | readonly string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  returnUrl?: string;
}

/**
 * Parameter names used by the Account Goal trends page.
 */
const accountGoalTrendsParamNames = {
  goalHistoryPage: "goalHistoryPage",
  goalHistorySort: "goalHistorySort",
  accountId: "accountId",
  accountName: "accountName",
  startAccountingPeriodId: "startAccountingPeriodId",
  endAccountingPeriodId: "endAccountingPeriodId",
  returnUrl: "returnUrl",
} as const satisfies Record<keyof AccountGoalTrendsSearchParams, string>;

export { type AccountGoalTrendsSearchParams, accountGoalTrendsParamNames };
