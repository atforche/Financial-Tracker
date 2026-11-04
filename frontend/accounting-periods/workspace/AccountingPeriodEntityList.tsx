"use client";

import { Box, MenuItem, Stack, TextField } from "@mui/material";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import { getPaginationIndex, getRowsPerPage } from "@/framework/listframe/page";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import Link from "next/link";
import ListFrame from "@/framework/listframe/ListFrame";
import PeriodEntityStatus from "@/accounting-periods/workspace/periodEntityStatus";
import type { Route } from "next";
import SearchBar from "@/framework/listframe/SearchBar";
import SignedChange from "@/framework/view/SignedChange";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import parseEnumValue from "@/framework/data/parseEnumValue";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * An account or fund's balances and goal status in an accounting period.
 */
interface PeriodEntity {
  readonly id: string;
  readonly name: string;
  readonly href: Route;
  readonly openingBalance: number;
  readonly closingBalance: number;
  readonly minimum: number | null;
  readonly maximum: number | null;
  readonly status: PeriodEntityStatus;
}

const statusDetails: Record<
  PeriodEntityStatus,
  { readonly label: string; readonly color: string; readonly order: number }
> = {
  [PeriodEntityStatus.BelowMinimum]: {
    label: "Below minimum",
    color: "error.main",
    order: 0,
  },
  [PeriodEntityStatus.AboveMaximum]: {
    label: "Above maximum",
    color: "warning.main",
    order: 1,
  },
  [PeriodEntityStatus.WithinRange]: {
    label: "In range",
    color: "success.main",
    order: 2,
  },
  [PeriodEntityStatus.NoGoal]: {
    label: "No goal",
    color: "text.secondary",
    order: 3,
  },
};

/**
 * Supported sort orders.
 */
enum EntitySort {
  Name = "Name",
  NameDescending = "NameDescending",
  Closing = "Closing",
  ClosingDescending = "ClosingDescending",
  Change = "Change",
  ChangeDescending = "ChangeDescending",
  Status = "Status",
  StatusDescending = "StatusDescending",
}

/**
 * Gets how much an entity's balance changed over the period.
 */
const getChange = function (entity: PeriodEntity): number {
  return getCurrencyDifference(entity.closingBalance, entity.openingBalance);
};

/**
 * Shows a closing balance against its goal range on one compact scale.
 */
const GoalRangeBar = function ({
  entity,
}: {
  readonly entity: PeriodEntity;
}): JSX.Element | null {
  const { minimum, maximum, closingBalance, status } = entity;
  if (minimum === null) {
    return null;
  }
  const upperBound = maximum ?? minimum;
  const lower = Math.min(closingBalance, minimum);
  const upper = Math.max(closingBalance, upperBound);
  const padding = Math.max(upper - lower, Math.abs(minimum), 1) * 0.15;
  const position = (amount: number): number =>
    ((amount - lower + padding) / (upper - lower + padding * 2)) * 100;
  return (
    <Box sx={{ position: "relative", height: 14, minWidth: 120 }}>
      <Box
        sx={{
          position: "absolute",
          inset: "3px 0",
          borderRadius: 999,
          bgcolor: "action.hover",
        }}
      />
      <Box
        sx={{
          position: "absolute",
          top: 3,
          bottom: 3,
          left: `${position(minimum)}%`,
          width: `${(maximum === null ? 100 : position(maximum)) - position(minimum)}%`,
          borderRadius: 999,
          bgcolor: "success.main",
          opacity: 0.35,
        }}
      />
      <Box
        sx={{
          position: "absolute",
          left: `${position(closingBalance)}%`,
          width: 14,
          height: 14,
          borderRadius: "50%",
          border: "2px solid",
          borderColor: "background.paper",
          bgcolor: statusDetails[status].color,
          transform: "translateX(-50%)",
        }}
      />
    </Box>
  );
};

/**
 * Compares entities for the selected sort, with worse goal statuses first by default.
 */
const compareEntities = function (
  first: PeriodEntity,
  second: PeriodEntity,
  sort: EntitySort,
): number {
  const direction = sort.endsWith("Descending") ? -1 : 1;
  switch (sort) {
    case EntitySort.Name:
    case EntitySort.NameDescending:
      return first.name.localeCompare(second.name) * direction;
    case EntitySort.Closing:
    case EntitySort.ClosingDescending:
      return (first.closingBalance - second.closingBalance) * direction;
    case EntitySort.Change:
    case EntitySort.ChangeDescending:
      return (getChange(first) - getChange(second)) * direction;
    case EntitySort.Status:
    case EntitySort.StatusDescending:
    default:
      return (
        (statusDetails[first.status].order -
          statusDetails[second.status].order) *
        direction
      );
  }
};

/**
 * Compares accounts or funds within one accounting period, linking each to its goal for that period.
 */
const AccountingPeriodEntityList = function ({
  title,
  entities,
}: {
  readonly title: string;
  readonly entities: readonly PeriodEntity[];
}): JSX.Element {
  const searchParams = useSearchParams();
  const updateParams = useSearchParamUpdater(["entityPage"]);
  const currentSort = parseEnumValue(
    EntitySort,
    searchParams.get("entitySort") ?? "",
  );
  const status = parseEnumValue(
    PeriodEntityStatus,
    searchParams.get("status") ?? "",
  );
  const search = (searchParams.get("search") ?? "").trim().toLowerCase();
  const filtered = entities
    .filter(
      (entity) =>
        (status === null || entity.status === status) &&
        entity.name.toLowerCase().includes(search),
    )
    .sort(
      (first, second) =>
        compareEntities(first, second, currentSort ?? EntitySort.Status) ||
        first.name.localeCompare(second.name),
    );
  const pageSize = getRowsPerPage(searchParams.get("pageSize"));
  const pageIndex = getPaginationIndex(
    searchParams.get("entityPage"),
    filtered.length,
    pageSize,
  );
  const getSortProps = createColumnSortProps(currentSort, (sort) => {
    updateParams((params) => {
      if (sort === null) {
        params.delete("entitySort");
      } else {
        params.set("entitySort", sort);
      }
    });
  });
  const columns: ColumnDefinition<PeriodEntity>[] = [
    {
      name: "name",
      headerContent: "Name",
      mobilePrimary: true,
      getBodyContent: (entity) => <Link href={entity.href}>{entity.name}</Link>,
      ...getSortProps(EntitySort.Name, EntitySort.NameDescending),
    },
    {
      name: "opening",
      headerContent: "Opening",
      alignment: "right",
      getBodyContent: (entity) => formatCurrency(entity.openingBalance),
    },
    {
      name: "closing",
      headerContent: "Closing",
      alignment: "right",
      getBodyContent: (entity) => formatCurrency(entity.closingBalance),
      ...getSortProps(EntitySort.Closing, EntitySort.ClosingDescending),
    },
    {
      name: "change",
      headerContent: "Change",
      alignment: "right",
      getBodyContent: (entity) => <SignedChange amount={getChange(entity)} />,
      ...getSortProps(EntitySort.Change, EntitySort.ChangeDescending),
    },
    {
      name: "goal",
      headerContent: "Ending balance vs. goal",
      getBodyContent: (entity) => <GoalRangeBar entity={entity} />,
    },
    {
      name: "status",
      headerContent: "Status",
      getBodyContent: (entity) => (
        <Box
          component="span"
          sx={{ color: statusDetails[entity.status].color }}
        >
          {statusDetails[entity.status].label}
        </Box>
      ),
      ...getSortProps(EntitySort.Status, EntitySort.StatusDescending),
    },
  ];
  return (
    <Stack spacing={2}>
      <Stack direction="row" useFlexGap spacing={2} sx={{ flexWrap: "wrap" }}>
        <SearchBar searchParamName="search" pageParamName="entityPage" />
        <TextField
          select
          size="small"
          label="Status"
          value={status ?? "All"}
          onChange={(event) => {
            updateParams((params) => {
              if (event.target.value === "All") {
                params.delete("status");
              } else {
                params.set("status", event.target.value);
              }
            });
          }}
          sx={{ minWidth: 180 }}
        >
          <MenuItem value="All">All statuses</MenuItem>
          {Object.values(PeriodEntityStatus).map((value) => (
            <MenuItem key={value} value={value}>
              {statusDetails[value].label}
            </MenuItem>
          ))}
        </TextField>
      </Stack>
      <ListFrame<PeriodEntity>
        title={title}
        columns={columns}
        getId={(entity) => entity.id}
        data={filtered.slice(pageIndex * pageSize, (pageIndex + 1) * pageSize)}
        totalCount={filtered.length}
        pageParamName="entityPage"
        hasActiveFilters={search !== "" || status !== null}
        filteredEmptyState={{
          title: `No Matching ${title}`,
          description: "Change the search or status filter.",
        }}
      />
    </Stack>
  );
};

export default AccountingPeriodEntityList;
