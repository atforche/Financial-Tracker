"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@/data/accountTypes";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import DateEntryField from "@/framework/forms/DateEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/data/fundTypes";
import Link from "next/link";
import type { Transaction } from "@/data/transactionTypes";
import getBreadcrumbs from "@/app/transactions/[id]/post/getBreadcrumbs";
import postTransaction from "@/app/transactions/[id]/post/postTransaction";

/**
 * Props for the PostTransactionForm component.
 */
interface PostTransactionFormProps {
  readonly transaction: Transaction;
  readonly account: "debit" | "credit";
  readonly redirectUrl: string;
  readonly providedAccountingPeriod?: AccountingPeriod | null;
  readonly providedAccount?: Account | null;
  readonly providedFund?: Fund | null;
}

/**
 * Component that displays the form for posting a transaction to an account.
 */
const PostTransactionForm = function ({
  transaction,
  account,
  redirectUrl,
  providedAccountingPeriod = null,
  providedAccount = null,
  providedFund = null,
}: PostTransactionFormProps): JSX.Element {
  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));

  const accountId =
    (account === "debit"
      ? transaction.debitAccount?.accountId
      : transaction.creditAccount?.accountId) ?? "";
  const [state, action, pending] = useActionState(postTransaction, {
    transactionId: transaction.id,
    accountId,
    redirectUrl,
  });

  return (
    <Stack spacing={2} maxWidth={800}>
      <Breadcrumbs
        breadcrumbs={getBreadcrumbs(
          transaction,
          providedAccountingPeriod,
          providedAccount,
          providedFund,
        )}
      />
      <DateEntryField
        label="Posted Date"
        value={date}
        setValue={setDate}
        errorMessage={state.dateErrors ?? null}
      />
      <DialogActions sx={{ maxWidth: "800px" }}>
        <Link href={redirectUrl} tabIndex={-1}>
          <Button variant="outlined">Cancel</Button>
        </Link>
        <Button
          variant="contained"
          loading={pending}
          disabled={date === null}
          onClick={() => {
            if (date === null) {
              return;
            }
            startTransition(() => {
              action({
                accountId,
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
  );
};

export default PostTransactionForm;
