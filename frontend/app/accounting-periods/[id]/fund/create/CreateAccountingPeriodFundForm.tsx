"use client";

import { Button, DialogActions, Stack, TextField } from "@mui/material";
import { type JSX, useActionState } from "react";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import type { CreateFundRequest } from "@/data/fundTypes";
import DateEntryField from "@/framework/forms/DateEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import createAccountingPeriodFund from "@/app/accounting-periods/[id]/fund/create/createAccountingPeriodFund";
import nameof from "@/data/nameof";

/**
 * Props for the CreateAccountingPeriodFundForm component.
 */
interface CreateAccountingPeriodFundFormProps {
  readonly accountingPeriod: AccountingPeriod;
}

/**
 * Component that displays the form for creating a fund for an accounting period.
 */
const CreateAccountingPeriodFundForm = function ({
  accountingPeriod,
}: CreateAccountingPeriodFundFormProps): JSX.Element {
  const [state, action, pending] = useActionState(createAccountingPeriodFund, {
    accountingPeriodId: accountingPeriod.id,
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={[
          { label: "Accounting Periods", href: "/accounting-periods" },
          {
            label: accountingPeriod.name,
            href: `/accounting-periods/${accountingPeriod.id}`,
          },
          {
            label: "Create Fund",
            href: `/accounting-periods/${accountingPeriod.id}/fund/create`,
          },
        ]}
      />
      <form action={action}>
        <Stack spacing={2} sx={{ maxWidth: "500px" }}>
          <TextField
            name={nameof<CreateFundRequest>("name")}
            label="Name"
            defaultValue={state.name ?? null}
            variant="outlined"
            error={(state.nameErrorMessage ?? null) !== null}
            helperText={state.nameErrorMessage ?? null}
          />
          <TextField
            name={nameof<CreateFundRequest>("description")}
            label="Description"
            defaultValue={state.description ?? null}
            variant="outlined"
            error={(state.descriptionErrorMessage ?? null) !== null}
            helperText={state.descriptionErrorMessage ?? null}
          />
          <DateEntryField
            name={nameof<CreateFundRequest>("addDate")}
            label="Add Date"
            defaultValue={state.date ?? null}
            errorMessage={state.dateErrorMessage ?? null}
          />
          <DialogActions>
            <Link href={`/accounting-periods/${accountingPeriod.id}`}>
              <Button variant="outlined">Cancel</Button>
            </Link>
            <Button type="submit" variant="contained" loading={pending}>
              Create
            </Button>
          </DialogActions>
          <ErrorAlert errorMessage={state.overallErrorMessage ?? null} />
        </Stack>
      </form>
    </Stack>
  );
};

export default CreateAccountingPeriodFundForm;
