"use client";

import { Button, Stack, TextField, Typography } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import type {
  PostTransactionRequest,
  Transaction,
  TransactionAccount,
} from "@/transactions/transaction";
import dayjs, { type Dayjs } from "dayjs";
import {
  getPostableTransactionAccounts,
  getPostedTransactionAccounts,
} from "@/transactions/postingHelpers";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import DateEntryField from "@/framework/forms/DateEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import TransactionBalanceDetails from "@/transactions/workspace/TransactionBalanceDetails";
import { asAccountTransaction } from "@/transactions/accountTransaction";
import { asIncomeTransaction } from "@/transactions/incomeTransaction";
import { asSpendingTransaction } from "@/transactions/spendingTransaction";
import postTransaction from "@/transactions/workspace/postTransaction";

/**
 * Props for the TransactionAccountViewFrame component.
 */
interface TransactionAccountViewFrameProps {
  readonly transaction: Transaction;
  readonly account: TransactionAccount;
  readonly label?: string;
}

/**
 * Displays a transaction account and, when applicable, its posting controls.
 */
const TransactionAccountViewFrame = function ({
  transaction,
  account,
  label = "Account",
}: TransactionAccountViewFrameProps): JSX.Element {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();

  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [state, action, pending] = useActionState(postTransaction, {});
  const postedAccountsById = new Map(
    getPostedTransactionAccounts(transaction).map((postedAccount) => [
      postedAccount.accountId,
      postedAccount.postedDate,
    ]),
  );
  const postableAccountIds = new Set(
    getPostableTransactionAccounts(transaction).map(
      (postableAccount) => postableAccount.accountId,
    ),
  );

  const currentSearch = searchParams.toString();
  const redirectUrl =
    currentSearch === "" ? pathname : `${pathname}?${currentSearch}`;
  const postedDate = postedAccountsById.get(account.accountId) ?? null;
  const canPost =
    postedDate === null &&
    postableAccountIds.has(account.accountId) &&
    (asSpendingTransaction(transaction) !== null ||
      asIncomeTransaction(transaction) !== null ||
      asAccountTransaction(transaction) !== null);

  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

  useEffect(() => {
    setDate(dayjs(transaction.date));
  }, [transaction.date, account.accountId]);

  const request: PostTransactionRequest | null =
    date === null
      ? null
      : {
          accountId: account.accountId,
          date: date.format("YYYY-MM-DD"),
        };

  let helperContent = null;
  if (postedDate !== null) {
    helperContent = (
      <Typography variant="caption" color="text.secondary" sx={{ px: 1.75 }}>
        Posted on {dayjs(postedDate).format("MMMM D, YYYY")}
      </Typography>
    );
  } else if (canPost) {
    helperContent = (
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
                  transactionId: transaction.id,
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
  }

  return (
    <Stack spacing={0.75}>
      <TextField
        label={label}
        value={account.accountName}
        variant="outlined"
        slotProps={{
          input: {
            readOnly: true,
          },
        }}
      />
      <TransactionBalanceDetails
        previousPostedBalance={account.previousAccountBalance.postedBalance}
        newPostedBalance={account.newAccountBalance.postedBalance}
      />
      {helperContent}
    </Stack>
  );
};

export default TransactionAccountViewFrame;
