import { Box, Button, Paper, Stack, Typography } from "@mui/material";
import {
  TransactionSortOrder,
  getPostableTransactionAccounts,
} from "@/transactions/types";
import type { AccountingPeriodIdentifier } from "@/accounting-periods/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { JSX } from "react";
import SummaryCard from "@/framework/view/SummaryCard";
import TransactionListFrame from "@/transactions/TransactionListFrame";
import TransactionsDashboardControls from "@/transactions/TransactionsDashboardControls";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import breadcrumbs from "@/transactions/breadcrumbs";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";
import routes from "@/transactions/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Search parameters for the TransactionsView component.
 */
interface TransactionsViewSearchParams {
  readonly accountingPeriodId?: string;
  readonly search?: string;
  readonly sort?: TransactionSortOrder;
  readonly page?: number;
}

/**
 * Props for the TransactionsView component.
 */
interface TransactionsViewProps {
  readonly searchParams: Promise<TransactionsViewSearchParams>;
}

/**
 * Formats the current transaction sort into human-readable text.
 */
const formatTransactionSort = function (
  sort: TransactionSortOrder | undefined,
): string {
  if (typeof sort !== "string") {
    return "Default order";
  }

  switch (sort) {
    case TransactionSortOrder.Date:
      return "Date: oldest first";
    case TransactionSortOrder.DateDescending:
      return "Date: newest first";
    case TransactionSortOrder.Location:
      return "Location: A to Z";
    case TransactionSortOrder.LocationDescending:
      return "Location: Z to A";
    case TransactionSortOrder.DebitFrom:
      return "Debit from: A to Z";
    case TransactionSortOrder.DebitFromDescending:
      return "Debit from: Z to A";
    case TransactionSortOrder.CreditTo:
      return "Credit to: A to Z";
    case TransactionSortOrder.CreditToDescending:
      return "Credit to: Z to A";
    case TransactionSortOrder.Amount:
      return "Amount: low to high";
    case TransactionSortOrder.AmountDescending:
      return "Amount: high to low";
    default:
      return "Default order";
  }
};

/**
 * Component that displays the top-level Transactions view.
 */
const TransactionsView = async function ({
  searchParams,
}: TransactionsViewProps): Promise<JSX.Element> {
  const { accountingPeriodId, search, sort, page } = await searchParams;

  const apiClient = getApiClient();
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods", {
    params: {
      query: {
        Search: "",
        Sort: null,
        Limit: 250,
        Offset: 0,
      },
    },
  });
  const openAccountingPeriodsPromise = apiClient.GET(
    "/accounting-periods/open",
  );

  const [{ data: accountingPeriods }, { data: openAccountingPeriods }] =
    await Promise.all([accountingPeriodsPromise, openAccountingPeriodsPromise]);

  if (
    typeof accountingPeriods === "undefined" ||
    typeof openAccountingPeriods === "undefined"
  ) {
    throw new Error("Failed to fetch transaction filters");
  }

  const selectedAccountingPeriod =
    typeof accountingPeriodId === "string"
      ? (accountingPeriods.items.find(
          (accountingPeriod) => accountingPeriod.id === accountingPeriodId,
        ) ?? null)
      : null;
  const currentOpenAccountingPeriod = openAccountingPeriods[0] ?? null;
  const transactionsPromise = apiClient.GET("/transactions", {
    params: {
      query: {
        AccountingPeriodId: selectedAccountingPeriod?.id ?? "",
        Search: search ?? "",
        Sort: sort ?? null,
        Limit: rowsPerPage,
        Offset: ((page ?? 1) - 1) * rowsPerPage,
      },
    },
  });

  const [{ data: transactions }] = await Promise.all([transactionsPromise]);

  if (typeof transactions === "undefined") {
    throw new Error("Failed to fetch transactions");
  }

  const accountingPeriodOptions: AccountingPeriodIdentifier[] =
    accountingPeriods.items.map((accountingPeriod) => ({
      id: accountingPeriod.id,
      name: accountingPeriod.name,
    }));
  const hasOpenAccountingPeriods = openAccountingPeriods.length > 0;
  const currentSearch = search?.trim() ?? "";
  const hasActiveSearch = currentSearch !== "";
  const visibleCount = transactions.items.length;
  const postableTransactionCount = transactions.items.filter(
    (transaction) => getPostableTransactionAccounts(transaction).length > 0,
  ).length;
  const createActionHref = hasOpenAccountingPeriods
    ? routes.create(
        selectedAccountingPeriod !== null
          ? { accountingPeriodId: selectedAccountingPeriod.id }
          : currentOpenAccountingPeriod !== null
            ? { accountingPeriodId: currentOpenAccountingPeriod.id }
            : {},
      )
    : accountingPeriodRoutes.create;
  const createActionLabel = hasOpenAccountingPeriods
    ? "Create transaction"
    : "Open accounting period";
  const scopeLabel =
    selectedAccountingPeriod?.name ??
    (currentOpenAccountingPeriod !== null
      ? currentOpenAccountingPeriod.name
      : "All periods");

  return (
    <Stack spacing={3} sx={{ maxWidth: 1440 }}>
      <Breadcrumbs breadcrumbs={breadcrumbs.index()} />
      <Paper
        sx={{
          backgroundColor: "background.paper",
          backgroundImage:
            "linear-gradient(135deg, rgba(255, 152, 0, 0.18) 0%, rgba(33, 150, 243, 0.12) 46%, rgba(255, 255, 255, 0) 74%)",
          border: "1px solid",
          borderColor: "divider",
          overflow: "hidden",
          p: { xs: 3, md: 4 },
        }}
      >
        <Box
          sx={{
            display: "grid",
            gap: 3,
            gridTemplateColumns: {
              xs: "1fr",
              xl: "minmax(0, 1.2fr) minmax(360px, 0.8fr)",
            },
          }}
        >
          <Stack spacing={3}>
            <Stack spacing={1}>
              <Typography variant="overline" color="text.secondary">
                Transactions workspace
              </Typography>
              <Typography variant="h3">Transactions dashboard</Typography>
              <Typography color="text.secondary" maxWidth={760}>
                {hasOpenAccountingPeriods
                  ? "Monitor activity across accounting periods, focus the current ledger when needed, and move into transaction detail without leaving the workspace."
                  : "Review recorded transaction history and open the next accounting period before entering new activity."}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {selectedAccountingPeriod !== null
                  ? `Showing ${visibleCount} of ${transactions.totalCount} transactions for ${selectedAccountingPeriod.name}.`
                  : hasActiveSearch
                    ? `Showing ${visibleCount} of ${transactions.totalCount} transactions matching "${currentSearch}".`
                    : `Showing ${visibleCount} transactions on this page across ${transactions.totalCount} total transactions.`}
              </Typography>
            </Stack>
            <Stack direction="row" spacing={1.5} flexWrap="wrap" useFlexGap>
              <Button variant="contained" href={createActionHref}>
                {createActionLabel}
              </Button>
              {selectedAccountingPeriod !== null ? (
                <Button variant="outlined" href={routes.index({})}>
                  All periods
                </Button>
              ) : currentOpenAccountingPeriod !== null ? (
                <Button
                  variant="outlined"
                  href={routes.index({
                    accountingPeriodId: currentOpenAccountingPeriod.id,
                  })}
                >
                  Current period
                </Button>
              ) : null}
              <Button
                variant="outlined"
                href={routes.index({
                  accountingPeriodId: selectedAccountingPeriod?.id ?? "",
                  sort: TransactionSortOrder.DateDescending,
                })}
              >
                Recent first
              </Button>
              <Button
                variant="outlined"
                href={routes.index({
                  accountingPeriodId: selectedAccountingPeriod?.id ?? "",
                  sort: TransactionSortOrder.AmountDescending,
                })}
              >
                Largest amounts first
              </Button>
            </Stack>
          </Stack>
          <TransactionsDashboardControls
            accountingPeriods={accountingPeriodOptions}
            accountingPeriodParamName={nameof<TransactionsViewSearchParams>(
              "accountingPeriodId",
            )}
            searchParamName={nameof<TransactionsViewSearchParams>("search")}
            sortParamName={nameof<TransactionsViewSearchParams>("sort")}
            pageParamName={nameof<TransactionsViewSearchParams>("page")}
          />
        </Box>
      </Paper>
      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns: {
            xs: "1fr",
            md: "repeat(2, minmax(0, 1fr))",
            xl: "repeat(4, minmax(0, 1fr))",
          },
        }}
      >
        <SummaryCard
          title="Transactions In Scope"
          value={transactions.totalCount}
          description={
            selectedAccountingPeriod !== null
              ? `Current filter is locked to ${selectedAccountingPeriod.name}.`
              : "Includes every transaction that matches the current dashboard filters."
          }
        />
        <SummaryCard
          title="Visible On Page"
          value={visibleCount}
          description={`${visibleCount} transaction${visibleCount === 1 ? " is" : "s are"} currently loaded in the ledger.`}
        />
        <SummaryCard
          title="Open Periods"
          value={openAccountingPeriods.length}
          description={
            hasOpenAccountingPeriods
              ? `${openAccountingPeriods.length} open accounting period${openAccountingPeriods.length === 1 ? " is" : "s are"} available for new transaction entry.`
              : "Create a new accounting period before recording additional activity."
          }
        />
        <SummaryCard
          title="Ready To Post"
          value={postableTransactionCount}
          description="Transactions on the current page with at least one unposted account entry."
        />
      </Box>
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns: {
            xs: "1fr",
            xl: "minmax(0, 1.35fr) minmax(320px, 0.65fr)",
          },
        }}
      >
        <Stack spacing={2}>
          <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
            <Stack spacing={0.75}>
              <Typography variant="h5">Transaction ledger</Typography>
              <Typography variant="body2" color="text.secondary">
                Scan activity across periods, sort directly from the ledger
                columns, and open transaction detail without losing the wider
                collection view.
              </Typography>
            </Stack>
          </Paper>
          <TransactionListFrame
            data={transactions.items}
            totalCount={transactions.totalCount}
            createActionHref={createActionHref}
            createActionLabel={createActionLabel}
            selectedAccountingPeriodName={
              selectedAccountingPeriod?.name ?? null
            }
          />
        </Stack>
        <Stack
          spacing={2}
          sx={{
            alignSelf: "start",
            position: { xl: "sticky" },
            top: { xl: 24 },
          }}
        >
          <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
            <Stack spacing={2}>
              <Typography variant="h6">Current view</Typography>
              <Stack spacing={1.25}>
                <Box
                  sx={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: 2,
                  }}
                >
                  <Typography variant="body2" color="text.secondary">
                    Period
                  </Typography>
                  <Typography
                    variant="body2"
                    fontWeight={600}
                    textAlign="right"
                  >
                    {scopeLabel}
                  </Typography>
                </Box>
                <Box
                  sx={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: 2,
                  }}
                >
                  <Typography variant="body2" color="text.secondary">
                    Search
                  </Typography>
                  <Typography
                    variant="body2"
                    fontWeight={600}
                    textAlign="right"
                  >
                    {hasActiveSearch ? currentSearch : "All transactions"}
                  </Typography>
                </Box>
                <Box
                  sx={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: 2,
                  }}
                >
                  <Typography variant="body2" color="text.secondary">
                    Sort
                  </Typography>
                  <Typography
                    variant="body2"
                    fontWeight={600}
                    textAlign="right"
                  >
                    {formatTransactionSort(sort)}
                  </Typography>
                </Box>
                <Box
                  sx={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: 2,
                  }}
                >
                  <Typography variant="body2" color="text.secondary">
                    Visible rows
                  </Typography>
                  <Typography
                    variant="body2"
                    fontWeight={600}
                    textAlign="right"
                  >
                    {visibleCount} of {transactions.totalCount}
                  </Typography>
                </Box>
              </Stack>
            </Stack>
          </Paper>
          <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
            <Stack spacing={1.5}>
              <Typography variant="h6">Posting focus</Typography>
              <Typography variant="body2" color="text.secondary">
                {postableTransactionCount === 0
                  ? "No transactions on this page still need account posting work."
                  : `${postableTransactionCount} transaction${postableTransactionCount === 1 ? " still needs" : "s still need"} account posting attention on this page.`}
              </Typography>
              {hasOpenAccountingPeriods ? (
                <Button variant="outlined" href={createActionHref}>
                  Create another transaction
                </Button>
              ) : (
                <Button variant="outlined" href={accountingPeriodRoutes.create}>
                  Open next accounting period
                </Button>
              )}
            </Stack>
          </Paper>
        </Stack>
      </Box>
    </Stack>
  );
};

export type { TransactionsViewSearchParams };
export default TransactionsView;
