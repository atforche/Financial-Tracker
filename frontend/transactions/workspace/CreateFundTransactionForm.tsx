"use client";

import { Button, Stack } from "@mui/material";
import {
  CreateFundTransactionType,
  type CreateTransactionRequest,
  isFundTransactionComplete,
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
import type { AccountingPeriod } from "@/accounting-periods/types";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/funds/types";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import TransactionFundPairSection from "@/transactions/workspace/TransactionFundPairSection";
import createTransaction from "@/transactions/workspace/createTransaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";

interface CreateFundTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly funds: Fund[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated create form for fund transfer transactions.
 */
const CreateFundTransactionForm = function ({
  accountingPeriods,
  funds,
  redirectUrl,
}: CreateFundTransactionFormProps): JSX.Element {
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
  const [debitFund, setDebitFund] = useState<Fund | null>(null);
  const [creditFund, setCreditFund] = useState<Fund | null>(null);

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
    setDebitFund(null);
    setCreditFund(null);
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (state.success === true) {
      reset();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state]);

  let request: CreateTransactionRequest | null = null;
  if (
    accountingPeriod !== null &&
    (date !== null || defaultDate !== null) &&
    location !== "" &&
    description !== "" &&
    amount !== null &&
    amount > 0 &&
    isFundTransactionComplete(debitFund, creditFund)
  ) {
    request = {
      type: CreateFundTransactionType.Fund,
      accountingPeriodId: accountingPeriod.id,
      date:
        date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
      location,
      description,
      amount,
      debitFundId: debitFund?.id ?? "",
      creditFundId: creditFund?.id ?? "",
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
        <TransactionFundPairSection
          title="Transfer Path"
          description="Choose the source fund and the destination fund."
          funds={funds}
          leftLabel="Debit From"
          rightLabel="Credit To"
          leftFund={debitFund}
          rightFund={creditFund}
          setLeftFund={setDebitFund}
          setRightFund={setCreditFund}
          leftFilter={(fund) =>
            fund.name !== "Unassigned" && fund.id !== creditFund?.id
          }
          rightFilter={(fund) =>
            fund.name !== "Unassigned" && fund.id !== debitFund?.id
          }
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
            Create Fund Transfer
          </Button>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default CreateFundTransactionForm;
