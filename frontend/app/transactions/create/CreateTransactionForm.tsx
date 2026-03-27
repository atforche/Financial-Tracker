"use client";

import type { Account, AccountIdentifier } from "@/data/accountTypes";
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
import AccountingPeriodEntryField from "@/framework/forms/AccountingPeriodEntryField";
import Breadcrumbs from "@/framework/Breadcrumbs";
import DateEntryField from "@/framework/forms/DateEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FundAmountCollectionEntryFrame from "@/framework/forms/FundAmountCollectionEntryFrame";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import createTransaction from "@/app/transactions/create/createTransaction";

/**
 * Props for the CreateTransactionForm component.
 */
interface CreateTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: AccountIdentifier[];
  readonly funds: FundIdentifier[];
  readonly providedAccountingPeriod?: AccountingPeriod | null;
  readonly providedDebitAccount?: Account | null;
  readonly providedCreditAccount?: Account | null;
}

/**
 * Gets the breadcrumbs to be displayed at the top of the form.
 */
const getBreadcrumbs = function (
  accountingPeriod: AccountingPeriod | null,
  debitAccount: Account | null,
  creditAccount: Account | null,
): JSX.Element {
  const account = debitAccount ?? creditAccount;
  if (accountingPeriod !== null && account !== null) {
    return (
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Accounting Periods",
            href: "/accounting-periods",
          },
          {
            label: accountingPeriod.name,
            href: `/accounting-periods/${accountingPeriod.id}`,
          },
          {
            label: account.name,
            href: `/accounting-periods/${accountingPeriod.id}/accounts/${account.id}`,
          },
          {
            label: "Create Transaction",
            href: `/transactions/create`,
          },
        ]}
      />
    );
  }
  if (accountingPeriod !== null) {
    return (
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Accounting Periods",
            href: "/accounting-periods",
          },
          {
            label: accountingPeriod.name,
            href: `/accounting-periods/${accountingPeriod.id}`,
          },
          {
            label: "Create Transaction",
            href: `/transactions/create`,
          },
        ]}
      />
    );
  }
  if (account !== null) {
    return (
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Accounts",
            href: "/accounts",
          },
          {
            label: account.name,
            href: `/accounts/${account.id}`,
          },
          {
            label: "Create Transaction",
            href: `/transactions/create`,
          },
        ]}
      />
    );
  }
  throw new Error(
    "Invalid state: Create Transaction page must have either an associated accounting period or account",
  );
};

/**
 * Gets the URL to redirect the user to after successfully creating a transaction.
 */
const getRedirectUrl = function (
  accountingPeriod: AccountingPeriod | null,
  debitAccount: Account | null,
  creditAccount: Account | null,
): string {
  const account = debitAccount ?? creditAccount;
  if (accountingPeriod !== null && account !== null) {
    return `/accounting-periods/${accountingPeriod.id}/accounts/${account.id}`;
  }
  if (accountingPeriod !== null) {
    return `/accounting-periods/${accountingPeriod.id}`;
  }
  if (account !== null) {
    return `/accounts/${account.id}`;
  }
  throw new Error(
    "Invalid state: Create Transaction page must have either an associated accounting period or account",
  );
};

/**
 * Component that displays the form for creating a transaction.
 */
const CreateTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  providedAccountingPeriod,
  providedDebitAccount,
  providedCreditAccount,
}: CreateTransactionFormProps): JSX.Element {
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(providedAccountingPeriod ?? null);
  const [date, setDate] = useState<Dayjs | null>(null);
  const [location, setLocation] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [debitAccount, setDebitAccount] = useState<AccountIdentifier | null>(
    providedDebitAccount ?? null,
  );
  const [debitFundAmounts, setDebitFundAmounts] = useState<FundAmount[]>([]);
  const [creditAccount, setCreditAccount] = useState<AccountIdentifier | null>(
    providedCreditAccount ?? null,
  );
  const [creditFundAmounts, setCreditFundAmounts] = useState<FundAmount[]>([]);
  const [state, action, pending] = useActionState(createTransaction, {
    redirectUrl: getRedirectUrl(
      providedAccountingPeriod ?? null,
      providedDebitAccount ?? null,
      providedCreditAccount ?? null,
    ),
  });

  const defaultDate =
    accountingPeriod !== null
      ? dayjs(accountingPeriod.name, "MMMM YYYY")
      : null;

  return (
    <Stack spacing={2}>
      {getBreadcrumbs(
        providedAccountingPeriod ?? null,
        providedDebitAccount ?? null,
        providedCreditAccount ?? null,
      )}
      <Stack spacing={2} sx={{ maxWidth: "800px" }}>
        <AccountingPeriodEntryField
          label="Accounting Period"
          options={accountingPeriods}
          value={accountingPeriod}
          setValue={
            typeof providedAccountingPeriod !== "undefined" &&
            providedAccountingPeriod !== null
              ? null
              : setAccountingPeriod
          }
        />
        <DateEntryField
          label="Date"
          value={date ?? defaultDate}
          setValue={setDate}
          minDate={
            accountingPeriod !== null ? getMinimumDate(accountingPeriod) : null
          }
          maxDate={
            accountingPeriod !== null ? getMaximumDate(accountingPeriod) : null
          }
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
              setValue={
                typeof providedDebitAccount !== "undefined" &&
                providedDebitAccount !== null
                  ? null
                  : setDebitAccount
              }
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
              setValue={
                typeof providedCreditAccount !== "undefined" &&
                providedCreditAccount !== null
                  ? null
                  : setCreditAccount
              }
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
            href={getRedirectUrl(
              providedAccountingPeriod ?? null,
              providedDebitAccount ?? null,
              providedCreditAccount ?? null,
            )}
            tabIndex={-1}
          >
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={
              accountingPeriod === null ||
              (date === null && defaultDate === null) ||
              location.trim() === "" ||
              description.trim() === "" ||
              ((debitAccount === null || debitFundAmounts.length === 0) &&
                (creditAccount === null || creditFundAmounts.length === 0))
            }
            onClick={() => {
              if (
                accountingPeriod === null ||
                (date === null && defaultDate === null) ||
                location.trim() === "" ||
                description.trim() === "" ||
                ((debitAccount === null || debitFundAmounts.length === 0) &&
                  (creditAccount === null || creditFundAmounts.length === 0))
              ) {
                return;
              }
              startTransition(() => {
                action({
                  accountingPeriodId: accountingPeriod.id,
                  date:
                    date?.format("YYYY-MM-DD") ??
                    defaultDate?.format("YYYY-MM-DD") ??
                    dayjs().format("YYYY-MM-DD"),
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
