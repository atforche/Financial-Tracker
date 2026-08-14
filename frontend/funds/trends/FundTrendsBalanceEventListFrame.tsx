"use client";

import { type FundBalanceEvent, FundBalanceEventSort } from "@/funds/types";
import {
  clearFundTrendsFilters,
  fundTrendsParamNames,
  hasActiveFundTrendsFilters,
} from "@/funds/trends/helpers";
import { useRouter, useSearchParams } from "next/navigation";
import { Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import createTrendsBalanceEventColumns from "@/balance-events/createTrendsBalanceEventColumns";
import { formatBalanceEventCounterparty } from "@/balance-events/helpers";
import parseEnumValue from "@/framework/data/parseEnumValue";
import routes from "@/transactions/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the FundTrendsBalanceEventListFrame component.
 */
interface FundTrendsBalanceEventListFrameProps {
  readonly data: FundBalanceEvent[] | null;
  readonly totalCount: number | null;
  readonly mode: "AccountingPeriod" | "Date";
}

/**
 * Presents the paged balance event table for the Funds trends.
 */
const FundTrendsBalanceEventListFrame = function ({
  data,
  mode,
  totalCount,
}: FundTrendsBalanceEventListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const router = useRouter();

  const sortParamName = fundTrendsParamNames.balanceEventSort;
  const pageParamName = fundTrendsParamNames.balanceEventPage;

  const updateParams = useSearchParamUpdater([pageParamName]);

  const setSort = function (sort: FundBalanceEventSort | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };

  const currentSort = parseEnumValue(
    FundBalanceEventSort,
    searchParams.get(sortParamName) ?? "",
  );

  const openTransactionWorkspace = function (
    balanceEvent: FundBalanceEvent,
  ): void {
    router.push(
      routes.workspace({
        accountingPeriodIds: [balanceEvent.accountingPeriod.id],
        fundIds: [balanceEvent.fund.id],
        selectedTransactionId: balanceEvent.transactionId,
      }),
    );
  };
  const hasActiveFilters = hasActiveFundTrendsFilters(searchParams);

  const getSortProps = createColumnSortProps(currentSort, setSort);

  const fundColumn: ColumnDefinition<FundBalanceEvent> = {
    name: "fundName",
    headerContent: "Fund",
    getBodyContent: (balanceEvent) => balanceEvent.fund.name,
    ...getSortProps(
      FundBalanceEventSort.FundName,
      FundBalanceEventSort.FundNameDescending,
    ),
    minWidth: 140,
  };
  const leadingColumns: ColumnDefinition<FundBalanceEvent>[] = [];

  if (mode === "AccountingPeriod") {
    leadingColumns.push({
      name: "accountingPeriodName",
      headerContent: "Accounting Period",
      getBodyContent: (balanceEvent) => balanceEvent.accountingPeriod.name,
      ...getSortProps(
        FundBalanceEventSort.AccountingPeriodName,
        FundBalanceEventSort.AccountingPeriodNameDescending,
      ),
      minWidth: 160,
    });
  }

  const columns = createTrendsBalanceEventColumns({
    leadingColumns,
    afterDateColumns: [fundColumn],
    getSortProps,
    dateSort: {
      ascending: FundBalanceEventSort.Date,
      descending: FundBalanceEventSort.DateDescending,
    },
    typeSort: {
      ascending: FundBalanceEventSort.Type,
      descending: FundBalanceEventSort.TypeDescending,
    },
    typeBeforeDetailColumns: true,
    amountSort: {
      ascending: FundBalanceEventSort.Amount,
      descending: FundBalanceEventSort.AmountDescending,
    },
    detailColumns: [
      {
        name: "flow",
        headerContent: "To / From",
        getBodyContent: formatBalanceEventCounterparty,
        ...getSortProps(
          FundBalanceEventSort.Counterparty,
          FundBalanceEventSort.CounterpartyDescending,
        ),
        minWidth: 190,
      },
    ],
    onOpen: openTransactionWorkspace,
  });

  return (
    <ListFrame<FundBalanceEvent>
      title="Balance Events"
      desktopBreakpoint="xl"
      columns={columns}
      getId={(balanceEvent) =>
        `${balanceEvent.transactionId}-${balanceEvent.fund.id}-${balanceEvent.accountingPeriod.id}-${balanceEvent.eventDate}-${balanceEvent.type}-${balanceEvent.amount}`
      }
      data={data ?? null}
      totalCount={totalCount ?? null}
      pageParamName={pageParamName}
      hasActiveFilters={hasActiveFilters}
      onRowClick={(balanceEvent) => {
        openTransactionWorkspace(balanceEvent);
      }}
      initialEmptyState={{
        title: "No Balance Events Found",
        description:
          "Try a different date range or accounting period to inspect account activity.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              updateParams((params) => {
                [...params.keys()].forEach((key) => {
                  params.delete(key);
                });
              });
            }}
          >
            Reset trends
          </Button>
        ),
      }}
      filteredEmptyState={{
        title: "No Balance Events Match This Trends Filter",
        description:
          "Try a different fund filter or range to widen the activity feed.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              updateParams((params) => {
                clearFundTrendsFilters(params);
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

export default FundTrendsBalanceEventListFrame;
