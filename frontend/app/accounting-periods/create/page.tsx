"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import { type JSX, useActionState } from "react";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { CreateAccountingPeriodRequest } from "@/data/accountingPeriodTypes";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import IntegerEntryField from "@/framework/forms/IntegerEntryField";
import Link from "next/link";
import createAccountingPeriod from "@/app/accounting-periods/create/createAccountingPeriod";
import nameof from "@/data/nameof";

/**
 * Component that displays the create Accounting Period page.
 */
const CreateAccountingPeriod = function (): JSX.Element {
  const [state, action, pending] = useActionState(createAccountingPeriod, {});

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={[
          { label: "Accounting Periods", href: "/accounting-periods" },
          { label: "Create", href: "/accounting-periods/create" },
        ]}
      />
      <form action={action}>
        <Stack spacing={2} sx={{ maxWidth: "500px" }}>
          <IntegerEntryField
            name={nameof<CreateAccountingPeriodRequest>("year")}
            label="Year"
            defaultValue={state.year?.value ?? null}
            errorMessage={state.year?.errorMessage ?? null}
          />
          <IntegerEntryField
            name={nameof<CreateAccountingPeriodRequest>("month")}
            label="Month"
            defaultValue={state.month?.value ?? null}
            errorMessage={state.month?.errorMessage ?? null}
          />
          <DialogActions>
            <Link href="/accounting-periods">
              <Button variant="outlined">Cancel</Button>
            </Link>
            <Button type="submit" variant="contained" loading={pending}>
              Create
            </Button>
          </DialogActions>
          <ErrorAlert
            errorMessage={state.errorTitle ?? null}
            unmappedErrors={state.unmappedErrors ?? null}
          />
        </Stack>
      </form>
    </Stack>
  );
};

export default CreateAccountingPeriod;
