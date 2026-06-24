"use client";

import { Button, Stack, Typography } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import dayjs, { type Dayjs } from "dayjs";
import DateEntryField from "@/framework/forms/DateEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { PostTransactionRequest } from "@/transactions/transaction";
import postTransaction from "@/transactions/workspace/postTransaction";
import { useRouter } from "next/navigation";

interface TransactionAccountPostActionProps {
  readonly transactionId: string;
  readonly accountId: string;
  readonly defaultDate: string;
  readonly redirectUrl: string;
}

/**
 * Handles posting a transaction to update the account ledger for a specific account.
 */
const TransactionAccountPostAction = function ({
  transactionId,
  accountId,
  defaultDate,
  redirectUrl,
}: TransactionAccountPostActionProps): JSX.Element {
  const router = useRouter();
  const [date, setDate] = useState<Dayjs | null>(dayjs(defaultDate));
  const [state, action, pending] = useActionState(postTransaction, {});

  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

  const request: PostTransactionRequest | null =
    date === null
      ? null
      : {
          accountId,
          date: date.format("YYYY-MM-DD"),
        };

  return (
    <Stack spacing={1.25} sx={{ paddingTop: 2 }}>
      <Stack
        direction={{ xs: "column", sm: "row" }}
        spacing={1.5}
        alignItems={{ xs: "stretch", sm: "flex-start" }}
      >
        <DateEntryField
          label="Posted Date"
          value={date}
          setValue={setDate}
          errorMessage={state.dateErrors ?? null}
        />
        <Button
          variant="contained"
          loading={pending}
          disabled={request === null || pending}
          sx={{ minWidth: 120 }}
          onClick={() => {
            if (request === null) {
              return;
            }
            startTransition(() => {
              action({
                transactionId,
                redirectUrl,
                request,
              });
            });
          }}
        >
          Post
        </Button>
      </Stack>
      {state.accountErrors !== null ? (
        <Typography variant="caption" color="error" sx={{ px: 1.75 }}>
          {state.accountErrors}
        </Typography>
      ) : null}
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />
    </Stack>
  );
};

export default TransactionAccountPostAction;
