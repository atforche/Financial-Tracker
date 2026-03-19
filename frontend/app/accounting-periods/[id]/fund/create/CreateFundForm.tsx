"use client";

import {
  type AccountingPeriod,
  getMaximumDate,
  getMinimumDate,
} from "@/data/accountingPeriodTypes";
import { Button, DialogActions, Stack } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import Breadcrumbs from "@/framework/Breadcrumbs";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import createFund from "@/app/accounting-periods/[id]/fund/create/createFund";

/**
 * Props for the CreateFundForm component.
 */
interface CreateFundFormProps {
  readonly accountingPeriod: AccountingPeriod;
}

/**
 * Component that displays the form for creating a fund.
 */
const CreateFundForm = function ({
  accountingPeriod,
}: CreateFundFormProps): JSX.Element {
  const [name, setName] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [addDate, setAddDate] = useState<Dayjs | null>(null);
  const [state, action, pending] = useActionState(createFund, {});

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
        <DateEntryField
          label="Add Date"
          value={addDate}
          setValue={setAddDate}
          errorMessage={state.dateErrors ?? null}
          minDate={getMinimumDate(accountingPeriod)}
          maxDate={getMaximumDate(accountingPeriod)}
        />
        <DialogActions>
          <Link
            href={`/accounting-periods/${accountingPeriod.id}`}
            tabIndex={-1}
          >
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={name === "" || description === "" || addDate === null}
            onClick={() => {
              startTransition(() => {
                action({
                  name,
                  description,
                  addDate: addDate?.format("YYYY-MM-DD") ?? "",
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
