"use client";

import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
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
import ViewTransactionForm from "@/transactions/workspace/ViewTransactionForm";

/**
 * Props for the TransactionWorkspaceActions component.
 */
interface TransactionWorkspaceActionsProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
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
  const actions: TransactionWorkspaceAction[] = ["view", "update", "delete"];
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
  assignmentGoals,
  spendingGoals,
  selectedTransaction,
  requestedAction,
}: TransactionWorkspaceActionsProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const allActions: readonly TransactionWorkspaceAction[] = [
    "view",
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

  const redirectUrl = `${pathname}?${searchParams.toString()}`;

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
  const selectedTransactionDebitAccount =
    selectedTransaction === null
      ? null
      : (accounts.find(
          (account) =>
            "debitAccount" in selectedTransaction &&
            account.id === selectedTransaction.debitAccount?.accountId,
        ) ?? null);
  const selectedTransactionCreditAccount =
    selectedTransaction === null
      ? null
      : (accounts.find(
          (account) =>
            "creditAccount" in selectedTransaction &&
            account.id === selectedTransaction.creditAccount?.accountId,
        ) ?? null);
  const selectedTransactionDebitFund =
    selectedTransaction === null
      ? null
      : (funds.find(
          (fund) =>
            "debitFund" in selectedTransaction &&
            fund.id === selectedTransaction.debitFund.fundId,
        ) ?? null);
  const selectedTransactionCreditFund =
    selectedTransaction === null
      ? null
      : (funds.find(
          (fund) =>
            "creditFund" in selectedTransaction &&
            fund.id === selectedTransaction.creditFund.fundId,
        ) ?? null);

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
              <Typography variant="h5">Existing Transaction</Typography>
              <Typography variant="body2" color="text.secondary">
                Choose how to interact with this existing transaction.
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
                  {action === "view"
                    ? "View"
                    : action === "post"
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
            assignmentGoals={assignmentGoals}
            spendingGoals={spendingGoals}
            redirectUrl={redirectUrl}
          />
        ) : null}
        {activeAction === "view" &&
        selectedTransaction !== null &&
        selectedTransactionAccountingPeriod !== null ? (
          <ViewTransactionForm
            transaction={selectedTransaction}
            transactionAccountingPeriod={selectedTransactionAccountingPeriod}
            funds={funds}
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
            transactionDebitAccount={selectedTransactionDebitAccount}
            transactionCreditAccount={selectedTransactionCreditAccount}
            transactionDebitFund={selectedTransactionDebitFund}
            transactionCreditFund={selectedTransactionCreditFund}
            funds={funds}
            assignmentGoals={assignmentGoals}
            spendingGoals={spendingGoals}
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
