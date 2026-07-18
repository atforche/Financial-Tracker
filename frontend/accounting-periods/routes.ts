import {
  appendRepeatedSearchParam,
  buildUrl,
} from "@/framework/routes/helpers";
import type { AccountingPeriodTrendsSearchParams } from "@/accounting-periods/trends/AccountingPeriodTrends";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import type { CurrentAccountingPeriodSearchParams } from "@/accounting-periods/current/CurrentAccountingPeriod";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

/**
 * Converts an AccountingPeriodWorkspaceSearchParams object to generic URL search params.
 */
const accountingPeriodWorkspaceSearchParamsToSearchParams = function (
  searchParams: AccountingPeriodWorkspaceSearchParams,
): URLSearchParams {
  const { years, months, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "years", years);
  appendRepeatedSearchParam(params, "months", months);

  return params;
};

/**
 * App routes related to accounting periods.
 */
const routes = {
  current: (searchParams: CurrentAccountingPeriodSearchParams): Route =>
    buildUrl("/accounting-periods/current", objectToSearchParams(searchParams)),
  trends: (searchParams: AccountingPeriodTrendsSearchParams): Route =>
    buildUrl("/accounting-periods/trends", objectToSearchParams(searchParams)),
  workspace: (searchParams: AccountingPeriodWorkspaceSearchParams): Route =>
    buildUrl(
      "/accounting-periods/workspace",
      accountingPeriodWorkspaceSearchParamsToSearchParams(searchParams),
    ),
};

export default routes;
