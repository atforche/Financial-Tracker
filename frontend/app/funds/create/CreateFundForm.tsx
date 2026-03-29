"use client";

import {
  type AccountingPeriod,
  getMaximumDate,
  getMinimumDate,
} from "@/data/accountingPeriodTypes";
import { Button, DialogActions, Stack } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import AccountingPeriodEntryField from "@/framework/forms/AccountingPeriodEntryField";
import Breadcrumbs from "@/framework/Breadcrumbs";
import DateEntryField from "@/framework/forms/DateEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import createFund from "@/app/funds/create/createFund";

/**
 * Props for the CreateFundForm component.
 */
interface CreateFundFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly providedAccountingPeriod?: AccountingPeriod | null;
}

/**
 * Gets the breadcrumbs to be displayed at the top of the form.
 */
const getBreadcrumbs = function (
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
            label: "Create Fund",
            href: `/funds/create`,
          },
        ]}
      />
    );
  }
  return (
    <Breadcrumbs
      breadcrumbs={[
        { label: "Funds", href: "/funds" },
        {
          label: "Create",
          href: `/funds/create`,
        },
      ]}
    />
  );
};

/**
 * Gets the URL to redirect the user to after successfully creating a fund.
 */
const getRedirectUrl = function (
  providedAccountingPeriod: AccountingPeriod | null,
): string {
  if (providedAccountingPeriod !== null) {
    return `/accounting-periods/${providedAccountingPeriod.id}`;
  }
  return "/funds";
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
  const [addDate, setAddDate] = useState<Dayjs | null>(null);

  const [state, action, pending] = useActionState(createFund, {
    redirectUrl: getRedirectUrl(providedAccountingPeriod),
  });

  const defaultAddDate =
    accountingPeriod === null
      ? null
      : dayjs(accountingPeriod.name, "MMMM YYYY");

  return (
    <Stack spacing={2}>
      {getBreadcrumbs(providedAccountingPeriod)}
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
        />
        <DateEntryField
          label="Date Added"
          value={addDate ?? defaultAddDate}
          setValue={setAddDate}
          errorMessage={state.dateErrors ?? null}
          minDate={
            accountingPeriod === null ? null : getMinimumDate(accountingPeriod)
          }
          maxDate={
            accountingPeriod === null ? null : getMaximumDate(accountingPeriod)
          }
        />
        <DialogActions>
          <Link href={getRedirectUrl(providedAccountingPeriod)} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={
              name === "" ||
              accountingPeriod === null ||
              (addDate === null && defaultAddDate === null)
            }
            onClick={() => {
              if (
                name === "" ||
                accountingPeriod === null ||
                (addDate === null && defaultAddDate === null)
              ) {
                return;
              }
              startTransition(() => {
                action({
                  name,
                  description,
                  accountingPeriodId: accountingPeriod.id,
                  addDate:
                    addDate?.format("YYYY-MM-DD") ??
                    defaultAddDate?.format("YYYY-MM-DD") ??
                    dayjs().format("YYYY-MM-DD"),
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
