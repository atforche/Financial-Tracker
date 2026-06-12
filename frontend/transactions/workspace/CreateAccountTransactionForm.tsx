"use client";

import { Button, Stack } from "@mui/material";
import {
  CreateAccountTransactionType,
  type CreateTransactionRequest,
  isAccountTransaction,
} from "@/transactions/types";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import TransactionAccountPairSection from "@/transactions/workspace/TransactionAccountPairSection";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import createTransaction from "@/transactions/workspace/createTransaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import { useRouter } from "next/navigation";

interface CreateAccountTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: Account[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated create form for account transactions.
 */
const CreateAccountTransactionForm = function ({
  accountingPeriods,
  accounts,
  redirectUrl,
}: CreateAccountTransactionFormProps): JSX.Element {
  const router = useRouter();
  const formRef = useRef<HTMLDivElement | null>(null);
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(
      accountingPeriods.length > 0
        ? (accountingPeriods[accountingPeriods.length - 1] ?? null)
        : null,
    );
  const defaultDate =
    accountingPeriod !== null
      ? dayjs(`${accountingPeriod.year}-${accountingPeriod.month}-01`)
      : null;
  const [date, setDate] = useState<Dayjs | null>(null);
  const [location, setLocation] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [amount, setAmount] = useState<number | null>(null);
  const [debitAccount, setDebitAccount] = useState<Account | null>(null);
  const [creditAccount, setCreditAccount] = useState<Account | null>(null);

  const [state, action, pending] = useActionState(createTransaction, {});

  const reset = function (): void {
    setAccountingPeriod(
      accountingPeriods.length > 0
        ? (accountingPeriods[accountingPeriods.length - 1] ?? null)
        : null,
    );
    setDate(null);
    setLocation("");
    setDescription("");
    setAmount(null);
    setDebitAccount(null);
    setCreditAccount(null);
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (state.success === true && state.transactionId !== null) {
      const nextUrl = `${redirectUrl}${redirectUrl.includes("?") ? "&" : "?"}selectedTransactionId=${state.transactionId}`;
      router.replace(nextUrl, { scroll: false });
      reset();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [redirectUrl, router, state]);

  let request: CreateTransactionRequest | null = null;
  if (
    accountingPeriod !== null &&
    (date !== null || defaultDate !== null) &&
    location !== "" &&
    description !== "" &&
    amount !== null &&
    amount > 0 &&
    isAccountTransaction(debitAccount, creditAccount, null, null)
  ) {
    request = {
      type: CreateAccountTransactionType.Account,
      accountingPeriodId: accountingPeriod.id,
      date:
        date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
      location,
      description,
      amount,
      debitAccountId: debitAccount?.id ?? null,
      creditAccountId: creditAccount?.id ?? null,
    };
  }

  return (
    <Stack ref={formRef} spacing={3}>
      <Stack spacing={3} sx={{ width: "100%" }}>
        <TransactionDetailsSection
          accountingPeriods={accountingPeriods}
          accountingPeriod={accountingPeriod}
          setAccountingPeriod={setAccountingPeriod}
          date={date ?? defaultDate}
          setDate={setDate}
          location={location}
          setLocation={setLocation}
          descriptionValue={description}
          setDescriptionValue={setDescription}
          amount={amount}
          setAmount={setAmount}
        />
        <TransactionAccountPairSection
          title="Transfer Path"
          description="Choose the source and destination accounts."
          accounts={accounts}
          leftLabel="Debit From"
          rightLabel="Credit To"
          leftAccount={debitAccount}
          rightAccount={creditAccount}
          setLeftAccount={setDebitAccount}
          setRightAccount={setCreditAccount}
          leftFilter={(account) => account.id !== creditAccount?.id}
          rightFilter={(account) => account.id !== debitAccount?.id}
        />
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={1.5}
          justifyContent="flex-end"
        >
          <Button variant="outlined" onClick={reset}>
            Reset
          </Button>
          <Button
            variant="contained"
            loading={pending}
            disabled={request === null}
            onClick={() => {
              if (request === null) {
                return;
              }
              startTransition(() => {
                action({ redirectUrl, request });
              });
            }}
          >
            Create Account Transaction
          </Button>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default CreateAccountTransactionForm;
