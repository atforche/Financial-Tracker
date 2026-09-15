"use client";

import { getPaginationIndex, getRowsPerPage } from "@/framework/listframe/page";
import { Chip } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { formatCurrency } from "@/framework/currencyHelpers";
import parseEnumValue from "@/framework/data/parseEnumValue";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

interface HistoryPoint {
  readonly periodId: string;
  readonly period: string;
  readonly periodOrder: number;
  readonly isOpen: boolean;
  readonly minimum: number | null;
  readonly maximum: number | null;
  readonly span: number | null;
  readonly balance: number | null;
  readonly expected: number | null;
  readonly assigned: number | null;
  readonly satisfied: boolean | null;
}

enum HistorySort {
  Period = "Period",
  PeriodDescending = "PeriodDescending",
  Minimum = "Minimum",
  MinimumDescending = "MinimumDescending",
  Maximum = "Maximum",
  MaximumDescending = "MaximumDescending",
  Balance = "Balance",
  BalanceDescending = "BalanceDescending",
  Contribution = "Contribution",
  ContributionDescending = "ContributionDescending",
  Result = "Result",
  ResultDescending = "ResultDescending",
}

const sortParamName = "fundGoalHistorySort";
const pageParamName = "fundGoalHistoryPage";

const compareNullable = function (
  first: number | null,
  second: number | null,
  direction: number,
): number {
  if (first === null) {
    return second === null ? 0 : 1;
  }
  if (second === null) {
    return -1;
  }
  return (first - second) * direction;
};

/** Sortable period history in the application's standard list frame. */
const FundGoalHistoryListFrame = function ({
  points,
}: {
  readonly points: readonly HistoryPoint[];
}): JSX.Element {
  const searchParams = useSearchParams();
  const updateParams = useSearchParamUpdater([pageParamName]);
  const currentSort = parseEnumValue(
    HistorySort,
    searchParams.get(sortParamName) ?? "",
  );
  const effectiveSort = currentSort ?? HistorySort.PeriodDescending;
  const setSort = (sort: HistorySort | null): void => {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };
  const direction = effectiveSort.endsWith("Descending") ? -1 : 1;
  const sorted = [...points].sort((first, second) => {
    const comparison = ((): number => {
      switch (effectiveSort) {
        case HistorySort.Period:
        case HistorySort.PeriodDescending:
          return (first.periodOrder - second.periodOrder) * direction;
        case HistorySort.Minimum:
        case HistorySort.MinimumDescending:
          return compareNullable(first.minimum, second.minimum, direction);
        case HistorySort.Maximum:
        case HistorySort.MaximumDescending:
          return compareNullable(first.maximum, second.maximum, direction);
        case HistorySort.Balance:
        case HistorySort.BalanceDescending:
          return compareNullable(first.balance, second.balance, direction);
        case HistorySort.Contribution:
        case HistorySort.ContributionDescending:
          return compareNullable(first.assigned, second.assigned, direction);
        case HistorySort.Result:
        case HistorySort.ResultDescending:
          return compareNullable(
            first.satisfied === null ? null : Number(first.satisfied),
            second.satisfied === null ? null : Number(second.satisfied),
            direction,
          );
        default:
          return 0;
      }
    })();
    return comparison || second.periodOrder - first.periodOrder;
  });
  const pageSize = getRowsPerPage(searchParams.get("pageSize"));
  const pageIndex = getPaginationIndex(
    searchParams.get(pageParamName),
    sorted.length,
    pageSize,
  );
  const getSortProps = createColumnSortProps(currentSort, setSort);
  const columns: ColumnDefinition<HistoryPoint>[] = [
    {
      name: "period",
      headerContent: "Period",
      getBodyContent: (point) =>
        `${point.period}${point.isOpen ? " (open)" : ""}`,
      mobilePrimary: true,
      ...getSortProps(HistorySort.Period, HistorySort.PeriodDescending),
    },
    {
      name: "minimum",
      headerContent: "Minimum",
      getBodyContent: (point) =>
        point.minimum === null ? "—" : formatCurrency(point.minimum),
      alignment: "right",
      ...getSortProps(HistorySort.Minimum, HistorySort.MinimumDescending),
    },
    {
      name: "maximum",
      headerContent: "Maximum",
      getBodyContent: (point) =>
        point.minimum === null
          ? "—"
          : point.maximum === null
            ? "No maximum"
            : formatCurrency(point.maximum),
      alignment: "right",
      ...getSortProps(HistorySort.Maximum, HistorySort.MaximumDescending),
    },
    {
      name: "balance",
      headerContent: "Balance",
      getBodyContent: (point) =>
        point.balance === null ? "—" : formatCurrency(point.balance),
      alignment: "right",
      ...getSortProps(HistorySort.Balance, HistorySort.BalanceDescending),
    },
    {
      name: "contribution",
      headerContent: "Contribution",
      getBodyContent: (point) =>
        point.expected === null
          ? "—"
          : `${formatCurrency(point.assigned ?? 0)} / ${formatCurrency(point.expected)}`,
      alignment: "right",
      ...getSortProps(
        HistorySort.Contribution,
        HistorySort.ContributionDescending,
      ),
    },
    {
      name: "result",
      headerContent: "Result",
      getBodyContent: (point) => (
        <Chip
          size="small"
          label={
            point.satisfied === null
              ? "No goal"
              : point.isOpen
                ? point.satisfied
                  ? "On track"
                  : "Outside range"
                : point.satisfied
                  ? "Achieved"
                  : "Missed"
          }
          color={
            point.satisfied === null
              ? "default"
              : point.satisfied
                ? "success"
                : "error"
          }
        />
      ),
      ...getSortProps(HistorySort.Result, HistorySort.ResultDescending),
    },
  ];

  return (
    <ListFrame<HistoryPoint>
      title="Goal History"
      columns={columns}
      getId={(point) => point.periodId}
      data={sorted.slice(pageIndex * pageSize, (pageIndex + 1) * pageSize)}
      totalCount={sorted.length}
      pageParamName={pageParamName}
      initialEmptyState={{
        title: "No Goal History",
        description:
          "No Accounting Periods are available in the selected range.",
      }}
    />
  );
};

export { type HistoryPoint };
export default FundGoalHistoryListFrame;
