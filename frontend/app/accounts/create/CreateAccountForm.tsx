"use client";

import {
  type AccountingPeriod,
  getMaximumDate,
  getMinimumDate,
} from "@/data/accountingPeriodTypes";
import { Button, DialogActions, Stack } from "@mui/material";
import type { FundAmount, FundIdentifier } from "@/data/fundTypes";
import { type JSX, startTransition, useActionState, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { AccountType } from "@/data/accountTypes";
import AccountTypeEntryField from "@/framework/forms/AccountTypeEntryField";
import AccountingPeriodEntryField from "@/framework/forms/AccountingPeriodEntryField";
import Breadcrumbs from "@/framework/Breadcrumbs";
import DateEntryField from "@/framework/forms/DateEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FundAmountCollectionEntryFrame from "@/framework/forms/FundAmountCollectionEntryFrame";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import createAccount from "@/app/accounts/create/createAccount";

/**
 * Props for the CreateAccountForm component.
 */
interface CreateAccountFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly funds: FundIdentifier[];
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
            label: "Create Account",
            href: `/accounts/create`,
          },
        ]}
      />
    );
  }
  return (
    <Breadcrumbs
      breadcrumbs={[
        { label: "Accounts", href: "/accounts" },
        {
          label: "Create",
          href: `/accounts/create`,
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
    return `/accounting-periods/${providedAccountingPeriod.id}`;
  }
  return "/accounts";
};

/**
 * Component that displays the form for creating an account.
 */
const CreateAccountForm = function ({
  accountingPeriods,
  funds,
  providedAccountingPeriod = null,
}: CreateAccountFormProps): JSX.Element {
  const [name, setName] = useState<string>("");
  const [type, setType] = useState<AccountType | null>(null);
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(providedAccountingPeriod);
  const [addDate, setAddDate] = useState<Dayjs | null>(null);
  const [fundAmounts, setFundAmounts] = useState<FundAmount[]>([]);

  const [state, action, pending] = useActionState(createAccount, {
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
        <AccountTypeEntryField label="Type" value={type} setValue={setType} />
        <AccountingPeriodEntryField
          label="Accounting Period"
          options={accountingPeriods}
          value={accountingPeriod}
          setValue={
            providedAccountingPeriod === null ? setAccountingPeriod : null
          }
        />
        <DateEntryField
          label="Date Opened"
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
        <FundAmountCollectionEntryFrame
          label="Opening Balance"
          funds={funds}
          value={fundAmounts}
          setValue={setFundAmounts}
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
              type === null ||
              (addDate === null && defaultAddDate === null)
            }
            onClick={() => {
              if (
                name === "" ||
                accountingPeriod === null ||
                type === null ||
                (addDate === null && defaultAddDate === null)
              ) {
                return;
              }
              startTransition(() => {
                action({
                  name,
                  type,
                  accountingPeriodId: accountingPeriod.id,
                  addDate:
                    addDate?.format("YYYY-MM-DD") ??
                    defaultAddDate?.format("YYYY-MM-DD") ??
                    dayjs().format("YYYY-MM-DD"),
                  initialFundAmounts: fundAmounts,
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

export default CreateAccountForm;
