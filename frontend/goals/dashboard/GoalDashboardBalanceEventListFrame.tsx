"use client";

import { Button, IconButton, Paper, Stack, Typography } from "@mui/material";
import {
  type GoalDashboardBalanceEvent,
  type GoalDashboardView,
  defaultGoalDashboardView,
} from "@/goals/dashboard/goalDashboardTypes";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import { GoalDashboardBalanceEventSortOrder } from "@/goals/types";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/transactions/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

const dateFormatter = new Intl.DateTimeFormat("en-US", {
  month: "short",
  day: "numeric",
  year: "numeric",
});

/**
 * Props for the GoalDashboardBalanceEventListFrame component.
 */
interface GoalDashboardBalanceEventListFrameProps {
  readonly view: GoalDashboardView;
  readonly data: GoalDashboardBalanceEvent[] | null;
  readonly totalCount: number | null;
}

const getResetRoute = function (
  pathname: string,
  view: GoalDashboardView,
): string {
  if (view === defaultGoalDashboardView) {
    return pathname;
  }

  return `${pathname}?view=${view}`;
};

/**
 * Presents the paged balance-event table for the Goals dashboard.
 */
const GoalDashboardBalanceEventListFrame = function ({
  view,
  data,
  totalCount,
}: GoalDashboardBalanceEventListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "balanceEventSort";
  const pageParamName = "balanceEventPage";
  const goalTypeParamName = "goalType";
  const fundNameParamName = "fundName";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";

  const setSort = function (
    sort: GoalDashboardBalanceEventSortOrder | null,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete(sortParamName);
    } else {
      params.set(sortParamName, sort);
    }
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`, { scroll: false });
  };

  const currentSort = tryParseEnum(
    GoalDashboardBalanceEventSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

  const openTransactionWorkspace = function (
    balanceEvent: GoalDashboardBalanceEvent,
  ): void {
    router.push(
      routes.workspace({
        accountingPeriodIds: [balanceEvent.accountingPeriodId],
        fundIds: [balanceEvent.fundId],
        selectedTransactionId: balanceEvent.transactionId,
      }),
    );
  };

  const hasActiveFilters =
    searchParams.getAll(goalTypeParamName).length > 0 ||
    searchParams.getAll(fundNameParamName).length > 0 ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName);

  const columns: ColumnDefinition<GoalDashboardBalanceEvent>[] = [
    {
      name: "fundName",
      headerContent: "Fund",
      getBodyContent: (balanceEvent) => balanceEvent.fundName,
      sortType:
        currentSort === GoalDashboardBalanceEventSortOrder.FundName
          ? ColumnSortType.Ascending
          : currentSort ===
              GoalDashboardBalanceEventSortOrder.FundNameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(GoalDashboardBalanceEventSortOrder.FundName);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(GoalDashboardBalanceEventSortOrder.FundNameDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 160,
    },
    {
      name: "accountingPeriodName",
      headerContent: "Accounting Period",
      getBodyContent: (balanceEvent) => balanceEvent.accountingPeriodName,
      sortType:
        currentSort === GoalDashboardBalanceEventSortOrder.AccountingPeriodName
          ? ColumnSortType.Ascending
          : currentSort ===
              GoalDashboardBalanceEventSortOrder.AccountingPeriodNameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(GoalDashboardBalanceEventSortOrder.AccountingPeriodName);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            GoalDashboardBalanceEventSortOrder.AccountingPeriodNameDescending,
          );
        } else {
          setSort(null);
        }
      },
      minWidth: 180,
    },
    {
      name: "date",
      headerContent: "Event Date",
      getBodyContent: (balanceEvent) =>
        balanceEvent.isPosted
          ? dateFormatter.format(new Date(`${balanceEvent.date}T00:00:00`))
          : "Pending",
      sortType:
        currentSort === GoalDashboardBalanceEventSortOrder.Date
          ? ColumnSortType.Ascending
          : currentSort === GoalDashboardBalanceEventSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(GoalDashboardBalanceEventSortOrder.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(GoalDashboardBalanceEventSortOrder.DateDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 130,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (balanceEvent) => formatCurrency(balanceEvent.amount),
      sortType:
        currentSort === GoalDashboardBalanceEventSortOrder.Amount
          ? ColumnSortType.Ascending
          : currentSort === GoalDashboardBalanceEventSortOrder.AmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(GoalDashboardBalanceEventSortOrder.Amount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(GoalDashboardBalanceEventSortOrder.AmountDescending);
        } else {
          setSort(null);
        }
      },
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
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2.5}>
        <Typography variant="h5">
          {view === "assignment" ? "Assignment Events" : "Spending Events"}
        </Typography>
        <ListFrame<GoalDashboardBalanceEvent>
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
                  router.replace(getResetRoute(pathname, view));
                }}
              >
                Reset filters
              </Button>
            ),
          }}
          filteredEmptyState={{
            title:
              view === "assignment"
                ? "No assignment events match this dashboard filter"
                : "No spending events match this dashboard filter",
            description:
              "Try a different fund name, goal type, or accounting period to widen the dashboard scope.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.replace(getResetRoute(pathname, view));
                }}
              >
                Reset filters
              </Button>
            ),
          }}
        />
      </Stack>
    </Paper>
  );
};

export default GoalDashboardBalanceEventListFrame;
