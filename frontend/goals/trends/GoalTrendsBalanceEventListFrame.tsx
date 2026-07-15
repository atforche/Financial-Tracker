"use client";

import { Button, IconButton, Paper, Stack, Typography } from "@mui/material";
import {
  type GoalBalanceEvent,
  type GoalTrendsView,
  defaultGoalTrendsView,
} from "@/goals/trends/goalTrendsTypes";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { GoalBalanceEventSort } from "@/goals/types";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import formatShortDate from "@/framework/formatShortDate";
import routes from "@/transactions/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";
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
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "balanceEventSort";
  const pageParamName = "balanceEventPage";
  const goalTypeParamName = "goalType";
  const fundNameParamName = "fundName";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";

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

  const currentSort = tryParseEnum(
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

  const columns: ColumnDefinition<GoalBalanceEvent>[] = [
    {
      name: "fundName",
      headerContent: "Fund",
      getBodyContent: (balanceEvent) => balanceEvent.fund.name,
      ...getSortProps(
        GoalBalanceEventSort.Fund,
        GoalBalanceEventSort.FundDescending,
      ),
      minWidth: 160,
    },
    {
      name: "accountingPeriodName",
      headerContent: "Accounting Period",
      getBodyContent: (balanceEvent) => balanceEvent.accountingPeriod.name,
      ...getSortProps(
        GoalBalanceEventSort.AccountingPeriod,
        GoalBalanceEventSort.AccountingPeriodDescending,
      ),
      minWidth: 180,
    },
    {
      name: "date",
      headerContent: "Event Date",
      getBodyContent: (balanceEvent) =>
        balanceEvent.isPosted
          ? formatShortDate(new Date(`${balanceEvent.date}T00:00:00`))
          : "Pending",
      ...getSortProps(
        GoalBalanceEventSort.Date,
        GoalBalanceEventSort.DateDescending,
      ),
      minWidth: 130,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (balanceEvent) => formatCurrency(balanceEvent.amount),
      ...getSortProps(
        GoalBalanceEventSort.Amount,
        GoalBalanceEventSort.AmountDescending,
      ),
      alignment: "right",
      minWidth: 130,
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (balanceEvent) => (
        <IconButton
          size="small"
          color="primary"
          onClick={(event) => {
            event.stopPropagation();
            openTransactionWorkspace(balanceEvent);
          }}
          aria-label={`Open transaction ${balanceEvent.transactionId}`}
        >
          <ArrowForwardOutlined fontSize="small" color="action" />
        </IconButton>
      ),
      alignment: "right",
      minWidth: 52,
      maxWidth: 52,
    },
  ];

  return (
    <ListFrame<GoalBalanceEvent>
      title={view === "assignment" ? "Assignment Events" : "Spending Events"}
      columns={columns}
      getId={(balanceEvent) => balanceEvent.transactionId}
      data={data ?? null}
      totalCount={totalCount ?? null}
      searchParamName="balanceEventSearch"
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
                [...params.keys()].forEach((key) => params.delete(key));
                if (view !== defaultGoalTrendsView) {
                  params.set("view", view);
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
                [...params.keys()].forEach((key) => params.delete(key));
                if (view !== defaultGoalTrendsView) {
                  params.set("view", view);
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
