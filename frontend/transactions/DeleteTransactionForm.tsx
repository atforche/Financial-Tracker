"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/funds/types";
import Link from "next/link";
import type { Transaction } from "@/transactions/types";
import accountRoutes from "@/accounts/routes";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import breadcrumbs from "@/transactions/breadcrumbs";
import deleteTransaction from "@/transactions/deleteTransaction";
import fundRoutes from "@/funds/routes";

/**
 * Props for the DeleteTransactionForm component.
 */
interface DeleteTransactionFormProps {
  readonly transaction: Transaction;
  readonly providedAccountingPeriod: AccountingPeriod | null;
  readonly providedAccount: Account | null;
  readonly providedFund: Fund | null;
}

/**
 * Gets the redirect URL based on the provided context of accounting period, account, and fund.
 */
const getRedirectUrl = function (
  providedAccountingPeriod: AccountingPeriod | null,
  providedAccount: Account | null,
  providedFund: Fund | null,
): string {
  if (providedAccountingPeriod !== null) {
    if (providedAccount !== null) {
      return accountingPeriodRoutes.accountDetail({
        id: providedAccountingPeriod.id,
        accountId: providedAccount.id,
      });
    }
    if (providedFund !== null) {
      return accountingPeriodRoutes.fundDetail(
        { id: providedAccountingPeriod.id, fundId: providedFund.id },
        {},
      );
    }
  }
  if (providedAccount !== null) {
    return accountRoutes.detail({ id: providedAccount.id }, {});
  }
  if (providedFund !== null) {
    return fundRoutes.detail({ id: providedFund.id }, {});
  }
  return "";
};

/**
 * Component that displays the form for deleting a transaction.
 */
const DeleteTransactionForm = function ({
  transaction,
  providedAccountingPeriod,
  providedAccount,
  providedFund,
}: DeleteTransactionFormProps): JSX.Element {
  const [state, action, pending] = useActionState(deleteTransaction, {
    transactionId: transaction.id,
    redirectUrl: getRedirectUrl(
      providedAccountingPeriod,
      providedAccount,
      providedFund,
    ),
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={breadcrumbs.delete(
          transaction,
          providedAccountingPeriod,
          providedAccount,
          providedFund,
        )}
      />
      <Stack spacing={2} sx={{ maxWidth: "600px" }}>
        <Typography>
          Are you sure you want to delete this transaction?
        </Typography>
        <DialogActions>
          <Link href={state.redirectUrl} tabIndex={-1}>
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
            Delete
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

export default DeleteTransactionForm;
