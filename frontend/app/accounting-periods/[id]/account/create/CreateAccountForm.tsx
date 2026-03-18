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
import { AccountType } from "@/data/accountTypes";
import AccountTypeEntryField from "@/framework/forms/AccountTypeEntryField";
import Breadcrumbs from "@/framework/Breadcrumbs";
import DateEntryField from "@/framework/forms/DateEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FundAmountCollectionEntryFrame from "@/framework/forms/FundAmountCollectionEntryFrame";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import createAccount from "@/app/accounting-periods/[id]/account/create/createAccount";

/**
 * Props for the CreateAccountForm component.
 */
interface CreateAccountFormProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly funds: FundIdentifier[];
}

/**
 * Component that displays the form for creating an account.
 */
const CreateAccountForm = function ({
  accountingPeriod,
  funds,
}: CreateAccountFormProps): JSX.Element {
  const [name, setName] = useState<string>("");
  const [type, setType] = useState<AccountType | null>(null);
  const [addDate, setAddDate] = useState<Dayjs | null>(null);
  const [fundAmounts, setFundAmounts] = useState<FundAmount[]>([]);

  const [state, action, pending] = useActionState(createAccount, {});

  const defaultAddDate = dayjs(accountingPeriod.name, "MMMM YYYY");

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
            label: "Create Account",
            href: `/accounting-periods/${accountingPeriod.id}/account/create`,
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
        <AccountTypeEntryField label="Type" value={type} setValue={setType} />
        <DateEntryField
          label="Date Opened"
          value={addDate ?? defaultAddDate}
          setValue={setAddDate}
          errorMessage={state.dateErrors ?? null}
          minDate={getMinimumDate(accountingPeriod)}
          maxDate={getMaximumDate(accountingPeriod)}
        />
        <FundAmountCollectionEntryFrame
          label="Opening Balance"
          funds={funds}
          value={fundAmounts}
          setValue={setFundAmounts}
        />
        <DialogActions>
          <Link href={`/accounting-periods/${accountingPeriod.id}`}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={name === "" || type === null}
            onClick={() => {
              startTransition(() => {
                action({
                  name,
                  type: type ?? AccountType.Standard,
                  accountingPeriodId: accountingPeriod.id,
                  addDate:
                    addDate?.format("YYYY-MM-DD") ??
                    defaultAddDate.format("YYYY-MM-DD"),
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
