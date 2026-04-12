"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import Breadcrumbs from "@/framework/Breadcrumbs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import IntegerEntryField from "@/framework/forms/IntegerEntryField";
import Link from "next/link";
import createAccountingPeriod from "@/app/accounting-periods/create/createAccountingPeriod";
import routes from "@/framework/routes";

/**
 * Component that displays the create Accounting Period page.
 */
const Page = function (): JSX.Element {
  const [year, setYear] = useState<number | null>(null);
  const [month, setMonth] = useState<number | null>(null);
  const [state, action, pending] = useActionState(createAccountingPeriod, {});

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={[
          { label: "Accounting Periods", href: routes.accountingPeriods.index },
          { label: "Create", href: routes.accountingPeriods.create },
        ]}
      />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
        <IntegerEntryField
          label="Year"
          value={year}
          setValue={setYear}
          errorMessage={state.yearErrors ?? null}
        />
        <IntegerEntryField
          label="Month"
          value={month}
          setValue={setMonth}
          errorMessage={state.monthErrors ?? null}
        />
        <DialogActions>
          <Link href={routes.accountingPeriods.index} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={year === null || month === null}
            onClick={() => {
              startTransition(() => {
                action({ year: year ?? 0, month: month ?? 0 });
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

export default Page;
