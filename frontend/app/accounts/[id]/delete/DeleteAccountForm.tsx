"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { Account } from "@/data/accountTypes";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import deleteAccount from "@/app/accounts/[id]/delete/deleteAccount";
import routes from "@/framework/routes";

/**
 * Props for the DeleteAccountForm component.
 */
interface DeleteAccountFormProps {
  readonly account: Account;
  readonly accountingPeriod: AccountingPeriod | null;
}

/**
 * Gets the breadcrumbs to display at the top of the form.
 */
const getBreadcrumbs = function (
  account: Account,
  accountingPeriod: AccountingPeriod | null,
): JSX.Element {
  if (accountingPeriod !== null) {
    return (
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Accounting Periods",
            href: routes.accountingPeriods.index,
          },
          {
            label: accountingPeriod.name,
            href: routes.accountingPeriods.detail(accountingPeriod.id),
          },
          {
            label: `Delete ${account.name}`,
            href: routes.accounts.delete(account.id),
          },
        ]}
      />
    );
  }
  return (
    <Breadcrumbs
      breadcrumbs={[
        {
          label: "Accounts",
          href: routes.accounts.index,
        },
        {
          label: `Delete ${account.name}`,
          href: routes.accounts.delete(account.id),
        },
      ]}
    />
  );
};

/**
 * Gets the URL to redirect to after the account is deleted.
 */
const getRedirectUrl = function (
  accountingPeriod: AccountingPeriod | null,
): string {
  if (accountingPeriod !== null) {
    return routes.accountingPeriods.detail(accountingPeriod.id);
  }
  return routes.accounts.index;
};

/**
 * Component that displays the form for deleting an account.
 */
const DeleteAccountForm = function ({
  account,
  accountingPeriod,
}: DeleteAccountFormProps): JSX.Element {
  const [state, action, pending] = useActionState(deleteAccount, {
    accountId: account.id,
    redirectUrl: getRedirectUrl(accountingPeriod),
  });

  return (
    <Stack spacing={2}>
      {getBreadcrumbs(account, accountingPeriod)}
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <Typography>
          Are you sure you want to delete the account &quot;
          {account.name}&quot;?
        </Typography>
        <DialogActions>
          <Link href={getRedirectUrl(accountingPeriod)} tabIndex={-1}>
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

export default DeleteAccountForm;
