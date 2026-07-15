"use client";

import {
  AccountBalanceEventSort,
  type AccountBalanceEventSortValue,
  type AccountWorkspaceBalanceEvent,
} from "@/accounts/types";
import { BalanceEventTypeModel } from "@/framework/data/api";
import {
  Box,
  Button,
  IconButton,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import formatShortDate from "@/framework/formatShortDate";
import routes from "@/transactions/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

const formatBalanceEventType = function (type: BalanceEventTypeModel): string {
  return type === BalanceEventTypeModel.Debit ? "Debit" : "Credit";
};

/**
 * Props for the AccountTrendsBalanceEventListFrame component.
 */
interface AccountTrendsBalanceEventListFrameProps {
  readonly data: AccountWorkspaceBalanceEvent[] | null;
  readonly totalCount: number | null;
  readonly mode: "AccountingPeriod" | "Date";
}

/**
 * Presents the paged balance event table for the Accounts trends.
 */
const AccountTrendsBalanceEventListFrame = function ({
  data,
  mode,
  totalCount,
}: AccountTrendsBalanceEventListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "balanceEventSort";
  const pageParamName = "balanceEventPage";
  const accountTypeParamName = "accountType";
  const accountNameParamName = "accountName";
  const modeParamName = "mode";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";
  const startDateParamName = "startDate";
  const endDateParamName = "endDate";

  const setSort = function (sort: AccountBalanceEventSortValue | null): void {
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
    AccountBalanceEventSort,
    searchParams.get(sortParamName) ?? "",
  );

  const openTransactionWorkspace = function (
    balanceEvent: AccountWorkspaceBalanceEvent,
  ): void {
    router.push(
      routes.workspace({
        accountingPeriodIds: [balanceEvent.accountingPeriod.id],
        accountIds: [balanceEvent.account.id],
        selectedTransactionId: balanceEvent.transactionId,
      }),
    );
  };
  const hasActiveFilters =
    searchParams.getAll(accountTypeParamName).length > 0 ||
    searchParams.getAll(accountNameParamName).length > 0 ||
    searchParams.get(modeParamName) === "date" ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName) ||
    searchParams.has(startDateParamName) ||
    searchParams.has(endDateParamName);

  const columns: ColumnDefinition<AccountWorkspaceBalanceEvent>[] = [
    {
      name: "accountName",
      headerContent: "Account",
      getBodyContent: (balanceEvent) => balanceEvent.account.name,
      sortType:
        currentSort === AccountBalanceEventSort.AccountName
          ? ColumnSortType.Ascending
          : currentSort === AccountBalanceEventSort.AccountNameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountBalanceEventSort.AccountName);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountBalanceEventSort.AccountNameDescending);
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
          ? formatShortDate(new Date(`${balanceEvent.date}T00:00:00`))
          : "Pending",
      sortType:
        currentSort === AccountBalanceEventSort.Date
          ? ColumnSortType.Ascending
          : currentSort === AccountBalanceEventSort.DateDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountBalanceEventSort.Date);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountBalanceEventSort.DateDescending);
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
              balanceEvent.type === BalanceEventTypeModel.Debit
                ? "warning.dark"
                : "info.dark",
            fontWeight: 600,
          }}
        >
          {formatBalanceEventType(balanceEvent.type)}
        </Box>
      ),
      sortType:
        currentSort === AccountBalanceEventSort.Type
          ? ColumnSortType.Ascending
          : currentSort === AccountBalanceEventSort.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountBalanceEventSort.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountBalanceEventSort.TypeDescending);
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
        currentSort === AccountBalanceEventSort.Amount
          ? ColumnSortType.Ascending
          : currentSort === AccountBalanceEventSort.AmountDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountBalanceEventSort.Amount);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountBalanceEventSort.AmountDescending);
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

  if (mode === "AccountingPeriod") {
    columns.splice(1, 0, {
      name: "accountingPeriodName",
      headerContent: "Accounting Period",
      getBodyContent: (balanceEvent) => balanceEvent.accountingPeriod.name,
      sortType:
        currentSort === AccountBalanceEventSort.AccountingPeriodName
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountBalanceEventSort.AccountingPeriodNameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountBalanceEventSort.AccountingPeriodName);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountBalanceEventSort.AccountingPeriodNameDescending);
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
        <ListFrame<AccountWorkspaceBalanceEvent>
          columns={columns}
          getId={(balanceEvent) =>
            `${balanceEvent.account.id}-${balanceEvent.accountingPeriod.id}-${balanceEvent.date}-${balanceEvent.type}-${balanceEvent.amount}`
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
                Reset trends
              </Button>
            ),
          }}
          filteredEmptyState={{
            title: "No balance events match this trends filter",
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

export default AccountTrendsBalanceEventListFrame;
