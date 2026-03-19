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
import AccountEntryField from "@/framework/forms/AccountEntryField";
import type { AccountIdentifier } from "@/data/accountTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import DateEntryField from "@/framework/forms/DateEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FundAmountCollectionEntryFrame from "@/framework/forms/FundAmountCollectionEntryFrame";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import createTransaction from "@/app/accounting-periods/[id]/transaction/create/createTransaction";

/**
 * Props for the CreateTransactionForm component.
 */
interface CreateTransactionFormProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly accounts: AccountIdentifier[];
  readonly funds: FundIdentifier[];
}

/**
 * Component that displays the form for creating a transaction.
 */
const CreateTransactionForm = function ({
  accountingPeriod,
  accounts,
  funds,
}: CreateTransactionFormProps): JSX.Element {
  const [date, setDate] = useState<Dayjs | null>(null);
  const [location, setLocation] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [debitAccount, setDebitAccount] = useState<AccountIdentifier | null>(
    null,
  );
  const [debitFundAmounts, setDebitFundAmounts] = useState<FundAmount[]>([]);
  const [creditAccount, setCreditAccount] = useState<AccountIdentifier | null>(
    null,
  );
  const [creditFundAmounts, setCreditFundAmounts] = useState<FundAmount[]>([]);
  const [state, action, pending] = useActionState(createTransaction, {});

  const defaultDate = dayjs(accountingPeriod.name, "MMMM YYYY");

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
            label: "Create Transaction",
            href: `/accounting-periods/${accountingPeriod.id}/transaction/create`,
          },
        ]}
      />
      <Stack spacing={2} sx={{ maxWidth: "800px" }}>
        <DateEntryField
          label="Date"
          value={date ?? defaultDate}
          setValue={setDate}
          minDate={getMinimumDate(accountingPeriod)}
          maxDate={getMaximumDate(accountingPeriod)}
        />
        <StringEntryField
          label="Location"
          value={location}
          setValue={setLocation}
        />
        <StringEntryField
          label="Description"
          value={description}
          setValue={setDescription}
        />
        <Stack direction="row" spacing={2}>
          <Stack spacing={2} sx={{ minWidth: 400 }}>
            <AccountEntryField
              label="Debit Account"
              options={accounts}
              value={debitAccount}
              setValue={setDebitAccount}
            />
            <FundAmountCollectionEntryFrame
              label="Debit Fund Amounts"
              funds={funds}
              value={debitFundAmounts}
              setValue={setDebitFundAmounts}
            />
          </Stack>
          <Stack spacing={2} sx={{ minWidth: 400 }}>
            <AccountEntryField
              label="Credit Account"
              options={accounts}
              value={creditAccount}
              setValue={setCreditAccount}
            />
            <FundAmountCollectionEntryFrame
              label="Credit Fund Amounts"
              funds={funds}
              value={creditFundAmounts}
              setValue={setCreditFundAmounts}
            />
          </Stack>
        </Stack>
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
            disabled={
              location.trim() === "" ||
              description.trim() === "" ||
              ((debitAccount === null || debitFundAmounts.length === 0) &&
                (creditAccount === null || creditFundAmounts.length === 0))
            }
            onClick={() => {
              startTransition(() => {
                action({
                  accountingPeriodId: accountingPeriod.id,
                  date:
                    date?.format("YYYY-MM-DD") ??
                    defaultDate.format("YYYY-MM-DD"),
                  location: location.trim(),
                  description: description.trim(),
                  debitAccount:
                    debitAccount === null
                      ? null
                      : {
                          accountId: debitAccount.id,
                          fundAmounts: debitFundAmounts,
                        },
                  creditAccount:
                    creditAccount === null
                      ? null
                      : {
                          accountId: creditAccount.id,
                          fundAmounts: creditFundAmounts,
                        },
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

export default CreateTransactionForm;
