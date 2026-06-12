"use client";

import { Button, Stack, Typography } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import {
  type PostTransactionRequest,
  type Transaction,
  getPostableTransactionAccounts,
  getPostedTransactionAccounts,
} from "@/transactions/types";
import dayjs, { type Dayjs } from "dayjs";
import DateEntryField from "@/framework/forms/DateEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import TransactionDisplayField from "@/transactions/workspace/TransactionDisplayField";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import postTransaction from "@/transactions/workspace/postTransaction";

interface AccountPostingState {
  readonly accountId: string;
  readonly accountName: string;
  readonly postedDate: string | null;
}

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
  const postableAccounts = getPostableTransactionAccounts(transaction).map(
    (account) => ({
      ...account,
      postedDate: null,
    }),
  );
  const postedAccounts = getPostedTransactionAccounts(transaction).map(
    (account) => {
      const matchingAccount =
        "debitAccount" in transaction &&
        transaction.debitAccount?.accountId === account.accountId
          ? transaction.debitAccount
          : "creditAccount" in transaction &&
              transaction.creditAccount?.accountId === account.accountId
            ? transaction.creditAccount
            : null;
      return {
        ...account,
        postedDate: matchingAccount?.postedDate ?? null,
      };
    },
  );
  const accountPostings: AccountPostingState[] = [
    ...postableAccounts,
    ...postedAccounts,
  ];
  const [datesByAccountId, setDatesByAccountId] = useState<
    Record<string, Dayjs | null>
  >(() =>
    Object.fromEntries(
      accountPostings.map((account) => [
        account.accountId,
        dayjs(account.postedDate ?? transaction.date),
      ]),
    ),
  );
  const [activeAccountId, setActiveAccountId] = useState<string | null>(null);
  const [state, action, pending] = useActionState(postTransaction, {});

  const setAccountDate = function (
    accountId: string,
    newDate: Dayjs | null,
  ): void {
    setDatesByAccountId((currentDates) => ({
      ...currentDates,
      [accountId]: newDate,
    }));
  };

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <TransactionSection
        title="Account Posting"
        description="Specify the posting date for each affected account."
      >
        <Stack spacing={2}>
          {accountPostings.length === 0 ? (
            <Typography>
              This transaction does not affect any accounts.
            </Typography>
          ) : (
            accountPostings.map((account) => {
              const date = datesByAccountId[account.accountId] ?? null;
              const isAlreadyPosted = account.postedDate !== null;
              const request: PostTransactionRequest | null =
                date === null
                  ? null
                  : {
                      accountId: account.accountId,
                      date: date.format("YYYY-MM-DD"),
                    };
              const isActiveRow = activeAccountId === account.accountId;

              return (
                <Stack
                  key={account.accountId}
                  direction={{ xs: "column", md: "row" }}
                  spacing={2}
                  alignItems={{ xs: "stretch", md: "flex-start" }}
                  sx={{
                    p: 2,
                    borderRadius: 3,
                    border: (theme) => `1px solid ${theme.palette.divider}`,
                  }}
                >
                  <Stack
                    direction={{ xs: "column", sm: "row" }}
                    spacing={2}
                    sx={{ flex: 1 }}
                  >
                    <TransactionDisplayField
                      label="Account"
                      value={account.accountName}
                      helperText={
                        isActiveRow ? (state.accountErrors ?? null) : null
                      }
                    />
                    <DateEntryField
                      label="Posted Date"
                      value={date}
                      setValue={
                        isAlreadyPosted
                          ? null
                          : (newValue): void => {
                              setAccountDate(account.accountId, newValue);
                            }
                      }
                      errorMessage={
                        isActiveRow ? (state.dateErrors ?? null) : null
                      }
                    />
                  </Stack>
                  {isAlreadyPosted ? (
                    <Button variant="outlined" disabled sx={{ minWidth: 140 }}>
                      Already Posted
                    </Button>
                  ) : (
                    <Button
                      variant="contained"
                      loading={pending ? isActiveRow : null}
                      disabled={pending || request === null}
                      sx={{ minWidth: 140 }}
                      onClick={() => {
                        if (request === null) {
                          return;
                        }
                        setActiveAccountId(account.accountId);
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
                  )}
                </Stack>
              );
            })
          )}
        </Stack>
      </TransactionSection>
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />
    </Stack>
  );
};

export { getPostableTransactionAccounts };
export default PostTransactionForm;
