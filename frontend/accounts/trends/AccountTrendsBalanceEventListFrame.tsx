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
import { useRouter, useSearchParams } from "next/navigation";
import { Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import createTrendsBalanceEventColumns from "@/balance-events/createTrendsBalanceEventColumns";
import parseEnumValue from "@/framework/data/parseEnumValue";
import routes from "@/transactions/routes";
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

  const currentSort = parseEnumValue(
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

  const leadingColumns: ColumnDefinition<AccountBalanceEvent>[] = [
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
  ];

  if (mode === "AccountingPeriod") {
    leadingColumns.push({
      name: "accountingPeriodName",
      headerContent: "Accounting Period",
      getBodyContent: (balanceEvent) => balanceEvent.accountingPeriod.name,
      ...getSortProps(
        AccountBalanceEventSort.AccountingPeriodName,
        AccountBalanceEventSort.AccountingPeriodNameDescending,
      ),
      minWidth: 160,
    });
  }

  const columns = createTrendsBalanceEventColumns({
    leadingColumns,
    getSortProps,
    dateSort: {
      ascending: AccountBalanceEventSort.Date,
      descending: AccountBalanceEventSort.DateDescending,
    },
    typeSort: {
      ascending: AccountBalanceEventSort.Type,
      descending: AccountBalanceEventSort.TypeDescending,
    },
    amountSort: {
      ascending: AccountBalanceEventSort.Amount,
      descending: AccountBalanceEventSort.AmountDescending,
    },
    onOpen: openTransactionWorkspace,
  });

  return (
    <ListFrame<AccountBalanceEvent>
      title="Balance Events"
      columns={columns}
      getId={(balanceEvent) =>
        `${balanceEvent.transactionId}-${balanceEvent.account.id}-${balanceEvent.date ?? "pending"}-${balanceEvent.type}-${balanceEvent.amount}`
      }
      data={data}
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
