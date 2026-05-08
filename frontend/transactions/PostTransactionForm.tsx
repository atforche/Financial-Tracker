"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import {
  type Transaction,
  getPostableTransactionAccounts,
} from "@/transactions/types";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import breadcrumbs from "@/transactions/breadcrumbs";
import postTransaction from "@/transactions/postTransaction";
import routes from "@/transactions/routes";

/**
 * Props for the PostTransactionForm component.
 */
interface PostTransactionFormProps {
  readonly transaction: Transaction;
  readonly routeAccountingPeriod: AccountingPeriod | null;
  readonly routeAccount: Account | null;
}

/**
 * Component that displays the form for posting a transaction.
 */
const PostTransactionForm = function ({
  transaction,
  routeAccountingPeriod,
  routeAccount,
}: PostTransactionFormProps): JSX.Element {
  const postableAccounts = getPostableTransactionAccounts(transaction);
  const [selectedAccount, setSelectedAccount] = useState(
    postableAccounts.find(
      (account) => account.accountId === (routeAccount?.id ?? ""),
    ) ??
      postableAccounts[0] ??
      null,
  );
  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const redirectUrl = routes.detail(
    { id: transaction.id },
    {
      accountingPeriodId: routeAccountingPeriod?.id ?? null,
      accountId: routeAccount?.id ?? null,
    },
  );
  const [state, action, pending] = useActionState(postTransaction, {
    transactionId: transaction.id,
    redirectUrl,
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={breadcrumbs.post(
          transaction,
          routeAccountingPeriod,
          routeAccount,
        )}
      />
      <Stack spacing={2} sx={{ maxWidth: "600px" }}>
        {postableAccounts.length === 0 ? (
          <Typography>
            This transaction has no accounts left to post.
          </Typography>
        ) : (
          <>
            <ComboBoxEntryField
              label="Account"
              options={postableAccounts.map((account) => ({
                label: account.accountName,
                value: account,
              }))}
              value={
                selectedAccount === null
                  ? { label: "", value: null }
                  : {
                      label: selectedAccount.accountName,
                      value: selectedAccount,
                    }
              }
              setValue={(newValue): void => {
                setSelectedAccount(newValue?.value ?? null);
              }}
              errorMessage={state.accountErrors ?? null}
            />
            <DateEntryField
              label="Posted Date"
              value={date}
              setValue={setDate}
              errorMessage={state.dateErrors ?? null}
            />
          </>
        )}
        <DialogActions>
          <Link href={redirectUrl} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={
              postableAccounts.length === 0 ||
              selectedAccount === null ||
              date === null
            }
            onClick={() => {
              if (selectedAccount === null || date === null) {
                return;
              }

              startTransition(() => {
                action({
                  accountId: selectedAccount.accountId,
                  date: date.format("YYYY-MM-DD"),
                });
              });
            }}
          >
            Post
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

export { getPostableTransactionAccounts };
export default PostTransactionForm;
