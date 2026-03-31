"use client";

import {
  type AccountingPeriod,
  getMaximumDate,
  getMinimumDate,
} from "@/data/accountingPeriodTypes";
import { Button, DialogActions, Stack } from "@mui/material";
import type { Fund, FundAmount, FundIdentifier } from "@/data/fundTypes";
import { type JSX, startTransition, useActionState, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@/data/accountTypes";
import AccountEntryField from "@/framework/forms/AccountEntryField";
import AccountingPeriodEntryField from "@/framework/forms/AccountingPeriodEntryField";
import Breadcrumbs from "@/framework/Breadcrumbs";
import DateEntryField from "@/framework/forms/DateEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FundAmountCollectionEntryFrame from "@/framework/forms/FundAmountCollectionEntryFrame";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import type { Transaction } from "@/data/transactionTypes";
import updateTransaction from "@/app/transactions/[id]/update/updateTransaction";

/**
 * Props for the UpdateTransactionForm component.
 */
interface UpdateTransactionFormProps {
  readonly transaction: Transaction;
  readonly accountingPeriod: AccountingPeriod;
  readonly funds: FundIdentifier[];
  readonly providedAccountingPeriod?: AccountingPeriod | null;
  readonly providedAccount?: Account | null;
  readonly providedFund?: Fund | null;
}

/**
 * Gets the breadcrumbs to be displayed at the top of the form.
 */
const getBreadcrumbs = function (
  transaction: Transaction,
  accountingPeriod: AccountingPeriod | null,
  account: Account | null,
  fund: Fund | null,
): JSX.Element {
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
            label: "Transaction",
            href: `/transactions/${transaction.id}`,
          },
          {
            label: "Update",
            href: `/transactions/${transaction.id}/update`,
          },
        ]}
      />
    );
  }
  if (accountingPeriod !== null && fund !== null) {
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
            label: fund.name,
            href: `/accounting-periods/${accountingPeriod.id}/funds/${fund.id}`,
          },
          {
            label: "Transaction",
            href: `/transactions/${transaction.id}`,
          },
          {
            label: "Update",
            href: `/transactions/${transaction.id}/update`,
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
            label: "Transaction",
            href: `/transactions/${transaction.id}`,
          },
          {
            label: "Update",
            href: `/transactions/${transaction.id}/update`,
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
            label: "Transaction",
            href: `/transactions/${transaction.id}`,
          },
          {
            label: "Update",
            href: `/transactions/${transaction.id}/update`,
          },
        ]}
      />
    );
  }
  if (fund !== null) {
    return (
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Funds",
            href: "/funds",
          },
          {
            label: fund.name,
            href: `/funds/${fund.id}`,
          },
          {
            label: "Transaction",
            href: `/transactions/${transaction.id}`,
          },
          {
            label: "Update",
            href: `/transactions/${transaction.id}/update`,
          },
        ]}
      />
    );
  }
  throw new Error(
    "Invalid state: Update Transaction page must have either an associated accounting period, account, or fund",
  );
};

/**
 * Gets the URL to redirect the user to after successfully updating a transaction.
 */
const getRedirectUrl = function (
  transaction: Transaction,
  accountingPeriod: AccountingPeriod | null,
  account: Account | null,
  fund: Fund | null,
): string {
  const params = new URLSearchParams();
  if (accountingPeriod !== null) {
    params.set("accountingPeriodId", accountingPeriod.id);
  }
  if (account !== null) {
    params.set("accountId", account.id);
  }
  if (fund !== null) {
    params.set("fundId", fund.id);
  }
  const queryString = params.toString();
  return `/transactions/${transaction.id}${queryString ? `?${queryString}` : ""}`;
};

/**
 * Component that displays the form for updating a transaction.
 */
const UpdateTransactionForm = function ({
  transaction,
  accountingPeriod,
  funds,
  providedAccountingPeriod = null,
  providedAccount = null,
  providedFund = null,
}: UpdateTransactionFormProps): JSX.Element {
  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [location, setLocation] = useState<string>(transaction.location);
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [debitFundAmounts, setDebitFundAmounts] = useState<FundAmount[]>(
    transaction.debitAccount?.fundAmounts.map((fa) => ({
      fundId: fa.fundId,
      fundName: fa.fundName,
      amount: fa.amount,
    })) ?? [],
  );
  const [creditFundAmounts, setCreditFundAmounts] = useState<FundAmount[]>(
    transaction.creditAccount?.fundAmounts.map((fa) => ({
      fundId: fa.fundId,
      fundName: fa.fundName,
      amount: fa.amount,
    })) ?? [],
  );
  const [state, action, pending] = useActionState(updateTransaction, {
    transactionId: transaction.id,
    redirectUrl: getRedirectUrl(
      transaction,
      providedAccountingPeriod,
      providedAccount,
      providedFund,
    ),
  });

  return (
    <Stack spacing={2}>
      {getBreadcrumbs(
        transaction,
        providedAccountingPeriod,
        providedAccount,
        providedFund,
      )}
      <Stack spacing={2} sx={{ maxWidth: "800px" }}>
        <AccountingPeriodEntryField
          label="Accounting Period"
          options={[accountingPeriod]}
          value={accountingPeriod}
        />
        <DateEntryField
          label="Date"
          value={date}
          setValue={setDate}
          minDate={getMinimumDate(accountingPeriod)}
          maxDate={getMaximumDate(accountingPeriod)}
          errorMessage={state.dateErrors ?? null}
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
          {transaction.debitAccount !== null &&
          typeof transaction.debitAccount !== "undefined" ? (
            <Stack spacing={2} sx={{ minWidth: 400 }}>
              <AccountEntryField
                label="Debit Account"
                options={[
                  {
                    id: transaction.debitAccount.accountId,
                    name: transaction.debitAccount.accountName,
                  },
                ]}
                value={{
                  id: transaction.debitAccount.accountId,
                  name: transaction.debitAccount.accountName,
                }}
                setValue={null}
              />
              <FundAmountCollectionEntryFrame
                label="Debit Fund Amounts"
                funds={funds}
                value={debitFundAmounts}
                setValue={setDebitFundAmounts}
                lockedFundIds={[]}
              />
            </Stack>
          ) : null}
          {transaction.creditAccount !== null &&
          typeof transaction.creditAccount !== "undefined" ? (
            <Stack spacing={2} sx={{ minWidth: 400 }}>
              <AccountEntryField
                label="Credit Account"
                options={[
                  {
                    id: transaction.creditAccount.accountId,
                    name: transaction.creditAccount.accountName,
                  },
                ]}
                value={{
                  id: transaction.creditAccount.accountId,
                  name: transaction.creditAccount.accountName,
                }}
                setValue={null}
              />
              <FundAmountCollectionEntryFrame
                label="Credit Fund Amounts"
                funds={funds}
                value={creditFundAmounts}
                setValue={setCreditFundAmounts}
                lockedFundIds={[]}
              />
            </Stack>
          ) : null}
        </Stack>
        <DialogActions>
          <Link
            href={getRedirectUrl(
              transaction,
              providedAccountingPeriod,
              providedAccount,
              providedFund,
            )}
            tabIndex={-1}
          >
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={
              date === null ||
              location.trim() === "" ||
              description.trim() === "" ||
              (transaction.debitAccount !== null &&
                typeof transaction.debitAccount !== "undefined" &&
                debitFundAmounts.length === 0) ||
              (transaction.creditAccount !== null &&
                typeof transaction.creditAccount !== "undefined" &&
                creditFundAmounts.length === 0)
            }
            onClick={() => {
              if (
                date === null ||
                location.trim() === "" ||
                description.trim() === ""
              ) {
                return;
              }
              startTransition(() => {
                action({
                  date: date.format("YYYY-MM-DD"),
                  location: location.trim(),
                  description: description.trim(),
                  debitAccount:
                    transaction.debitAccount === null ||
                    typeof transaction.debitAccount === "undefined"
                      ? null
                      : {
                          fundAmounts: debitFundAmounts,
                        },
                  creditAccount:
                    transaction.creditAccount === null ||
                    typeof transaction.creditAccount === "undefined"
                      ? null
                      : {
                          fundAmounts: creditFundAmounts,
                        },
                });
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

export default UpdateTransactionForm;
