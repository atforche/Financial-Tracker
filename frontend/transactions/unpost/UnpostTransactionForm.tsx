"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import type { Transaction } from "@/transactions/types";
import breadcrumbs from "@/transactions/breadcrumbs";
import routes from "@/transactions/routes";
import unpostTransaction from "@/transactions/unpost/unpostTransaction";

/**
 * Gets the accounts from the transaction that are already posted.
 */
const getPostedTransactionAccounts = function (
  transaction: Transaction,
): { accountId: string; accountName: string }[] {
  const accounts = [];
  if (
    "debitAccount" in transaction &&
    transaction.debitAccount !== null &&
    transaction.debitAccount.postedDate !== null
  ) {
    accounts.push({
      accountId: transaction.debitAccount.accountId,
      accountName: transaction.debitAccount.accountName,
    });
  }
  if (
    "creditAccount" in transaction &&
    transaction.creditAccount !== null &&
    transaction.creditAccount.postedDate !== null
  ) {
    accounts.push({
      accountId: transaction.creditAccount.accountId,
      accountName: transaction.creditAccount.accountName,
    });
  }
  return accounts;
};

/**
 * Props for the UnpostTransactionForm component.
 */
interface UnpostTransactionFormProps {
  readonly transaction: Transaction;
  readonly routeAccountingPeriod: AccountingPeriod | null;
  readonly routeAccount: Account | null;
}

/**
 * Component that displays the form for unposting a transaction.
 */
const UnpostTransactionForm = function ({
  transaction,
  routeAccountingPeriod,
  routeAccount,
}: UnpostTransactionFormProps): JSX.Element {
  const redirectUrl = routes.detail(
    { id: transaction.id },
    {
      accountingPeriodId: routeAccountingPeriod?.id ?? null,
      accountId: routeAccount?.id ?? null,
    },
  );
  const [state, action, pending] = useActionState(unpostTransaction, {
    transactionId: transaction.id,
    redirectUrl,
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={breadcrumbs.unpost(
          transaction,
          routeAccountingPeriod,
          routeAccount,
        )}
      />
      <Stack spacing={2} sx={{ maxWidth: "600px" }}>
        <Typography>
          Are you sure you want to unpost this transaction from all posted
          accounts?
        </Typography>
        <DialogActions>
          <Link href={redirectUrl} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
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
    </Stack>
  );
};

export { getPostedTransactionAccounts };
export default UnpostTransactionForm;
