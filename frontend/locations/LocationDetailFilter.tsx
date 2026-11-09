"use client";

import type { AccountingPeriod } from "@/accounting-periods/types";
import type { JSX } from "react";
import type { Location } from "@/locations/types";
import LocationDetailActions from "@/locations/LocationDetailActions";
import TrendFilterHeader from "@/framework/view/TrendFilterHeader";

/**
 * Shows a location's name with its actions and the range controls for its page.
 */
const LocationDetailFilter = function ({
  location,
  accountingPeriods,
  returnUrl,
  workspaceUrl,
}: {
  readonly location: Location;
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly returnUrl: string | undefined;
  readonly workspaceUrl: string;
}): JSX.Element {
  return (
    <TrendFilterHeader
      title={location.name}
      returnUrl={returnUrl}
      accountingPeriods={accountingPeriods}
      hasActiveFilters={false}
      clearFilters={() => undefined}
      action={
        <LocationDetailActions
          location={location}
          workspaceUrl={workspaceUrl}
        />
      }
    >
      {null}
    </TrendFilterHeader>
  );
};

export default LocationDetailFilter;
