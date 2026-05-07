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
  readonly routeAccountingPeriod: AccountingPeriod | null;
  readonly routeAccount: Account | null;
  readonly routeFund: Fund | null;
}

/**
 * Gets the redirect URL based on the provided context of accounting period, account, and fund.
 */
const getRedirectUrl = function (
  routeAccountingPeriod: AccountingPeriod | null,
  routeAccount: Account | null,
  routeFund: Fund | null,
): string {
  if (routeAccountingPeriod !== null) {
    if (routeAccount !== null) {
      return accountingPeriodRoutes.accountDetail({
        id: routeAccountingPeriod.id,
        accountId: routeAccount.id,
      });
    }
    if (routeFund !== null) {
      return accountingPeriodRoutes.fundDetail(
        { id: routeAccountingPeriod.id, fundId: routeFund.id },
        {},
      );
    }
  }
  if (routeAccount !== null) {
    return accountRoutes.detail({ id: routeAccount.id }, {});
  }
  if (routeFund !== null) {
    return fundRoutes.detail({ id: routeFund.id }, {});
  }
  return "";
};

/**
 * Component that displays the form for deleting a transaction.
 */
const DeleteTransactionForm = function ({
  transaction,
  routeAccountingPeriod,
  routeAccount,
  routeFund,
}: DeleteTransactionFormProps): JSX.Element {
  const [state, action, pending] = useActionState(deleteTransaction, {
    transactionId: transaction.id,
    redirectUrl: getRedirectUrl(routeAccountingPeriod, routeAccount, routeFund),
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={breadcrumbs.delete(
          transaction,
          routeAccountingPeriod,
          routeAccount,
          routeFund,
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
