"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import type { Account } from "@/data/accountTypes";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import routes from "@/framework/routes";
import updateAccount from "@/app/accounts/[id]/update/updateAccount";

/**
 * Props for the UpdateAccountForm component.
 */
interface UpdateAccountFormProps {
  readonly account: Account;
  readonly providedAccountingPeriod?: AccountingPeriod | null;
}

/**
 * Gets the breadcrumbs to be displayed at the top of the form.
 */
const getBreadcrumbs = function (
  account: Account,
  providedAccountingPeriod: AccountingPeriod | null,
): JSX.Element {
  if (providedAccountingPeriod !== null) {
    return (
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Accounting Periods",
            href: routes.accountingPeriods.index,
          },
          {
            label: providedAccountingPeriod.name,
            href: routes.accountingPeriods.detail(providedAccountingPeriod.id),
          },
          {
            label: `Update ${account.name}`,
            href: routes.accounts.update(account.id),
          },
        ]}
      />
    );
  }
  return (
    <Breadcrumbs
      breadcrumbs={[
        { label: "Accounts", href: routes.accounts.index },
        {
          label: `Update ${account.name}`,
          href: routes.accounts.update(account.id),
        },
      ]}
    />
  );
};

/**
 * Gets the URL to redirect the user to after successfully creating an account.
 */
const getRedirectUrl = function (
  providedAccountingPeriod: AccountingPeriod | null,
): string {
  if (providedAccountingPeriod !== null) {
    return routes.accountingPeriods.detail(providedAccountingPeriod.id);
  }
  return routes.accounts.index;
};

/**
 * Component that displays the form for updating an account.
 */
const UpdateAccountForm = function ({
  account,
  providedAccountingPeriod = null,
}: UpdateAccountFormProps): JSX.Element {
  const [name, setName] = useState<string>(account.name);

  const [state, action, pending] = useActionState(updateAccount, {
    accountId: account.id,
    redirectUrl: getRedirectUrl(providedAccountingPeriod),
  });

  return (
    <Stack spacing={2}>
      {getBreadcrumbs(account, providedAccountingPeriod)}
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <StringEntryField
          label="Name"
          value={name}
          setValue={setName}
          errorMessage={state.nameErrors ?? null}
        />
        <DialogActions>
          <Link href={getRedirectUrl(providedAccountingPeriod)} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={name === ""}
            onClick={() => {
              if (name === "") {
                return;
              }
              startTransition(() => {
                action({ name });
              });
            }}
          >
            Update
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

export default UpdateAccountForm;
