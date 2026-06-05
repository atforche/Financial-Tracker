"use client";

import { Button, DialogActions, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import {
  type PostTransactionRequest,
  type Transaction,
  getPostableTransactionAccounts,
} from "@/transactions/types";
import dayjs, { type Dayjs } from "dayjs";
import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import postTransaction from "@/transactions/workspace/postTransaction";

/**
 * Props for the PostTransactionForm component.
 */
interface PostTransactionFormProps {
  readonly transaction: Transaction;
  readonly redirectUrl: string;
}

/**
 * Component that displays the form for posting a transaction.
 */
const PostTransactionForm = function ({
  transaction,
  redirectUrl,
}: PostTransactionFormProps): JSX.Element {
  const postableAccounts = getPostableTransactionAccounts(transaction);
  const [selectedAccount, setSelectedAccount] = useState(
    postableAccounts[0] ?? null,
  );
  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [state, action, pending] = useActionState(postTransaction, {});

  let request: PostTransactionRequest | null = null;
  if (selectedAccount !== null && date !== null) {
    request = {
      accountId: selectedAccount.accountId,
      date: date.format("YYYY-MM-DD"),
    };
  }

  return (
    <Stack spacing={2}>
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
              if (request === null) {
                return;
              }
              startTransition(() => {
                action({
                  transactionId: transaction.id,
                  redirectUrl,
                  request,
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
