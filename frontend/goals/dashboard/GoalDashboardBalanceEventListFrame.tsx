"use client";

import {
  Box,
  Button,
  IconButton,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import {
  type GoalDashboardBalanceEvent,
  GoalDashboardBalanceEventSortOrder,
  GoalDashboardBalanceEventType,
} from "@/goals/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
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

const formatBalanceEventType = function (
  type: GoalDashboardBalanceEventType,
): string {
  return type === GoalDashboardBalanceEventType.Assignment
    ? "Assignment"
    : "Spending";
};

/**
 * Props for the GoalDashboardBalanceEventListFrame component.
 */
interface GoalDashboardBalanceEventListFrameProps {
  readonly data: GoalDashboardBalanceEvent[] | null;
  readonly totalCount: number | null;
}

/**
 * Presents the paged balance-event table for the Goals dashboard.
 */
const GoalDashboardBalanceEventListFrame = function ({
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
      name: "type",
      headerContent: "Type",
      getBodyContent: (balanceEvent): JSX.Element => (
        <Box
          component="span"
          sx={{
            color:
              balanceEvent.type === GoalDashboardBalanceEventType.Assignment
                ? "info.dark"
                : "warning.dark",
            fontWeight: 600,
          }}
        >
          {formatBalanceEventType(balanceEvent.type)}
        </Box>
      ),
      sortType:
        currentSort === GoalDashboardBalanceEventSortOrder.Type
          ? ColumnSortType.Ascending
          : currentSort === GoalDashboardBalanceEventSortOrder.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(GoalDashboardBalanceEventSortOrder.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(GoalDashboardBalanceEventSortOrder.TypeDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 110,
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
        <Typography variant="h5">Balance Events</Typography>
        <ListFrame<GoalDashboardBalanceEvent>
          columns={columns}
          getId={(balanceEvent) =>
            `${balanceEvent.fundId}-${balanceEvent.accountingPeriodId}-${balanceEvent.date}-${balanceEvent.type}-${balanceEvent.amount}`
          }
          data={data ?? null}
          totalCount={totalCount ?? null}
          searchParamName="balanceEventSearch"
          pageParamName={pageParamName}
          hasActiveFilters={hasActiveFilters}
          onRowClick={(balanceEvent) => {
            openTransactionWorkspace(balanceEvent);
          }}
          initialEmptyState={{
            title: "No balance events found",
            description:
              "Try a different date range or accounting period to inspect goal activity.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.replace(pathname);
                }}
              >
                Reset dashboard
              </Button>
            ),
          }}
          filteredEmptyState={{
            title: "No balance events match this dashboard filter",
            description:
              "Try a different fund filter or range to widen the activity feed.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.replace(pathname);
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
