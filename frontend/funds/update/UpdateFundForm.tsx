"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/funds/types";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import breadcrumbs from "@/funds/breadcrumbs";
import routes from "@/funds/routes";
import updateFund from "@/funds/update/updateFund";

/**
 * Props for the UpdateFundForm component.
 */
interface UpdateFundFormProps {
  readonly fund: Fund;
  readonly routeAccountingPeriod?: AccountingPeriod | null;
}

/**
 * Gets the URL to redirect the user to after successfully updating a fund.
 */
const getRedirectUrl = function (
  fund: Fund,
  routeAccountingPeriod: AccountingPeriod | null,
): string {
  if (routeAccountingPeriod !== null) {
    return accountingPeriodRoutes.fundDetail(
      {
        id: routeAccountingPeriod.id,
        fundId: fund.id,
      },
      {},
    );
  }
  return routes.detail({ id: fund.id }, {});
};

/**
 * Component that displays the form for updating a fund.
 */
const UpdateFundForm = function ({
  fund,
  routeAccountingPeriod = null,
}: UpdateFundFormProps): JSX.Element {
  const [name, setName] = useState<string>(fund.name);
  const [description, setDescription] = useState<string>(fund.description);

  const [state, action, pending] = useActionState(updateFund, {
    fundId: fund.id,
    redirectUrl: getRedirectUrl(fund, routeAccountingPeriod),
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={breadcrumbs.update(fund, routeAccountingPeriod)}
      />
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
            href={getRedirectUrl(fund, routeAccountingPeriod)}
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
