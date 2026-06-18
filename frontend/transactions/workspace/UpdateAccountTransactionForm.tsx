"use client";

import { Button, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import {
  type Transaction,
  UpdateAccountTransactionType,
  type UpdateTransactionRequest,
} from "@/transactions/types";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import TransactionAccountPairSection from "@/transactions/workspace/TransactionAccountPairSection";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import updateTransaction from "@/transactions/workspace/updateTransaction";

interface UpdateAccountTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly transactionDebitAccount: Account | null;
  readonly transactionCreditAccount: Account | null;
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated update form for account transactions.
 */
const UpdateAccountTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  transactionDebitAccount,
  transactionCreditAccount,
  redirectUrl,
}: UpdateAccountTransactionFormProps): JSX.Element {
  const formRef = useRef<HTMLDivElement | null>(null);
  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [amount, setAmount] = useState<number | null>(transaction.amount);

  const [state, action, pending] = useActionState(updateTransaction, {});

  const reset = function (): void {
    setDate(dayjs(transaction.date));
    setDescription(transaction.description);
    setAmount(transaction.amount);
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (state.success === true) {
      reset();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state]);

  let request: UpdateTransactionRequest | null = null;
  if (date !== null && description !== "" && amount !== null && amount > 0) {
    request = {
      type: UpdateAccountTransactionType.Account,
      date: date.format("YYYY-MM-DD"),
      description,
      amount,
    };
  }

  return (
    <Stack ref={formRef} spacing={3}>
      <Stack spacing={3} sx={{ width: "100%" }}>
        <TransactionDetailsSection
          accountingPeriods={[transactionAccountingPeriod]}
          accountingPeriod={transactionAccountingPeriod}
          setAccountingPeriod={null}
          date={date}
          setDate={setDate}
          descriptionValue={description}
          setDescriptionValue={setDescription}
          amount={amount}
          setAmount={setAmount}
        />
        <TransactionAccountPairSection
          title="Transfer Path"
          description="Choose the source and destination accounts."
          accounts={[transactionDebitAccount, transactionCreditAccount].filter(
            (account): account is Account => account !== null,
          )}
          leftLabel="Debit From"
          rightLabel="Credit To"
          leftAccount={transactionDebitAccount}
          rightAccount={transactionCreditAccount}
          setLeftAccount={null}
          setRightAccount={null}
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
                action({ transactionId: transaction.id, redirectUrl, request });
              });
            }}
          >
            Update Account Transaction
          </Button>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default UpdateAccountTransactionForm;
