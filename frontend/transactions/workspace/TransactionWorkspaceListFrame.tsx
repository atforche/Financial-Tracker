"use client";

import { Box, Button, Chip } from "@mui/material";
import PagedTable, { type PagedTableColumn } from "@/framework/view/PagedTable";
import { type Transaction, TransactionSort } from "@/transactions/types";
import {
  getTransactionDestinationLabel,
  getTransactionSourceLabel,
} from "@/transactions/transactionListHelpers";
import { usePathname, useSearchParams } from "next/navigation";
import type { JSX } from "react";
import Link from "next/link";
import type { Route } from "next";
import { buildUrl } from "@/framework/routes/helpers";
import { formatCurrency } from "@/framework/currencyHelpers";
import { formatShortDate } from "@/framework/dateHelpers";
import routes from "@/transactions/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

const columns: readonly PagedTableColumn<Transaction>[] = [
  {
    name: "date",
    header: "Date",
    getValue: (transaction) =>
      formatShortDate(new Date(`${transaction.date}T00:00:00`)),
    width: "minmax(0, 0.9fr)",
    // Keeps the whole date on phones, where the description gives up the space instead
    mobileWidth: "max-content",
    sort: [TransactionSort.Date, TransactionSort.DateDescending],
    descendingFirst: true,
  },
  {
    name: "description",
    header: "Description",
    getValue: (transaction) => transaction.description,
    width: "minmax(0, 1.6fr)",
    sort: [TransactionSort.Description, TransactionSort.DescriptionDescending],
  },
  {
    name: "source",
    header: "From",
    getValue: getTransactionSourceLabel,
    width: "minmax(0, 1.2fr)",
    hideOnMobile: true,
    sort: [TransactionSort.Source, TransactionSort.SourceDescending],
  },
  {
    name: "destination",
    header: "To",
    getValue: getTransactionDestinationLabel,
    width: "minmax(0, 1.2fr)",
    hideOnMobile: true,
    sort: [TransactionSort.Destination, TransactionSort.DestinationDescending],
  },
  {
    name: "amount",
    header: "Amount",
    getValue: (transaction) => formatCurrency(transaction.amount),
    width: "minmax(0, 1fr)",
    alignment: "right",
    sort: [TransactionSort.Amount, TransactionSort.AmountDescending],
    descendingFirst: true,
  },
  {
    name: "posted",
    header: "Status",
    getValue: (transaction) =>
      transaction.fullyPosted ? (
        "Posted"
      ) : (
        <Chip label="Pending" size="small" color="warning" variant="outlined" />
      ),
    width: "minmax(0, 0.8fr)",
    alignment: "right",
    hideOnMobile: true,
    sort: [TransactionSort.FullyPosted, TransactionSort.FullyPostedDescending],
  },
];

/**
 * Search parameters cleared when the user clears the search from the empty list.
 */
const filterParamNames = [
  "accountingPeriodIds",
  "accountIds",
  "fundIds",
  "locationIds",
  "fundNames",
  "accountTypes",
  "accountNames",
  "transactionTypes",
  "fullyPosted",
  "startDate",
  "endDate",
  "startAccountingPeriodId",
  "endAccountingPeriodId",
  "page",
] as const;

/**
 * Props for the TransactionWorkspaceListFrame component.
 */
interface TransactionWorkspaceListFrameProps {
  readonly data: Transaction[];
  readonly totalCount: number;
  readonly hasActiveFilters: boolean;
}

/**
 * Lists the transactions in the chosen range, newest first unless the user sorts otherwise, each opening its details page.
 */
const TransactionWorkspaceListFrame = function ({
  data,
  hasActiveFilters,
  totalCount,
}: TransactionWorkspaceListFrameProps): JSX.Element {
  const canWrite = useWriteAccess();
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const updateParams = useSearchParamUpdater([]);

  // Pages opened from this list return to it
  const listUrl = buildUrl(pathname, new URLSearchParams(searchParams));
  const createUrl = buildUrl(
    `${pathname}/create`,
    new URLSearchParams({ returnUrl: listUrl }),
  ) as Route;

  return (
    <PagedTable
      title={hasActiveFilters ? "Matching Transactions" : "Recent Transactions"}
      headerContent={
        canWrite ? (
          <Button
            variant="contained"
            component={Link}
            href={createUrl}
            aria-label="Add transaction"
            sx={{ flexShrink: 0 }}
          >
            {/* Keeps the title and button on one line on phones */}
            <Box
              component="span"
              sx={{ display: { xs: "inline", sm: "none" } }}
            >
              Add
            </Box>
            <Box
              component="span"
              sx={{ display: { xs: "none", sm: "inline" } }}
            >
              Add Transaction
            </Box>
          </Button>
        ) : null
      }
      columns={columns}
      data={data}
      getKey={(transaction) => transaction.id}
      getHref={(transaction) =>
        routes.workspaceDetail(transaction.id, { returnUrl: listUrl })
      }
      totalCount={totalCount}
      sortParamName="sort"
      defaultSort={TransactionSort.DateDescending}
      pageParamName="page"
      emptyState={
        hasActiveFilters
          ? {
              title: "No Transactions Match These Filters",
              description:
                "Try different filters, or clear them to see every transaction.",
              action: (
                <Button
                  variant="contained"
                  onClick={() => {
                    updateParams((params) => {
                      filterParamNames.forEach((name) => {
                        params.delete(name);
                      });
                    });
                  }}
                >
                  Clear filters
                </Button>
              ),
            }
          : {
              title: "No Transactions in This Range",
              description:
                "Widen the date range to see earlier or later transactions.",
            }
      }
    />
  );
};

export default TransactionWorkspaceListFrame;
