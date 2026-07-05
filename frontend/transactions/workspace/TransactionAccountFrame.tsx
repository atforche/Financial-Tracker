"use client";

import type { Account, AccountIdentifier } from "@/accounts/types";
import { Button, Stack, Typography } from "@mui/material";
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
  TransactionAccountDraft,
} from "@/transactions/transaction";
import dayjs, { type Dayjs } from "dayjs";
import {
  getSelectedTransactionAccountDraft,
  setTransactionAccountDraftBalanceChange,
} from "@/transactions/workspace/transactionAccountDraft";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AccountEntryField from "@/accounts/AccountEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import TransactionBalanceDetails from "@/transactions/workspace/TransactionBalanceDetails";
import postTransaction from "@/transactions/workspace/postTransaction";

/**
 * Props for the TransactionAccountFrame component.
 */
interface TransactionAccountFrameProps {
  readonly accounts?: Account[];
  readonly transaction?: Transaction | null;
  readonly account: TransactionAccountDraft | null;
  readonly setAccount?:
    ((account: TransactionAccountDraft | null) => void) | null;
  readonly accountFilter?: ((account: AccountIdentifier) => boolean) | null;
  readonly label?: string;
  readonly balanceChange?: number | null;
}

const emptyAccounts: Account[] = [];

/**
 * Displays a transaction account and, when applicable, its posting controls.
 */
const TransactionAccountFrame = function ({
  accounts = emptyAccounts,
  transaction = null,
  account,
  setAccount = null,
  accountFilter = null,
  label = "Account",
  balanceChange = null,
}: TransactionAccountFrameProps): JSX.Element {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();

  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction?.date));
  const [state, action, pending] = useActionState(postTransaction, {});

  const currentSearch = searchParams.toString();
  const redirectUrl =
    currentSearch === "" ? pathname : `${pathname}?${currentSearch}`;

  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

  useEffect(() => {
    setDate(dayjs(transaction?.date));
  }, [transaction?.date, account?.accountId]);

  const request: PostTransactionRequest | null =
    date === null
      ? null
      : {
          accountId: account?.accountId ?? "",
          date: date.format("YYYY-MM-DD"),
        };

  const postedDate = account?.postedDate ?? null;

  const displayedAccount =
    postedDate === null
      ? setTransactionAccountDraftBalanceChange(account, balanceChange)
      : account;
  const newBalanceLabel = postedDate === null ? "Projected" : "New";

  let helperContent = null;
  if (postedDate !== null) {
    helperContent = (
      <Typography variant="caption" color="text.secondary" sx={{ px: 1.75 }}>
        Posted on {dayjs(postedDate).format("MMMM D, YYYY")}
      </Typography>
    );
  } else if (setAccount === null) {
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
                  transactionId: transaction?.id ?? "",
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
      <AccountEntryField
        label={label}
        options={accounts}
        value={{
          id: displayedAccount?.accountId ?? "",
          name: displayedAccount?.accountName ?? "",
        }}
        setValue={
          setAccount === null
            ? null
            : (nextValue: AccountIdentifier | null): void => {
                setAccount(
                  getSelectedTransactionAccountDraft(
                    accounts,
                    nextValue,
                    account,
                    balanceChange,
                  ),
                );
              }
        }
        filter={accountFilter}
      />
      <TransactionBalanceDetails
        previousPostedBalance={displayedAccount?.previousAccountBalance ?? 0}
        newPostedBalance={displayedAccount?.newAccountBalance ?? 0}
        newBalanceLabel={newBalanceLabel}
      />
      {helperContent}
    </Stack>
  );
};

export default TransactionAccountFrame;
