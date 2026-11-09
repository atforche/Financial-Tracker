import { buildUrl, objectToSearchParams } from "@/framework/routes/helpers";
import type { Route } from "next";
import type { TrendRangeMode } from "@/framework/routes/trendRange";

/**
 * The range a location page covers, shared by the workspace and each location.
 */
interface LocationRangeSearchParams {
  mode?: TrendRangeMode;
  startDate?: string;
  endDate?: string;
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
}

/**
 * Search parameters for the location workspace.
 */
interface LocationWorkspaceSearchParams extends LocationRangeSearchParams {
  search?: string;
  sort?: string;
  page?: string;
  pageSize?: string;
}

/**
 * Search parameters for one location's page.
 */
interface LocationDetailSearchParams extends LocationRangeSearchParams {
  returnUrl?: string;
}

/**
 * App routes related to locations.
 */
const routes = {
  workspace: (searchParams: LocationWorkspaceSearchParams): Route =>
    buildUrl("/locations", objectToSearchParams(searchParams)),
  workspaceDetail: (
    locationId: string,
    searchParams: LocationDetailSearchParams,
  ): Route =>
    buildUrl(`/locations/${locationId}`, objectToSearchParams(searchParams)),
};

export type {
  LocationDetailSearchParams,
  LocationRangeSearchParams,
  LocationWorkspaceSearchParams,
};
export default routes;
