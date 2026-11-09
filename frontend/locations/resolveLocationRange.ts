import {
  type AccountingPeriod,
  AccountingPeriodSort,
} from "@/accounting-periods/types";
import {
  matchesTrendRange,
  resolveTrendRange,
} from "@/framework/routes/trendRange";
import type { LocationRangeSearchParams } from "@/locations/routes";
import type { Route } from "next";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * A date or period range in the shape of its search parameters.
 */
type ResolvedLocationRange = ReturnType<typeof resolveTrendRange>;

/**
 * Resolves the date or period range a location page covers, redirecting to the canonical URL for it when the
 * request leaves it out or asks for one that does not exist.
 */
const resolveLocationRange = async function (
  searchParams: LocationRangeSearchParams,
  getCanonicalUrl: (range: ResolvedLocationRange) => Route,
): Promise<{
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly range: ResolvedLocationRange;
}> {
  const apiClient = await createApiClient();
  const accountingPeriods = unwrapApiResponse(
    await apiClient.GET("/accounting-periods", {
      params: {
        query: { Sort: AccountingPeriodSort.DateDescending, Limit: 500 },
      },
    }),
    "Failed to fetch accounting periods",
  ).items;
  const range = resolveTrendRange(searchParams, accountingPeriods);
  if (!matchesTrendRange(searchParams, range)) {
    redirect(getCanonicalUrl(range));
  }
  return { accountingPeriods, range };
};

export default resolveLocationRange;
