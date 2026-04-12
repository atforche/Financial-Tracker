import type { Route } from "next";

type QueryParams = Record<string, string | number | boolean | null | undefined>;

/**
 * App routes backed by the Next.js app router tree.
 */
const routes = {
  home: "/" as Route,
  accountingPeriods: {
    index: "/accounting-periods" as Route,
    create: "/accounting-periods/create" as Route,
    detail: (accountingPeriodId: string): Route =>
      `/accounting-periods/${accountingPeriodId}`,
    close: (accountingPeriodId: string): Route =>
      `/accounting-periods/${accountingPeriodId}/close`,
    reopen: (accountingPeriodId: string): Route =>
      `/accounting-periods/${accountingPeriodId}/reopen`,
    delete: (accountingPeriodId: string): Route =>
      `/accounting-periods/${accountingPeriodId}/delete`,
    fundDetail: (accountingPeriodId: string, fundId: string): Route =>
      `/accounting-periods/${accountingPeriodId}/funds/${fundId}`,
    fundGoalCreate: (accountingPeriodId: string, fundId: string): Route =>
      `/accounting-periods/${accountingPeriodId}/funds/${fundId}/goal/create`,
    fundGoalUpdate: (accountingPeriodId: string, fundId: string): Route =>
      `/accounting-periods/${accountingPeriodId}/funds/${fundId}/goal/update`,
    fundGoalDelete: (accountingPeriodId: string, fundId: string): Route =>
      `/accounting-periods/${accountingPeriodId}/funds/${fundId}/goal/delete`,
  },
  accounts: {
    index: "/accounts" as Route,
    update: (accountId: string): Route => `/accounts/${accountId}/update`,
    delete: (accountId: string): Route => `/accounts/${accountId}/delete`,
  },
  funds: {
    index: "/funds" as Route,
    create: "/funds/create" as Route,
    detail: (fundId: string): Route => `/funds/${fundId}`,
    update: (fundId: string): Route => `/funds/${fundId}/update`,
    delete: (fundId: string): Route => `/funds/${fundId}/delete`,
  },
} as const;

/**
 * Appends a query string to a valid internal route when needed.
 */
const withQuery = function <Path extends string>(
  pathname: Path,
  query: QueryParams,
): Path | `${Path}?${string}` {
  const params = new URLSearchParams();
  for (const [key, value] of Object.entries(query)) {
    if (typeof value !== "undefined" && value !== null) {
      params.set(key, String(value));
    }
  }

  const queryString = params.toString();
  if (queryString === "") {
    return pathname;
  }

  const href: `${Path}?${string}` = `${pathname}?${queryString}`;
  return href;
};

export default routes;
export { withQuery };
