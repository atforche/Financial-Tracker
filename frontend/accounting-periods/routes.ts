import {
  appendRepeatedSearchParam,
  buildUrl,
  objectToSearchParams,
} from "@/framework/routes/helpers";
import type { AccountingPeriodTrendsSearchParams } from "@/accounting-periods/trends/AccountingPeriodTrends";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import type { Route } from "next";

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
  trends: (searchParams: AccountingPeriodTrendsSearchParams): Route =>
    buildUrl("/accounting-periods/trends", objectToSearchParams(searchParams)),
  workspace: (searchParams: AccountingPeriodWorkspaceSearchParams): Route =>
    buildUrl(
      "/accounting-periods/workspace",
      accountingPeriodWorkspaceSearchParamsToSearchParams(searchParams),
    ),
};

export default routes;
