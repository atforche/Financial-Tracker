import {
  getPageOffset,
  normalizePageValue,
  rowsPerPage,
} from "@/framework/view/pagination";
import routes, { type LocationWorkspaceSearchParams } from "@/locations/routes";
import type { JSX } from "react";
import { LocationRangeSortModel } from "@/framework/data/api";
import LocationWorkspaceFilter from "@/locations/LocationWorkspaceFilter";
import LocationWorkspaceTable from "@/locations/LocationWorkspaceTable";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import StatCard from "@/framework/view/StatCard";
import createApiClient from "@/framework/data/createApiClient";
import { formatCurrency } from "@/framework/currencyHelpers";
import parseEnumValue from "@/framework/data/parseEnumValue";
import resolveLocationRange from "@/locations/resolveLocationRange";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Shows where money went over a date or period range: totals across every location, and the locations ranked by
 * their activity.
 */
const LocationWorkspace = async function ({
  searchParams,
}: {
  readonly searchParams: Promise<LocationWorkspaceSearchParams>;
}): Promise<JSX.Element> {
  const resolvedSearchParams = await searchParams;
  const { search, sort, page } = resolvedSearchParams;
  const listParams = {
    ...(search === undefined ? {} : { search }),
    ...(sort === undefined ? {} : { sort }),
  };
  const { accountingPeriods, range } = await resolveLocationRange(
    resolvedSearchParams,
    (canonicalRange) => routes.workspace({ ...listParams, ...canonicalRange }),
  );
  const parsedSort = parseEnumValue(LocationRangeSortModel, sort ?? "");
  const query = {
    ...(search === undefined ? {} : { NameSearch: search }),
    ...(parsedSort === null ? {} : { Sort: parsedSort }),
    Limit: rowsPerPage,
    Offset: getPageOffset(normalizePageValue(page)),
  };
  const apiClient = await createApiClient();
  const locations = unwrapApiResponse(
    range.mode === "date"
      ? await apiClient.GET("/locations/date-range", {
          params: {
            query: {
              ...query,
              "Range.Start": range.startDate,
              "Range.End": range.endDate,
            },
          },
        })
      : await apiClient.GET("/locations/accounting-period-range", {
          params: {
            query: {
              ...query,
              "Range.Start": range.startAccountingPeriodId,
              "Range.End": range.endAccountingPeriodId,
            },
          },
        }),
    "Failed to fetch location activity",
  );

  return (
    <PageLayout>
      <LocationWorkspaceFilter accountingPeriods={accountingPeriods} />
      <ResponsiveGrid columns={{ xs: 2, lg: 4 }} uniformPhoneRows>
        <StatCard
          label="Spent"
          value={formatCurrency(locations.totalSpent)}
          caption="Paid to locations in this range"
        />
        <StatCard
          label="Received"
          value={formatCurrency(locations.totalReceived)}
          caption="Income and refunds from locations"
        />
        <StatCard
          label="Places with activity"
          value={locations.activeLocationCount.toString()}
          caption={`of ${locations.locations.totalCount.toString()} locations`}
        />
        <StatCard
          label="Top 5 share"
          value={
            locations.totalSpent === 0
              ? "—"
              : `${Math.round((locations.topFiveSpent / locations.totalSpent) * 100).toString()}%`
          }
          caption="of spending went to the top 5 locations"
        />
      </ResponsiveGrid>
      <LocationWorkspaceTable
        locations={locations.locations.items}
        totalCount={locations.locations.totalCount}
        detailParams={{
          ...range,
          returnUrl: routes.workspace({ ...listParams, ...range }),
        }}
      />
    </PageLayout>
  );
};

export default LocationWorkspace;
