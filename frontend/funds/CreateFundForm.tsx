"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import breadcrumbs from "@/funds/breadcrumbs";
import createFund from "@/funds/createFund";
import routes from "@/funds/routes";

/**
 * Props for the CreateFundForm component.
 */
interface CreateFundFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly providedAccountingPeriod?: AccountingPeriod | null;
}

/**
 * Gets the URL to redirect the user to after successfully creating a fund.
 */
const getRedirectUrl = function (
  providedAccountingPeriod: AccountingPeriod | null,
): string {
  if (providedAccountingPeriod !== null) {
    return accountingPeriodRoutes.detail(
      { id: providedAccountingPeriod.id },
      {},
    );
  }
  return routes.index({});
};

/**
 * Component that displays the form for creating a fund.
 */
const CreateFundForm = function ({
  accountingPeriods,
  providedAccountingPeriod = null,
}: CreateFundFormProps): JSX.Element {
  const [name, setName] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(providedAccountingPeriod);

  const [state, action, pending] = useActionState(createFund, {
    redirectUrl: getRedirectUrl(providedAccountingPeriod),
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs.create(providedAccountingPeriod)} />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <StringEntryField
          label="Name"
          value={name}
          setValue={setName}
          errorMessage={state.nameErrors ?? null}
        />
        <StringEntryField
          label="Description"
          value={description}
          setValue={setDescription}
          errorMessage={state.descriptionErrors ?? null}
        />
        <AccountingPeriodEntryField
          label="Accounting Period"
          options={accountingPeriods}
          value={accountingPeriod}
          setValue={
            providedAccountingPeriod === null ? setAccountingPeriod : null
          }
          errorMessage={state.accountingPeriodErrors ?? null}
        />
        <DialogActions>
          <Link href={getRedirectUrl(providedAccountingPeriod)} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={name === "" || accountingPeriod === null}
            onClick={() => {
              if (name === "" || accountingPeriod === null) {
                return;
              }
              startTransition(() => {
                action({
                  name,
                  description,
                  accountingPeriodId: accountingPeriod.id,
                });
              });
            }}
          >
            Create
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

export default CreateFundForm;
