"use client";

import {
  Paper,
  Stack,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from "@mui/material";
import {
  type Transaction,
  getPostableTransactionAccounts,
  getPostedTransactionAccounts,
} from "@/transactions/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateTransactionForm from "@/transactions/workspace/CreateTransactionForm";
import DeleteTransactionForm from "@/transactions/workspace/DeleteTransactionForm";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";
import PostTransactionForm from "@/transactions/workspace/PostTransactionForm";
import type { TransactionWorkspaceAction } from "@/transactions/workspace/TransactionWorkspace";
import UnpostTransactionForm from "@/transactions/workspace/UnpostTransactionForm";
import UpdateTransactionForm from "@/transactions/workspace/UpdateTransactionForm";

/**
 * Props for the TransactionWorkspaceActions component.
 */
interface TransactionWorkspaceActionsProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly selectedTransaction: Transaction | null;
  readonly requestedAction: TransactionWorkspaceAction | null;
}

/**
 * Gets the available actions for the provided transaction.
 */
const getAvailableActions = function (
  selectedTransaction: Transaction | null,
): readonly TransactionWorkspaceAction[] {
  if (selectedTransaction === null) {
    return ["create"];
  }
  const isPostable =
    getPostableTransactionAccounts(selectedTransaction).length > 0;
  const isUnpostable =
    getPostedTransactionAccounts(selectedTransaction).length > 0;
  const actions: TransactionWorkspaceAction[] = ["update", "delete"];
  if (isPostable) {
    actions.push("post");
  }
  if (isUnpostable) {
    actions.push("unpost");
  }
  return actions;
};

/**
 * Displays the available transaction actions for the current workspace selection.
 */
const TransactionWorkspaceActions = function ({
  accountingPeriods,
  accounts,
  funds,
  selectedTransaction,
  requestedAction,
}: TransactionWorkspaceActionsProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const allActions: readonly TransactionWorkspaceAction[] = [
    "update",
    "post",
    "unpost",
    "delete",
  ];
  const availableActions: readonly TransactionWorkspaceAction[] =
    getAvailableActions(selectedTransaction);

  const activeAction =
    requestedAction !== null && availableActions.includes(requestedAction)
      ? requestedAction
      : availableActions[0];

  const setAction = function (action: TransactionWorkspaceAction | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (action === null) {
      params.delete("action");
    } else {
      params.set("action", action);
    }
    router.replace(`${pathname}?${params.toString()}`, { scroll: false });
  };

  const selectedTransactionAccountingPeriod = selectedTransaction
    ? (accountingPeriods.find(
        (period) => period.id === selectedTransaction.accountingPeriodId,
      ) ?? null)
    : null;

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        p: { xs: 2.5, md: 3 },
      }}
    >
      <Stack spacing={3}>
        {selectedTransaction !== null ? (
          <>
            <Stack spacing={0.5}>
              <Typography variant="h5">Modify Transaction</Typography>
              <Typography variant="body2" color="text.secondary">
                Choose how to modify this transaction.
              </Typography>
            </Stack>
            <ToggleButtonGroup
              value={activeAction}
              exclusive
              onChange={(_, nextValue: TransactionWorkspaceAction | null) => {
                setAction(nextValue);
              }}
              sx={{ flexWrap: "wrap" }}
            >
              {allActions.map((action) => (
                <ToggleButton
                  key={action}
                  value={action}
                  disabled={!availableActions.includes(action)}
                >
                  {action === "post"
                    ? "Post"
                    : action === "unpost"
                      ? "Unpost"
                      : action === "update"
                        ? "Update"
                        : "Delete"}
                </ToggleButton>
              ))}
            </ToggleButtonGroup>
          </>
        ) : null}
        {activeAction === "create" ? (
          <CreateTransactionForm
            accountingPeriods={accountingPeriods}
            accounts={accounts}
            funds={funds}
            redirectUrl={pathname}
          />
        ) : null}
        {activeAction === "post" && selectedTransaction !== null ? (
          <PostTransactionForm
            transaction={selectedTransaction}
            redirectUrl={pathname}
          />
        ) : null}
        {activeAction === "unpost" && selectedTransaction !== null ? (
          <UnpostTransactionForm
            transaction={selectedTransaction}
            redirectUrl={pathname}
          />
        ) : null}
        {activeAction === "update" &&
        selectedTransaction !== null &&
        selectedTransactionAccountingPeriod !== null ? (
          <UpdateTransactionForm
            transaction={selectedTransaction}
            transactionAccountingPeriod={selectedTransactionAccountingPeriod}
            transactionDebitAccount={
              accounts.find(
                (account) =>
                  "debitAccountId" in selectedTransaction &&
                  account.id === selectedTransaction.debitAccountId,
              ) ?? null
            }
            transactionCreditAccount={
              accounts.find(
                (account) =>
                  "creditAccountId" in selectedTransaction &&
                  account.id === selectedTransaction.creditAccountId,
              ) ?? null
            }
            transactionDebitFund={
              funds.find(
                (fund) =>
                  "debitFundId" in selectedTransaction &&
                  fund.id === selectedTransaction.debitFundId,
              ) ?? null
            }
            transactionCreditFund={
              funds.find(
                (fund) =>
                  "creditFundId" in selectedTransaction &&
                  fund.id === selectedTransaction.creditFundId,
              ) ?? null
            }
            funds={funds}
            redirectUrl={pathname}
          />
        ) : null}
        {activeAction === "delete" && selectedTransaction !== null ? (
          <DeleteTransactionForm
            transaction={selectedTransaction}
            redirectUrl={pathname}
          />
        ) : null}
      </Stack>
    </Paper>
  );
};

export default TransactionWorkspaceActions;
