"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import { ToggleState } from "@/accounting-periods/AccountingPeriodViewListFrames";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import breadcrumbs from "@/accounts/breadcrumbs";
import routes from "@/accounts/routes";
import updateAccount from "@/accounts/updateAccount";

/**
 * Props for the UpdateAccountForm component.
 */
interface UpdateAccountFormProps {
  readonly account: Account;
  readonly routeAccountingPeriod?: AccountingPeriod | null;
}

/**
 * Gets the URL to redirect the user to after successfully updating an account.
 */
const getRedirectUrl = function (
  routeAccountingPeriod: AccountingPeriod | null,
): string {
  if (routeAccountingPeriod !== null) {
    return accountingPeriodRoutes.detail(
      { id: routeAccountingPeriod.id },
      { display: ToggleState.Accounts },
    );
  }
  return routes.index({});
};

/**
 * Component that displays the form for updating an account.
 */
const UpdateAccountForm = function ({
  account,
  routeAccountingPeriod = null,
}: UpdateAccountFormProps): JSX.Element {
  const [name, setName] = useState<string>(account.name);

  const [state, action, pending] = useActionState(updateAccount, {
    accountId: account.id,
    redirectUrl: getRedirectUrl(routeAccountingPeriod),
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={breadcrumbs.update(account, routeAccountingPeriod)}
      />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <StringEntryField
          label="Name"
          value={name}
          setValue={setName}
          errorMessage={state.nameErrors ?? null}
        />
        <DialogActions>
          <Link href={getRedirectUrl(routeAccountingPeriod)} tabIndex={-1}>
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
