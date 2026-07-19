"use client";

import {
  type AccountBalanceEvent,
  AccountBalanceEventSort,
} from "@/accounts/types";
import {
  type AccountTrendsDataMode,
  accountTrendsParamNames,
  clearAccountTrendsFilters,
  hasActiveAccountTrendsFilters,
} from "@/accounts/trends/helpers";
import { Box, Button, IconButton } from "@mui/material";
import { useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import { BalanceEventType } from "@/framework/data/types";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { formatBalanceEventType } from "@/framework/data/helpers";
import { formatCurrency } from "@/framework/currencyHelpers";
import { formatShortDate } from "@/framework/dateHelpers";
import routes from "@/transactions/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the AccountTrendsBalanceEventListFrame component.
 */
interface AccountTrendsBalanceEventListFrameProps {
  readonly data: readonly AccountBalanceEvent[];
  readonly totalCount: number;
  readonly mode: AccountTrendsDataMode;
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
  const router = useRouter();

  const { balanceEventSort: sortParamName, balanceEventPage: pageParamName } =
    accountTrendsParamNames;

  const updateParams = useSearchParamUpdater([pageParamName]);

  const setSort = function (sort: AccountBalanceEventSort | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };

  const currentSort = tryParseEnum(
    AccountBalanceEventSort,
    searchParams.get(sortParamName) ?? "",
  );

  const openTransactionWorkspace = function (
    balanceEvent: AccountBalanceEvent,
  ): void {
    router.push(
      routes.workspace({
        accountingPeriodIds: [balanceEvent.accountingPeriod.id],
        accountIds: [balanceEvent.account.id],
        selectedTransactionId: balanceEvent.transactionId,
      }),
    );
  };
  const hasActiveFilters = hasActiveAccountTrendsFilters(searchParams);

  const getSortProps = createColumnSortProps(currentSort, setSort);

  const accountingPeriodColumns: ColumnDefinition<AccountBalanceEvent>[] =
    mode === "AccountingPeriod"
      ? [
          {
            name: "accountingPeriodName",
            headerContent: "Accounting Period",
            getBodyContent: (balanceEvent) =>
              balanceEvent.accountingPeriod.name,
            ...getSortProps(
              AccountBalanceEventSort.AccountingPeriodName,
              AccountBalanceEventSort.AccountingPeriodNameDescending,
            ),
            minWidth: 160,
          },
        ]
      : [];

  const columns: ColumnDefinition<AccountBalanceEvent>[] = [
    {
      name: "accountName",
      headerContent: "Account",
      getBodyContent: (balanceEvent) => balanceEvent.account.name,
      ...getSortProps(
        AccountBalanceEventSort.AccountName,
        AccountBalanceEventSort.AccountNameDescending,
      ),
      minWidth: 140,
    },
    ...accountingPeriodColumns,
    {
      name: "date",
      headerContent: "Event Date",
      getBodyContent: (balanceEvent) =>
        balanceEvent.isPosted
          ? formatShortDate(new Date(`${balanceEvent.date}T00:00:00`))
          : "Pending",
      ...getSortProps(
        AccountBalanceEventSort.Date,
        AccountBalanceEventSort.DateDescending,
      ),
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
              balanceEvent.type === BalanceEventType.Debit
                ? "warning.dark"
                : "info.dark",
            fontWeight: 600,
          }}
        >
          {formatBalanceEventType(balanceEvent.type, balanceEvent.isPosted)}
        </Box>
      ),
      ...getSortProps(
        AccountBalanceEventSort.Type,
        AccountBalanceEventSort.TypeDescending,
      ),
      minWidth: 90,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (balanceEvent) => formatCurrency(balanceEvent.amount),
      ...getSortProps(
        AccountBalanceEventSort.Amount,
        AccountBalanceEventSort.AmountDescending,
      ),
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

  return (
    <ListFrame<AccountBalanceEvent>
      title="Balance Events"
      columns={columns}
      getId={(balanceEvent) =>
        `${balanceEvent.transactionId}-${balanceEvent.account.id}-${balanceEvent.date ?? "pending"}-${balanceEvent.type}-${balanceEvent.amount}`
      }
      data={[...data]}
      totalCount={totalCount}
      pageParamName={pageParamName}
      hasActiveFilters={hasActiveFilters}
      onRowClick={(balanceEvent) => {
        openTransactionWorkspace(balanceEvent);
      }}
      initialEmptyState={{
        title: "No balance events found",
        description:
          "Try a different date range or accounting period to inspect account activity.",
        action: null,
      }}
      filteredEmptyState={{
        title: "No balance events match this trends filter",
        description:
          "Try a different account filter or range to widen the activity feed.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              updateParams((params) => {
                clearAccountTrendsFilters(params);
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

export default AccountTrendsBalanceEventListFrame;
