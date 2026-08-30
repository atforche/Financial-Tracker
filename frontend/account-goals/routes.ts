import {
  appendRepeatedSearchParam,
  buildUrl,
  objectToSearchParams,
} from "@/framework/routes/helpers";
import type { AccountGoalTrendsSearchParams } from "@/account-goals/trends/helpers";
import type { AccountGoalWorkspaceSearchParams } from "@/account-goals/workspace/types";
import type { Route } from "next";

const accountGoalWorkspaceSearchParamsToSearchParams = function (
  searchParams: AccountGoalWorkspaceSearchParams,
): URLSearchParams {
  const { accountIds, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);
  appendRepeatedSearchParam(params, "accountIds", accountIds);
  return params;
};

const accountGoalTrendsSearchParamsToSearchParams = function (
  searchParams: AccountGoalTrendsSearchParams,
): URLSearchParams {
  const { accountName, ...remainingSearchParams } = searchParams;
  const params = objectToSearchParams(remainingSearchParams);
  appendRepeatedSearchParam(params, "accountName", accountName);
  return params;
};

/**
 * Defines the routes for the Account Goals section of the application.
 */
const routes = {
  trends: (searchParams: AccountGoalTrendsSearchParams): Route =>
    buildUrl(
      "/account-goals/trends",
      accountGoalTrendsSearchParamsToSearchParams(searchParams),
    ),
  workspace: (searchParams: AccountGoalWorkspaceSearchParams): Route =>
    buildUrl(
      "/account-goals/workspace",
      accountGoalWorkspaceSearchParamsToSearchParams(searchParams),
    ),
  workspaceDetail: (
    accountId: string,
    searchParams: AccountGoalWorkspaceSearchParams,
  ): Route =>
    buildUrl(
      `/account-goals/workspace/${accountId}`,
      accountGoalWorkspaceSearchParamsToSearchParams(searchParams),
    ),
};

export default routes;
