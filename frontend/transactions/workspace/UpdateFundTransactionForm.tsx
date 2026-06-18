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
  UpdateFundTransactionType,
  type UpdateTransactionRequest,
} from "@/transactions/types";
import dayjs, { type Dayjs } from "dayjs";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/funds/types";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import TransactionFundPairSection from "@/transactions/workspace/TransactionFundPairSection";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import updateTransaction from "@/transactions/workspace/updateTransaction";

interface UpdateFundTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly transactionDebitFund: Fund | null;
  readonly transactionCreditFund: Fund | null;
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated update form for fund transfer transactions.
 */
const UpdateFundTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  transactionDebitFund,
  transactionCreditFund,
  redirectUrl,
}: UpdateFundTransactionFormProps): JSX.Element {
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
      type: UpdateFundTransactionType.Fund,
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
        <TransactionFundPairSection
          title="Transfer Path"
          description="Choose the source fund and the destination fund."
          funds={[transactionDebitFund, transactionCreditFund].filter(
            (fund): fund is Fund => fund !== null,
          )}
          leftLabel="Debit From"
          rightLabel="Credit To"
          leftFund={transactionDebitFund}
          rightFund={transactionCreditFund}
          setLeftFund={null}
          setRightFund={null}
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
            Update Fund Transfer
          </Button>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default UpdateFundTransactionForm;
