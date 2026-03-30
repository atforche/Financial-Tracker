"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/data/fundTypes";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import updateFund from "@/app/funds/[id]/update/updateFund";

/**
 * Props for the UpdateFundForm component.
 */
interface UpdateFundFormProps {
  readonly fund: Fund;
  readonly providedAccountingPeriod?: AccountingPeriod | null;
}

/**
 * Gets the breadcrumbs to be displayed at the top of the form.
 */
const getBreadcrumbs = function (
  fund: Fund,
  providedAccountingPeriod: AccountingPeriod | null,
): JSX.Element {
  if (providedAccountingPeriod !== null) {
    return (
      <Breadcrumbs
        breadcrumbs={[
          { label: "Accounting Periods", href: "/accounting-periods" },
          {
            label: providedAccountingPeriod.name,
            href: `/accounting-periods/${providedAccountingPeriod.id}`,
          },
          {
            label: fund.name,
            href: `/accounting-periods/${providedAccountingPeriod.id}/funds/${fund.id}`,
          },
          {
            label: "Update",
            href: `/funds/${fund.id}/update`,
          },
        ]}
      />
    );
  }
  return (
    <Breadcrumbs
      breadcrumbs={[
        { label: "Funds", href: "/funds" },
        { label: fund.name, href: `/funds/${fund.id}` },
        {
          label: "Update",
          href: `/funds/${fund.id}/update`,
        },
      ]}
    />
  );
};

/**
 * Gets the URL to redirect the user to after successfully updating a fund.
 */
const getRedirectUrl = function (
  fund: Fund,
  providedAccountingPeriod: AccountingPeriod | null,
): string {
  if (providedAccountingPeriod !== null) {
    return `/accounting-periods/${providedAccountingPeriod.id}/funds/${fund.id}`;
  }
  return `/funds/${fund.id}`;
};

/**
 * Component that displays the form for updating a fund.
 */
const UpdateFundForm = function ({
  fund,
  providedAccountingPeriod = null,
}: UpdateFundFormProps): JSX.Element {
  const [name, setName] = useState<string>(fund.name);
  const [description, setDescription] = useState<string>(fund.description);

  const [state, action, pending] = useActionState(updateFund, {
    fundId: fund.id,
    redirectUrl: getRedirectUrl(fund, providedAccountingPeriod),
  });

  return (
    <Stack spacing={2}>
      {getBreadcrumbs(fund, providedAccountingPeriod)}
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
        <DialogActions>
          <Link
            href={getRedirectUrl(fund, providedAccountingPeriod)}
            tabIndex={-1}
          >
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
                action({ name, description });
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

export default UpdateFundForm;
