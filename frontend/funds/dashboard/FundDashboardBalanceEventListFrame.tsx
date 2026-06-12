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
  type FundDashboardBalanceEvent,
  FundDashboardBalanceEventSortOrder,
  FundDashboardBalanceEventType,
  FundDashboardMode,
} from "@/funds/types";
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
  type: FundDashboardBalanceEventType,
): string {
  return type === FundDashboardBalanceEventType.Debit ? "Debit" : "Credit";
};

/**
 * Props for the FundDashboardBalanceEventListFrame component.
 */
interface FundDashboardBalanceEventListFrameProps {
  readonly data: FundDashboardBalanceEvent[] | null;
  readonly totalCount: number | null;
  readonly mode: FundDashboardMode;
}

/**
 * Presents the paged balance event table for the Funds dashboard.
 */
const FundDashboardBalanceEventListFrame = function ({
  data,
  mode,
  totalCount,
}: FundDashboardBalanceEventListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "balanceEventSort";
  const pageParamName = "balanceEventPage";
  const fundNameParamName = "fundName";
  const modeParamName = "mode";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";
  const startDateParamName = "startDate";
  const endDateParamName = "endDate";

  const setSort = function (
    sort: FundDashboardBalanceEventSortOrder | null,
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
    FundDashboardBalanceEventSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

  const openTransactionWorkspace = function (
    balanceEvent: FundDashboardBalanceEvent,
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
    searchParams.getAll(fundNameParamName).length > 0 ||
    searchParams.get(modeParamName) === "date" ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName) ||
    searchParams.has(startDateParamName) ||
    searchParams.has(endDateParamName);

  const columns: ColumnDefinition<FundDashboardBalanceEvent>[] = [
    {
      name: "fundName",
      headerContent: "Fund",
      getBodyContent: (balanceEvent) => balanceEvent.fundName,
      sortType:
        currentSort === FundDashboardBalanceEventSortOrder.FundName
          ? ColumnSortType.Ascending
          : currentSort ===
              FundDashboardBalanceEventSortOrder.FundNameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundDashboardBalanceEventSortOrder.FundName);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundDashboardBalanceEventSortOrder.FundNameDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 140,
    },
    {
      name: "date",
      headerContent: "Event Date",
      getBodyContent: (balanceEvent) =>
        balanceEvent.isPosted
          ? dateFormatter.format(new Date(`${balanceEvent.date}T00:00:00`))
          : "Pending",
      sortType:
        currentSort === FundDashboardBalanceEventSortOrder.Date
          ? ColumnSortType.Ascending
          : currentSort === FundDashboardBalanceEventSortOrder.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundDashboardBalanceEventSortOrder.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundDashboardBalanceEventSortOrder.DateDescending);
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
              balanceEvent.type === FundDashboardBalanceEventType.Debit
                ? "warning.dark"
                : "info.dark",
            fontWeight: 600,
          }}
        >
          {formatBalanceEventType(balanceEvent.type)}
        </Box>
      ),
      sortType:
        currentSort === FundDashboardBalanceEventSortOrder.Type
          ? ColumnSortType.Ascending
          : currentSort === FundDashboardBalanceEventSortOrder.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundDashboardBalanceEventSortOrder.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundDashboardBalanceEventSortOrder.TypeDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 90,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (balanceEvent) => formatCurrency(balanceEvent.amount),
      sortType:
        currentSort === FundDashboardBalanceEventSortOrder.Amount
          ? ColumnSortType.Ascending
          : currentSort === FundDashboardBalanceEventSortOrder.AmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundDashboardBalanceEventSortOrder.Amount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundDashboardBalanceEventSortOrder.AmountDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 120,
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

  if (mode === FundDashboardMode.AccountingPeriod) {
    columns.splice(1, 0, {
      name: "accountingPeriodName",
      headerContent: "Accounting Period",
      getBodyContent: (balanceEvent) => balanceEvent.accountingPeriodName,
      sortType:
        currentSort === FundDashboardBalanceEventSortOrder.AccountingPeriodName
          ? ColumnSortType.Ascending
          : currentSort ===
              FundDashboardBalanceEventSortOrder.AccountingPeriodNameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundDashboardBalanceEventSortOrder.AccountingPeriodName);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(
            FundDashboardBalanceEventSortOrder.AccountingPeriodNameDescending,
          );
        } else {
          setSort(null);
        }
      },
      minWidth: 160,
    });
  }

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
        <ListFrame<FundDashboardBalanceEvent>
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
              "Try a different date range or accounting period to inspect account activity.",
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
              "Try a different account filter or range to widen the activity feed.",
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

export default FundDashboardBalanceEventListFrame;
