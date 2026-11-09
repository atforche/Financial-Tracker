"use client";

import PagedTable, { type PagedTableColumn } from "@/framework/view/PagedTable";
import routes, { type LocationDetailSearchParams } from "@/locations/routes";
import type { JSX } from "react";
import { LocationRangeSortModel } from "@/framework/data/api";
import type { LocationRangeSummary } from "@/locations/types";
import { formatCurrency } from "@/framework/currencyHelpers";
import { formatShortDate } from "@/framework/dateHelpers";

const columns: readonly PagedTableColumn<LocationRangeSummary>[] = [
  {
    name: "name",
    header: "Name",
    getValue: (location) => location.name,
    width: "minmax(0, 2fr)",
    sort: [LocationRangeSortModel.Name, LocationRangeSortModel.NameDescending],
  },
  {
    name: "spent",
    header: "Spent",
    getValue: (location) => formatCurrency(location.spent),
    width: "minmax(0, 1fr)",
    alignment: "right",
    sort: [
      LocationRangeSortModel.Spent,
      LocationRangeSortModel.SpentDescending,
    ],
    descendingFirst: true,
  },
  {
    name: "received",
    header: "Received",
    getValue: (location) => formatCurrency(location.received),
    width: "minmax(0, 1fr)",
    alignment: "right",
    hideOnMobile: true,
    sort: [
      LocationRangeSortModel.Received,
      LocationRangeSortModel.ReceivedDescending,
    ],
    descendingFirst: true,
  },
  {
    name: "transactions",
    header: "Transactions",
    getValue: (location) => location.transactionCount.toString(),
    width: "minmax(0, 0.8fr)",
    alignment: "right",
    hideOnMobile: true,
    sort: [
      LocationRangeSortModel.TransactionCount,
      LocationRangeSortModel.TransactionCountDescending,
    ],
    descendingFirst: true,
  },
  {
    name: "lastActivity",
    header: "Last Activity",
    getValue: (location) =>
      location.lastActivity === null || location.lastActivity === undefined
        ? "—"
        : formatShortDate(new Date(`${location.lastActivity}T00:00:00`)),
    width: "minmax(0, 1fr)",
    alignment: "right",
    hideOnMobile: true,
    sort: [
      LocationRangeSortModel.LastActivity,
      LocationRangeSortModel.LastActivityDescending,
    ],
    descendingFirst: true,
  },
];

/**
 * Ranks the locations by their activity in the range, each linking to the location over the same range.
 */
const LocationWorkspaceTable = function ({
  locations,
  totalCount,
  detailParams,
}: {
  readonly locations: readonly LocationRangeSummary[];
  readonly totalCount: number;

  /**
   * The range and return link, so each location opens over the same range.
   */
  readonly detailParams: LocationDetailSearchParams;
}): JSX.Element {
  return (
    <PagedTable
      title="Where Your Money Went"
      columns={columns}
      data={locations}
      getKey={(location) => location.id}
      getHref={(location) => routes.workspaceDetail(location.id, detailParams)}
      totalCount={totalCount}
      sortParamName="sort"
      defaultSort={LocationRangeSortModel.SpentDescending}
      pageParamName="page"
      emptyState={{
        title: "No Locations Found",
        description:
          "Locations are added when transactions name who was paid or who paid you. Try a different search.",
      }}
    />
  );
};

export default LocationWorkspaceTable;
