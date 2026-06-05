import type { Route } from "next";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import { objectToSearchParams } from "@/framework/routes";

const pathWithSearchParams = function (
  pathname: string,
  searchParams: URLSearchParams,
): Route {
  const query = searchParams.toString();
  return query === "" ? pathname : `${pathname}?${query}`;
};

/**
 * App routes related to accounting periods.
 */
const routes = {
  workspace: (searchParams: TransactionWorkspaceSearchParams): Route =>
    pathWithSearchParams(
      "/transactions/workspace",
      objectToSearchParams(searchParams),
    ),
};

export default routes;
