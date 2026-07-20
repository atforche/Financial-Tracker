"use client";

import type {
  Account,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import { Button, Stack, Typography } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import type { PostTransactionRequest, Transaction } from "@/transactions/types";
import dayjs, { type Dayjs } from "dayjs";
import {
  getSelectedTransactionAccountDraft,
  setTransactionAccountDraftBalanceChange,
} from "@/transactions/workspace/accountBalanceEventDraft";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AccountEntryField from "@/accounts/AccountEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import TransactionBalanceDetails from "@/transactions/workspace/TransactionBalanceDetails";
import { buildUrl } from "@/framework/routes/helpers";
import { formatLongDate } from "@/framework/dateHelpers";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";
import postTransaction from "@/transactions/workspace/postTransaction";

/**
 * Props for the AccountBalanceEventFrame component.
 */
interface AccountBalanceEventFrameProps {
  readonly accounts?: AccountWithBalance[];
  readonly transaction?: Transaction | null;
  readonly account: AccountBalanceEventDraft | null;
  readonly setAccount?:
    ((account: AccountBalanceEventDraft | null) => void) | null;
  readonly accountFilter?: ((account: Account) => boolean) | null;
  readonly label?: string;
  readonly balanceChange?: number | null;
}

const emptyAccounts: AccountWithBalance[] = [];

/**
 * Displays a transaction account and, when applicable, its posting controls.
 */
const AccountBalanceEventFrame = function ({
  accounts = emptyAccounts,
  transaction = null,
  account,
  setAccount = null,
  accountFilter = null,
  label = "Account",
  balanceChange = null,
}: AccountBalanceEventFrameProps): JSX.Element {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();

  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction?.date));
  const [state, action, pending] = useActionState(postTransaction, {});

  const redirectUrl = buildUrl(pathname, new URLSearchParams(searchParams));

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
  const displayedAccountId = displayedAccount?.accountId;
  const displayedAccountName = displayedAccount?.accountName;
  const displayedAccountType = displayedAccount?.accountType;
  const selectedAccount =
    accounts.find(({ id }) => id === displayedAccountId) ?? null;
  const readOnlyAccount =
    setAccount === null &&
    isNotNullOrUndefined(displayedAccountId) &&
    isNotNullOrUndefined(displayedAccountName) &&
    isNotNullOrUndefined(displayedAccountType)
      ? {
          id: displayedAccountId,
          name: displayedAccountName,
          type: displayedAccountType,
        }
      : null;
  const newBalanceLabel = postedDate === null ? "Projected" : "New";

  let helperContent = null;
  if (postedDate !== null) {
    helperContent = (
      <Typography variant="caption" color="text.secondary" sx={{ px: 1.75 }}>
        Posted on {formatLongDate(new Date(postedDate))}
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
        value={selectedAccount ?? readOnlyAccount}
        setValue={
          setAccount === null
            ? null
            : (nextValue: Account | null): void => {
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

export default AccountBalanceEventFrame;
