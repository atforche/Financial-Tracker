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
}

/**
 * Component that displays the action for unposting a transaction.
 */
const UnpostTransactionForm = function ({
  transaction,
  redirectUrl,
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
        <Button variant="outlined" onClick={openDialog}>
          Unpost
        </Button>
      )}
      title="Unpost Transaction"
      confirmationCopy="Are you sure you want to unpost this transaction from all posted accounts?"
      confirmLabel="Unpost"
      pending={pending}
      errorTitle={state.errorTitle}
      unmappedErrors={state.unmappedErrors}
      onConfirm={() => {
        startTransition(() => {
          action({ transactionId: transaction.id, redirectUrl });
        });
      }}
    />
  );
};

export default UnpostTransactionForm;
