import { Box, Stack } from "@mui/material";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import type { JSX } from "react";
import type { TransactionSortOrder } from "@/transactions/types";
import TransactionWorkspaceActions from "@/transactions/workspace/TransactionWorkspaceActions";
import TransactionWorkspaceFilter from "@/transactions/workspace/TransactionWorkspaceFilter";
import TransactionWorkspaceListFrame from "@/transactions/workspace/TransactionWorkspaceListFrame";
import getApiClient from "@/framework/data/getApiClient";
import { redirect } from "next/navigation";
import routes from "@/transactions/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

type TransactionWorkspaceAction =
  | "view"
  | "create"
  | "update"
  | "post"
  | "unpost"
  | "delete";

/**
 * Search parameters supported by the Transactions workspace.
 */
interface TransactionWorkspaceSearchParams {
  accountingPeriodIds?: string | string[];
  accountIds?: string | string[];
  fundIds?: string | string[];
  sort?: TransactionSortOrder;
  page?: number | string | null;
  selectedTransactionId?: string;
  action?: TransactionWorkspaceAction;
}

/**
 * Props for the TransactionWorkspace component.
 */
interface TransactionWorkspaceProps {
  readonly searchParams: Promise<TransactionWorkspaceSearchParams>;
}

const toRepeatedSearchParam = function (
  value: string | string[] | undefined,
): string[] | null {
  if (Array.isArray(value)) {
    return value;
  }

  return typeof value === "string" ? [value] : null;
};

/**
 * Displays the transaction workspace with list-backed inline actions.
 */
const TransactionWorkspace = async function ({
  searchParams,
}: TransactionWorkspaceProps): Promise<JSX.Element> {
  const {
    accountingPeriodIds,
    accountIds,
    fundIds,
    sort,
    page,
    selectedTransactionId,
    action,
  } = await searchParams;
  const apiClient = getApiClient();
  const currentPage = normalizePageValue(page);
  const normalizedAccountingPeriodIds =
    toRepeatedSearchParam(accountingPeriodIds);
  const normalizedAccountIds = toRepeatedSearchParam(accountIds);
  const normalizedFundIds = toRepeatedSearchParam(fundIds);

  const openAccountingPeriodsPromise = apiClient.GET(
    "/accounting-periods/open",
  );
  const accountsPromise = apiClient.GET("/accounts");
  const fundsPromise = apiClient.GET("/funds");
  const transactionsPromise = apiClient.GET("/transactions", {
    params: {
      query: {
        ...(normalizedAccountingPeriodIds !== null
          ? { AccountingPeriodIds: normalizedAccountingPeriodIds }
          : {}),
        ...(normalizedAccountIds !== null
          ? { AccountIds: normalizedAccountIds }
          : {}),
        ...(normalizedFundIds !== null ? { FundIds: normalizedFundIds } : {}),
        Sort: sort ?? null,
        Limit: rowsPerPage,
        Offset: getPageOffset(currentPage),
      },
    },
  });

  const [
    { data: openAccountingPeriods },
    { data: accounts },
    { data: funds },
    { data: transactions },
  ] = await Promise.all([
    openAccountingPeriodsPromise,
    accountsPromise,
    fundsPromise,
    transactionsPromise,
  ]);

  if (typeof openAccountingPeriods === "undefined") {
    throw new Error("Failed to fetch open accounting periods");
  }
  if (typeof accounts === "undefined") {
    throw new Error("Failed to fetch accounts");
  }
  if (typeof funds === "undefined") {
    throw new Error("Failed to fetch funds");
  }
  if (typeof transactions === "undefined") {
    throw new Error("Failed to fetch transactions");
  }

  const selectedTransactionById =
    typeof selectedTransactionId === "string"
      ? ((
          await apiClient.GET("/transactions/{transactionId}", {
            params: {
              path: {
                transactionId: selectedTransactionId,
              },
            },
          })
        ).data ?? null)
      : null;

  const selectedTransaction =
    selectedTransactionById ??
    transactions.items.find(
      (transaction) => transaction.id === selectedTransactionId,
    ) ??
    null;
  const displayedTransactions =
    selectedTransaction !== null &&
    !transactions.items.some(
      (transaction) => transaction.id === selectedTransaction.id,
    )
      ? [selectedTransaction, ...transactions.items].slice(0, rowsPerPage)
      : transactions.items;

  if (
    typeof selectedTransactionId === "string" &&
    selectedTransaction === null
  ) {
    redirect(
      routes.workspace({
        ...(normalizedAccountingPeriodIds !== null
          ? { accountingPeriodIds: normalizedAccountingPeriodIds }
          : {}),
        ...(normalizedAccountIds !== null
          ? { accountIds: normalizedAccountIds }
          : {}),
        ...(normalizedFundIds !== null ? { fundIds: normalizedFundIds } : {}),
        ...(typeof sort !== "undefined" ? { sort } : {}),
        ...(typeof page !== "undefined" ? { page: currentPage } : {}),
        ...(typeof action !== "undefined" ? { action } : {}),
      }),
    );
  }

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <TransactionWorkspaceFilter
          accountingPeriods={openAccountingPeriods}
          accounts={accounts.items}
          funds={funds.items}
        />
      </Stack>
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns:
            "repeat(auto-fit, minmax(min(100%, 600px), 1fr))",
        }}
      >
        <TransactionWorkspaceListFrame
          data={displayedTransactions}
          totalCount={transactions.totalCount}
          selectedTransactionId={selectedTransaction?.id ?? null}
        />
        <TransactionWorkspaceActions
          accountingPeriods={openAccountingPeriods}
          accounts={accounts.items}
          funds={funds.items}
          selectedTransaction={selectedTransaction}
          requestedAction={action ?? null}
        />
      </Box>
    </Stack>
  );
};

export type { TransactionWorkspaceAction, TransactionWorkspaceSearchParams };
export default TransactionWorkspace;
