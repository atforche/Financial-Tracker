"use client";

import { type JSX, startTransition, useActionState, useEffect } from "react";
import { Button } from "@mui/material";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";
import type { Transaction } from "@/transactions/types";
import unpostTransaction from "@/transactions/workspace/unpostTransaction";
import { useRouter } from "next/navigation";

/**
 * Props for the UnpostTransactionForm component.
 */
interface UnpostTransactionFormProps {
  readonly transaction: Transaction;
  readonly redirectUrl: string;
  readonly account?: { accountId: string; accountName: string } | null;
  readonly label?: string;
  readonly size?: "small" | "medium";
}

/**
 * Component that displays the action for unposting a transaction from the provided account,
 * or from all posted accounts when none is provided.
 */
const UnpostTransactionForm = function ({
  transaction,
  redirectUrl,
  account = null,
  label = "Unpost",
  size = "medium",
}: UnpostTransactionFormProps): JSX.Element {
  const router = useRouter();
  const [state, action, pending] = useActionState(unpostTransaction, {});

  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

  return (
    <ConfirmActionDialog
      trigger={(openDialog) => (
        <Button variant="outlined" size={size} onClick={openDialog}>
          {label}
        </Button>
      )}
      title="Unpost Transaction"
      confirmationCopy={
        account === null
          ? "Are you sure you want to unpost this transaction from all posted accounts?"
          : `Are you sure you want to unpost this transaction from ${account.accountName}?`
      }
      confirmLabel="Unpost"
      pending={pending}
      errorTitle={state.errorTitle}
      unmappedErrors={state.unmappedErrors}
      onConfirm={() => {
        startTransition(() => {
          action({
            transactionId: transaction.id,
            accountId: account?.accountId ?? null,
            redirectUrl,
          });
        });
      }}
    />
  );
};

export default UnpostTransactionForm;
