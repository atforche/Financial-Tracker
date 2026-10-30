import { buildUrl, objectToSearchParams } from "@/framework/routes/helpers";
import type { AccountingPeriodSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspaceDetailPage";
import type { AccountingPeriodTrendsSearchParams } from "@/accounting-periods/trends/AccountingPeriodTrends";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import type { Route } from "next";

/**
 * App routes related to accounting periods.
 */
const routes = {
  trends: (searchParams: AccountingPeriodTrendsSearchParams): Route =>
    buildUrl("/accounting-periods/trends", objectToSearchParams(searchParams)),
  workspace: (searchParams: AccountingPeriodWorkspaceSearchParams): Route =>
    buildUrl(
      "/accounting-periods/workspace",
      objectToSearchParams(searchParams),
    ),
  workspaceDetail: (
    accountingPeriodId: string,
    searchParams: AccountingPeriodSearchParams,
  ): Route =>
    buildUrl(
      `/accounting-periods/workspace/${accountingPeriodId}`,
      objectToSearchParams(searchParams),
    ),
  expectedIncomeSourceCreate: (
    accountingPeriodId: string,
    returnUrl?: string,
  ): Route =>
    buildUrl(
      `/accounting-periods/workspace/${accountingPeriodId}/expected-income-sources/create`,
      objectToSearchParams(
        typeof returnUrl === "undefined" ? {} : { returnUrl },
      ),
    ),
  expectedIncomeSource: (
    accountingPeriodId: string,
    sourceId: string,
    returnUrl?: string,
  ): Route =>
    buildUrl(
      `/accounting-periods/workspace/${accountingPeriodId}/expected-income-sources/${sourceId}`,
      objectToSearchParams(
        typeof returnUrl === "undefined" ? {} : { returnUrl },
      ),
    ),
  expectedIncomeSourceEdit: (
    accountingPeriodId: string,
    sourceId: string,
    returnUrl?: string,
  ): Route =>
    buildUrl(
      `/accounting-periods/workspace/${accountingPeriodId}/expected-income-sources/${sourceId}/edit`,
      objectToSearchParams(
        typeof returnUrl === "undefined" ? {} : { returnUrl },
      ),
    ),
};

export default routes;
