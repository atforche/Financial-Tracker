import { buildUrl, objectToSearchParams } from "@/framework/routes/helpers";
import type { LocationWorkspaceSearchParams } from "@/locations/LocationWorkspace";
import type { Route } from "next";

/** Routes related to the Location workspace. */
const routes = {
  workspace: (searchParams: LocationWorkspaceSearchParams): Route =>
    buildUrl("/locations", objectToSearchParams(searchParams)),
  workspaceDetail: (
    locationId: string,
    searchParams: LocationWorkspaceSearchParams,
  ): Route =>
    buildUrl(`/locations/${locationId}`, objectToSearchParams(searchParams)),
};

export default routes;
