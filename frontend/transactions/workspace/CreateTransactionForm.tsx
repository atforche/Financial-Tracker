"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import {
  CreateAccountTransactionType,
  CreateFundTransactionType,
  CreateIncomeTransactionType,
  CreateSpendingTransactionType,
  type CreateTransactionRequest,
  isAccountTransaction,
  isFundTransaction,
  isFundTransactionComplete,
  isIncomeTransaction,
  isIncomeTransactionComplete,
  isSpendingTransaction,
  isSpendingTransactionComplete,
} from "@/transactions/types";
import type { Fund, FundAmount } from "@/funds/types";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateOrUpdateIncomeTransactionFrame from "@/transactions/workspace/CreateOrUpdateIncomeTransactionFrame";
import CreateOrUpdateSpendingTransactionFrame from "@/transactions/workspace/CreateOrUpdateSpendingTransactionFrame";
import CreateOrUpdateTransactionDetailsFrame from "@/transactions/workspace/CreateOrUpdateTransactionDetailsFrame";
import CreateOrUpdateTransactionFromToFrame from "@/transactions/workspace/CreateOrUpdateTransactionFromToFrame";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import createTransaction from "@/transactions/workspace/createTransaction";
import { updateUnassignedFundAmount } from "@/funds/FundAssignmentEntryFrame";

/**
 * Props for the CreateTransactionForm component.
 */
interface CreateTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly redirectUrl: string;
}

/**
 * Component that displays the form for creating a transaction.
 */
const CreateTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  redirectUrl,
}: CreateTransactionFormProps): JSX.Element {
  const unassignedFund =
    funds.find((fund) => fund.name === "Unassigned") ?? null;

  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(
      accountingPeriods.length > 0
        ? (accountingPeriods[accountingPeriods.length - 1] ?? null)
        : null,
    );
  const [date, setDate] = useState<Dayjs | null>(null);
  const defaultDate =
    accountingPeriod !== null
      ? dayjs(`${accountingPeriod.year}-${accountingPeriod.month}-01`)
      : null;
  const [location, setLocation] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [amount, setAmount] = useState<number | null>(null);

  const [debitAccount, setDebitAccount] = useState<Account | null>(null);
  const [creditAccount, setCreditAccount] = useState<Account | null>(null);
  const [debitFund, setDebitFund] = useState<Fund | null>(null);
  const [creditFund, setCreditFund] = useState<Fund | null>(null);

  const [incomeFundAssignments, setIncomeFundAssignments] = useState<
    FundAmount[]
  >([]);

  const [spendingFundAssignments, setSpendingFundAssignments] = useState<
    FundAmount[]
  >([]);

  /**
   * Event handler for when the from or to fields are changed in the create transaction form.
   */
  const onToFromChange = function (
    newDebitAccount: Account | null,
    newCreditAccount: Account | null,
    newDebitFund: Fund | null,
    newCreditFund: Fund | null,
  ): void {
    if (
      !isIncomeTransaction(
        newDebitAccount,
        newCreditAccount,
        newDebitFund,
        newCreditFund,
      )
    ) {
      setIncomeFundAssignments([]);
    } else {
      setIncomeFundAssignments(
        updateUnassignedFundAmount(
          unassignedFund,
          amount,
          incomeFundAssignments,
        ),
      );
    }
    if (
      !isSpendingTransaction(
        newDebitAccount,
        newCreditAccount,
        newDebitFund,
        newCreditFund,
      )
    ) {
      setSpendingFundAssignments([]);
    } else {
      setSpendingFundAssignments(
        updateUnassignedFundAmount(
          unassignedFund,
          amount,
          spendingFundAssignments,
        ),
      );
    }
  };

  /**
   * Event handler for when the debit account or fund fields are changed in the create transaction form.
   */
  const onDebitFromChange = function (
    newDebitAccount: Account | null,
    newDebitFund: Fund | null,
  ): void {
    setDebitAccount(newDebitAccount);
    setDebitFund(newDebitFund);
    onToFromChange(newDebitAccount, creditAccount, newDebitFund, creditFund);
  };

  /**
   * Event handler for when the credit account or fund fields are changed in the create transaction form.
   */
  const onCreditToChange = function (
    newCreditAccount: Account | null,
    newCreditFund: Fund | null,
  ): void {
    setCreditAccount(newCreditAccount);
    setCreditFund(newCreditFund);
    onToFromChange(debitAccount, newCreditAccount, debitFund, newCreditFund);
  };

  /**
   * Event handler for when the amount field is changed in the create transaction form.
   */
  const onAmountChange = function (newAmount: number | null): void {
    setAmount(newAmount);

    setIncomeFundAssignments(
      updateUnassignedFundAmount(
        unassignedFund,
        newAmount,
        incomeFundAssignments,
      ),
    );
    setSpendingFundAssignments(
      updateUnassignedFundAmount(
        unassignedFund,
        newAmount,
        spendingFundAssignments,
      ),
    );
  };

  let request: CreateTransactionRequest | null = null;
  if (
    accountingPeriod !== null &&
    (date !== null || defaultDate !== null) &&
    location !== "" &&
    description !== "" &&
    amount !== null &&
    amount > 0
  ) {
    if (
      isIncomeTransaction(debitAccount, creditAccount, debitFund, creditFund) &&
      isIncomeTransactionComplete(incomeFundAssignments)
    ) {
      request = {
        type: CreateIncomeTransactionType.Income,
        accountingPeriodId: accountingPeriod.id,
        date:
          date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
        location,
        description,
        amount,
        debitAccount:
          debitAccount !== null
            ? {
                accountId: debitAccount.id,
              }
            : null,
        creditAccount: {
          accountId: creditAccount?.id ?? "",
        },
        fundAssignments: incomeFundAssignments
          .filter((fundAmount) => fundAmount.fundName !== "Unassigned")
          .map((fundAmount) => ({
            fundId: fundAmount.fundId,
            amount: fundAmount.amount,
          })),
      };
    } else if (
      isSpendingTransaction(
        debitAccount,
        creditAccount,
        debitFund,
        creditFund,
      ) &&
      isSpendingTransactionComplete(spendingFundAssignments)
    ) {
      request = {
        type: CreateSpendingTransactionType.Spending,
        accountingPeriodId: accountingPeriod.id,
        date:
          date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
        location,
        description,
        amount,
        debitAccount: {
          accountId: debitAccount?.id ?? "",
        },
        creditAccount:
          creditAccount !== null
            ? {
                accountId: creditAccount.id,
              }
            : null,
        fundAssignments: spendingFundAssignments
          .filter((fundAmount) => fundAmount.fundName !== "Unassigned")
          .map((fundAmount) => ({
            fundId: fundAmount.fundId,
            amount: fundAmount.amount,
          })),
      };
    } else if (
      isAccountTransaction(debitAccount, creditAccount, debitFund, creditFund)
    ) {
      request = {
        type: CreateAccountTransactionType.Account,
        accountingPeriodId: accountingPeriod.id,
        date:
          date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
        location,
        description,
        amount,
        debitAccount:
          debitAccount !== null
            ? {
                accountId: debitAccount.id,
              }
            : null,
        creditAccount:
          creditAccount !== null
            ? {
                accountId: creditAccount.id,
              }
            : null,
      };
    } else if (
      isFundTransaction(debitAccount, creditAccount, debitFund, creditFund) &&
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
  }

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
    setDebitFund(null);
    setCreditFund(null);
    setIncomeFundAssignments([]);
    setSpendingFundAssignments([]);
  };

  useEffect(() => {
    if (state.success === true) {
      reset();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state]);

  return (
    <Stack spacing={2}>
      <Stack spacing={2} sx={{ maxWidth: "600px" }}>
        <CreateOrUpdateTransactionDetailsFrame
          accountingPeriods={accountingPeriods}
          accountingPeriod={accountingPeriod}
          setAccountingPeriod={setAccountingPeriod}
          date={date ?? defaultDate}
          setDate={setDate}
          location={location}
          setLocation={setLocation}
          description={description}
          setDescription={setDescription}
          amount={amount}
          setAmount={onAmountChange}
        />
        <CreateOrUpdateTransactionFromToFrame
          accounts={accounts}
          debitAccount={debitAccount}
          creditAccount={creditAccount}
          funds={funds}
          debitFund={debitFund}
          creditFund={creditFund}
          setDebitFrom={(newDebitAccount, newDebitFund): void => {
            onDebitFromChange(newDebitAccount, newDebitFund);
          }}
          setCreditTo={(newCreditAccount, newCreditFund): void => {
            onCreditToChange(newCreditAccount, newCreditFund);
          }}
        />
        {isIncomeTransaction(
          debitAccount,
          creditAccount,
          debitFund,
          creditFund,
        ) && (
          <CreateOrUpdateIncomeTransactionFrame
            funds={funds}
            amount={amount}
            incomeFundAssignments={incomeFundAssignments}
            setIncomeFundAssignments={setIncomeFundAssignments}
          />
        )}
        {isSpendingTransaction(
          debitAccount,
          creditAccount,
          debitFund,
          creditFund,
        ) && (
          <CreateOrUpdateSpendingTransactionFrame
            funds={funds}
            amount={amount}
            spendingFundAssignments={spendingFundAssignments}
            setSpendingFundAssignments={setSpendingFundAssignments}
          />
        )}
        <DialogActions>
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
            Create
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

export default CreateTransactionForm;
