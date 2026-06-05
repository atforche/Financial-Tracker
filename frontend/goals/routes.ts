import type { GoalWorkspaceSearchParams } from "@/goals/workspace/GoalWorkspace";
import type { Route } from "next";
import { objectToSearchParams } from "@/framework/routes";

const pathWithSearchParams = function (
  pathname: string,
  searchParams: URLSearchParams,
): Route {
  const query = searchParams.toString();
  return query === "" ? pathname : `${pathname}?${query}`;
};

const isRepeatedSearchParamArray = function (
  value: string | readonly string[] | undefined,
): value is readonly string[] {
  return Array.isArray(value);
};

const appendRepeatedSearchParam = function (
  params: URLSearchParams,
  key: string,
  value: string | readonly string[] | undefined,
): void {
  if (isRepeatedSearchParamArray(value)) {
    value.forEach((item) => {
      params.append(key, item);
    });
    return;
  }

  if (typeof value === "string") {
    params.append(key, value);
  }
};

const goalWorkspaceSearchParamsToSearchParams = function (
  searchParams: GoalWorkspaceSearchParams,
): URLSearchParams {
  const { accountingPeriodIds, fundIds, ...remainingSearchParams } =
    searchParams;
  const params = objectToSearchParams(remainingSearchParams);

  appendRepeatedSearchParam(params, "accountingPeriodIds", accountingPeriodIds);
  appendRepeatedSearchParam(params, "fundIds", fundIds);

  return params;
};

/**
 * App routes related to goals.
 */
const routes = {
  index: (searchParams: GoalWorkspaceSearchParams): Route =>
    pathWithSearchParams(
      "/goals",
      goalWorkspaceSearchParamsToSearchParams(searchParams),
    ),
  workspace: (searchParams: GoalWorkspaceSearchParams): Route =>
    pathWithSearchParams(
      "/goals",
      goalWorkspaceSearchParamsToSearchParams(searchParams),
    ),
};

export default routes;
