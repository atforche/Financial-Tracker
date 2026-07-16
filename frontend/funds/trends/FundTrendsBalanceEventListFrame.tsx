"use client";

import { Box, Button, IconButton } from "@mui/material";
import { type FundBalanceEvent, FundBalanceEventSort } from "@/funds/types";
import { useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import { BalanceEventType } from "@/framework/data/types";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { FundTrendsSearchParams } from "@/funds/trends/FundTrends";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { formatCurrency } from "@/framework/currencyHelpers";
import formatShortDate from "@/framework/formatShortDate";
import nameof from "@/framework/data/nameof";
import routes from "@/transactions/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

const formatBalanceEventType = function (type: BalanceEventType): string {
  return type === BalanceEventType.Debit ? "Debit" : "Credit";
};

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

  const sortParamName = nameof<FundTrendsSearchParams>("balanceEventSort");
  const pageParamName = nameof<FundTrendsSearchParams>("balanceEventPage");
  const fundNameParamName = nameof<FundTrendsSearchParams>("fundName");
  const modeParamName = nameof<FundTrendsSearchParams>("mode");
  const startAccountingPeriodIdParamName = nameof<FundTrendsSearchParams>(
    "startAccountingPeriodId",
  );
  const endAccountingPeriodIdParamName = nameof<FundTrendsSearchParams>(
    "endAccountingPeriodId",
  );
  const startDateParamName = nameof<FundTrendsSearchParams>("startDate");
  const endDateParamName = nameof<FundTrendsSearchParams>("endDate");

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

  const currentSort = tryParseEnum(
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
  const hasActiveFilters =
    searchParams.getAll(fundNameParamName).length > 0 ||
    searchParams.get(modeParamName) === "date" ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName) ||
    searchParams.has(startDateParamName) ||
    searchParams.has(endDateParamName);

  const getSortProps = createColumnSortProps(currentSort, setSort);

  const columns: ColumnDefinition<FundBalanceEvent>[] = [
    {
      name: "fundName",
      headerContent: "Fund",
      getBodyContent: (balanceEvent) => balanceEvent.fund.name,
      ...getSortProps(
        FundBalanceEventSort.FundName,
        FundBalanceEventSort.FundNameDescending,
      ),
      minWidth: 140,
    },
    {
      name: "date",
      headerContent: "Event Date",
      getBodyContent: (balanceEvent) =>
        balanceEvent.isPosted
          ? formatShortDate(new Date(`${balanceEvent.date}T00:00:00`))
          : "Pending",
      ...getSortProps(
        FundBalanceEventSort.Date,
        FundBalanceEventSort.DateDescending,
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
          {formatBalanceEventType(balanceEvent.type)}
        </Box>
      ),
      ...getSortProps(
        FundBalanceEventSort.Type,
        FundBalanceEventSort.TypeDescending,
      ),
      minWidth: 90,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (balanceEvent) => formatCurrency(balanceEvent.amount),
      ...getSortProps(
        FundBalanceEventSort.Amount,
        FundBalanceEventSort.AmountDescending,
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

  if (mode === "AccountingPeriod") {
    columns.splice(1, 0, {
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

  return (
    <ListFrame<FundBalanceEvent>
      title="Balance Events"
      columns={columns}
      getId={(balanceEvent) =>
        `${balanceEvent.fund.id}-${balanceEvent.accountingPeriod.id}-${balanceEvent.date}-${balanceEvent.type}-${balanceEvent.amount}`
      }
      data={data ?? null}
      totalCount={totalCount ?? null}
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
        title: "No balance events match this trends filter",
        description:
          "Try a different account filter or range to widen the activity feed.",
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
            Reset filters
          </Button>
        ),
      }}
    />
  );
};

export default FundTrendsBalanceEventListFrame;
