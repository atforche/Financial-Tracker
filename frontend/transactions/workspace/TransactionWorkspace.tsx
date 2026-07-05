import { Box, Button, Paper, Stack, Typography } from "@mui/material";
import type { JSX } from "react";
import Link from "next/link";
import type { TransactionSortOrder } from "@/transactions/transaction";
import TransactionWorkspaceFilter from "@/transactions/workspace/TransactionWorkspaceFilter";
import TransactionWorkspaceListFrame from "@/transactions/workspace/TransactionWorkspaceListFrame";
import ViewTransactionForm from "@/transactions/workspace/ViewTransactionForm";
import { getTransactionWorkspaceListData } from "@/transactions/workspace/getTransactionWorkspaceData";
import { redirect } from "next/navigation";
import routes from "@/transactions/routes";

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
}

/**
 * Props for the TransactionWorkspace component.
 */
interface TransactionWorkspaceProps {
  readonly searchParams: Promise<TransactionWorkspaceSearchParams>;
}

/**
 * Renders the main transaction workspace, including filters, transaction list, and transaction details.
 */
const TransactionWorkspace = async function ({
  searchParams,
}: TransactionWorkspaceProps): Promise<JSX.Element> {
  const resolvedSearchParams = await searchParams;
  const { sort, selectedTransactionId, page } = resolvedSearchParams;
  const {
    openAccountingPeriods,
    allAccountingPeriods,
    accounts,
    funds,
    assignmentGoals,
    spendingGoals,
    currentPage,
    normalizedAccountingPeriodIds,
    normalizedAccountIds,
    normalizedFundIds,
    transactions,
    selectedTransaction,
    displayedTransactions,
  } = await getTransactionWorkspaceListData(resolvedSearchParams);

  const baseWorkspaceSearchParams: TransactionWorkspaceSearchParams = {
    ...(normalizedAccountingPeriodIds !== null
      ? { accountingPeriodIds: normalizedAccountingPeriodIds }
      : {}),
    ...(normalizedAccountIds !== null
      ? { accountIds: normalizedAccountIds }
      : {}),
    ...(normalizedFundIds !== null ? { fundIds: normalizedFundIds } : {}),
    ...(typeof sort !== "undefined" ? { sort } : {}),
    ...(typeof page !== "undefined" ? { page: currentPage } : {}),
  };

  if (
    typeof selectedTransactionId === "string" &&
    selectedTransaction === null
  ) {
    redirect(routes.workspace(baseWorkspaceSearchParams));
  }

  const selectedTransactionAccountingPeriod =
    selectedTransaction === null
      ? null
      : (allAccountingPeriods.find(
          (period) => period.id === selectedTransaction.accountingPeriodId,
        ) ?? null);
  const selectedWorkspaceSearchParams: TransactionWorkspaceSearchParams =
    selectedTransaction === null
      ? baseWorkspaceSearchParams
      : {
          ...baseWorkspaceSearchParams,
          selectedTransactionId: selectedTransaction.id,
        };
  const workspaceUrl = routes.workspace(selectedWorkspaceSearchParams);
  const createUrl = routes.workspaceCreate(baseWorkspaceSearchParams);
  const editUrl =
    selectedTransaction === null
      ? null
      : routes.workspaceEdit(
          selectedTransaction.id,
          selectedWorkspaceSearchParams,
        );

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <TransactionWorkspaceFilter
          accountingPeriods={openAccountingPeriods}
          accounts={accounts}
          funds={funds}
        />
      </Stack>
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns: {
            xs: "1fr",
            xl: "minmax(0, 1fr) minmax(0, 1fr)",
          },
        }}
      >
        <TransactionWorkspaceListFrame
          data={displayedTransactions}
          totalCount={transactions.totalCount}
          selectedTransactionId={selectedTransaction?.id ?? null}
        />
        <Box sx={{ display: { xs: "none", xl: "block" } }}>
          {selectedTransaction !== null &&
          selectedTransactionAccountingPeriod !== null &&
          editUrl !== null ? (
            <ViewTransactionForm
              transaction={selectedTransaction}
              transactionAccountingPeriod={selectedTransactionAccountingPeriod}
              funds={funds}
              assignmentGoals={assignmentGoals}
              spendingGoals={spendingGoals}
              currentUrl={workspaceUrl}
              workspaceUrl={workspaceUrl}
              editUrl={editUrl}
            />
          ) : (
            <Paper
              variant="outlined"
              sx={{
                borderRadius: 4,
                p: { xs: 2.5, md: 3 },
              }}
            >
              <Stack spacing={2.5}>
                <Stack spacing={0.5}>
                  <Typography variant="h6">Transaction Details</Typography>
                  <Typography variant="body2" color="text.secondary">
                    Select a transaction to review its details, post any tracked
                    account activity, or make changes.
                  </Typography>
                </Stack>
                <Link
                  href={createUrl}
                  style={{ alignSelf: "flex-start", textDecoration: "none" }}
                >
                  <Button component="span" variant="contained">
                    Create Transaction
                  </Button>
                </Link>
              </Stack>
            </Paper>
          )}
        </Box>
      </Box>
    </Stack>
  );
};

export type { TransactionWorkspaceSearchParams };
export default TransactionWorkspace;
