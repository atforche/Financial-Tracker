"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { Account } from "@/data/accountTypes";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/data/fundTypes";
import Link from "next/link";
import type { Transaction } from "@/data/transactionTypes";
import getBreadcrumbs from "@/app/transactions/[id]/unpost/getBreadcrumbs";
import unpostTransaction from "@/app/transactions/[id]/unpost/unpostTransaction";

/**
 * Props for the UnpostTransactionForm component.
 */
interface UnpostTransactionFormProps {
  readonly transaction: Transaction;
  readonly redirectUrl: string;
  readonly providedAccountingPeriod?: AccountingPeriod | null;
  readonly providedAccount?: Account | null;
  readonly providedFund?: Fund | null;
}

/**
 * Component that displays the form for unposting a transaction from an account.
 */
const UnpostTransactionForm = function ({
  transaction,
  redirectUrl,
  providedAccountingPeriod = null,
  providedAccount = null,
  providedFund = null,
}: UnpostTransactionFormProps): JSX.Element {
  const [state, action, pending] = useActionState(unpostTransaction, {
    transactionId: transaction.id,
    redirectUrl,
  });

  return (
    <Stack spacing={2} maxWidth={800}>
      <Breadcrumbs
        breadcrumbs={getBreadcrumbs(
          transaction,
          providedAccountingPeriod,
          providedAccount,
          providedFund,
        )}
      />
      <Typography>Are you sure you want to unpost this transaction?</Typography>
      <DialogActions sx={{ maxWidth: "800px" }}>
        <Link href={redirectUrl} tabIndex={-1}>
          <Button variant="outlined">Cancel</Button>
        </Link>
        <Button
          variant="contained"
          color="primary"
          loading={pending}
          onClick={() => {
            startTransition(() => {
              action();
            });
          }}
        >
          Unpost
        </Button>
      </DialogActions>
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />
    </Stack>
  );
};

export default UnpostTransactionForm;
