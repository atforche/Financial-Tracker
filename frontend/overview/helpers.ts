import type { TrendRangeMode } from "@/framework/routes/trendRange";

interface OverviewSearchParams {
  mode?: TrendRangeMode;
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
  startDate?: string;
  endDate?: string;
}

const overviewParamNames = {
  mode: "mode",
  startAccountingPeriodId: "startAccountingPeriodId",
  endAccountingPeriodId: "endAccountingPeriodId",
  startDate: "startDate",
  endDate: "endDate",
} as const satisfies Record<keyof OverviewSearchParams, string>;

export { overviewParamNames };
export type { OverviewSearchParams };
