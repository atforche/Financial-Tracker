"use client";

import {
  type GoalBalanceEvent,
  type GoalTrendsView,
  defaultGoalTrendsView,
} from "@/goals/trends/goalTrendsTypes";
import { useRouter, useSearchParams } from "next/navigation";
import { Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import { GoalBalanceEventSort } from "@/goals/types";
import type { GoalTrendsSearchParams } from "@/goals/trends/GoalTrends";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import createTrendsBalanceEventColumns from "@/balance-events/createTrendsBalanceEventColumns";
import parseEnumValue from "@/framework/data/parseEnumValue";
import propertyName from "@/framework/data/propertyName";
import routes from "@/transactions/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the GoalTrendsBalanceEventListFrame component.
 */
interface GoalTrendsBalanceEventListFrameProps {
  readonly view: GoalTrendsView;
  readonly data: GoalBalanceEvent[] | null;
  readonly totalCount: number | null;
}

/**
 * Presents the paged balance-event table for the Goals trends.
 */
const GoalTrendsBalanceEventListFrame = function ({
  view,
  data,
  totalCount,
}: GoalTrendsBalanceEventListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const router = useRouter();

  const sortParamName =
    propertyName<GoalTrendsSearchParams>("balanceEventSort");
  const pageParamName =
    propertyName<GoalTrendsSearchParams>("balanceEventPage");
  const goalTypeParamName = propertyName<GoalTrendsSearchParams>("goalType");
  const fundNameParamName = propertyName<GoalTrendsSearchParams>("fundName");
  const startAccountingPeriodIdParamName = propertyName<GoalTrendsSearchParams>(
    "startAccountingPeriodId",
  );
  const endAccountingPeriodIdParamName = propertyName<GoalTrendsSearchParams>(
    "endAccountingPeriodId",
  );

  const updateParams = useSearchParamUpdater([pageParamName]);

  const setSort = function (sort: string | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };

  const currentSort = parseEnumValue(
    GoalBalanceEventSort,
    searchParams.get(sortParamName) ?? "",
  );

  const openTransactionWorkspace = function (
    balanceEvent: GoalBalanceEvent,
  ): void {
    router.push(
      routes.workspace({
        accountingPeriodIds: [balanceEvent.accountingPeriod.id],
        fundIds: [balanceEvent.fund.id],
        selectedTransactionId: balanceEvent.transactionId,
      }),
    );
  };

  const hasActiveFilters =
    searchParams.getAll(goalTypeParamName).length > 0 ||
    searchParams.getAll(fundNameParamName).length > 0 ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName);

  const getSortProps = createColumnSortProps(currentSort, setSort);

  const leadingColumns: ColumnDefinition<GoalBalanceEvent>[] = [
    {
      name: "fundName",
      headerContent: "Fund",
      getBodyContent: (balanceEvent) => balanceEvent.fund.name,
      ...getSortProps(
        GoalBalanceEventSort.FundName,
        GoalBalanceEventSort.FundNameDescending,
      ),
      minWidth: 160,
    },
    {
      name: "accountingPeriodName",
      headerContent: "Accounting Period",
      getBodyContent: (balanceEvent) => balanceEvent.accountingPeriod.name,
      ...getSortProps(
        GoalBalanceEventSort.AccountingPeriodName,
        GoalBalanceEventSort.AccountingPeriodNameDescending,
      ),
      minWidth: 180,
    },
  ];

  const columns = createTrendsBalanceEventColumns({
    leadingColumns,
    getSortProps,
    dateSort: {
      ascending: GoalBalanceEventSort.Date,
      descending: GoalBalanceEventSort.DateDescending,
    },
    amountSort: {
      ascending: GoalBalanceEventSort.Amount,
      descending: GoalBalanceEventSort.AmountDescending,
    },
    onOpen: openTransactionWorkspace,
    amountMinWidth: 130,
  });

  return (
    <ListFrame<GoalBalanceEvent>
      title={view === "assignment" ? "Assignment Events" : "Spending Events"}
      columns={columns}
      getId={(balanceEvent) => balanceEvent.transactionId}
      data={data ?? null}
      totalCount={totalCount ?? null}
      pageParamName={pageParamName}
      onRowClick={openTransactionWorkspace}
      hasActiveFilters={hasActiveFilters}
      initialEmptyState={{
        title:
          view === "assignment"
            ? "No assignment events are available"
            : "No spending events are available",
        description:
          view === "assignment"
            ? "Post or assign transactions to see assignment activity here."
            : "Post spending transactions to see spending activity here.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              updateParams((params) => {
                [...params.keys()].forEach((key) => {
                  params.delete(key);
                });
                if (view !== defaultGoalTrendsView) {
                  params.set(
                    propertyName<GoalTrendsSearchParams>("view"),
                    view,
                  );
                }
              });
            }}
          >
            Reset filters
          </Button>
        ),
      }}
      filteredEmptyState={{
        title:
          view === "assignment"
            ? "No assignment events match this trends filter"
            : "No spending events match this trends filter",
        description:
          "Try a different fund name, goal type, or accounting period to widen the trends scope.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              updateParams((params) => {
                [...params.keys()].forEach((key) => {
                  params.delete(key);
                });
                if (view !== defaultGoalTrendsView) {
                  params.set(
                    propertyName<GoalTrendsSearchParams>("view"),
                    view,
                  );
                }
              });
            }}
          >
            Reset filters
          </Button>
        ),
      }}
    />
  );
};

export default GoalTrendsBalanceEventListFrame;
