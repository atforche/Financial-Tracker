"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import type { Fund, FundAmount } from "@/funds/types";
import { type JSX, startTransition, useActionState, useState } from "react";
import {
  type Transaction,
  TransactionType,
  UpdateAccountTransactionType,
  UpdateFundTransactionType,
  UpdateIncomeTransactionType,
  UpdateSpendingTransactionType,
  type UpdateTransactionRequest,
  isIncomeTransactionComplete,
  isSpendingTransactionComplete,
} from "@/transactions/types";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateOrUpdateIncomeTransactionFrame from "@/transactions/workspace/CreateOrUpdateIncomeTransactionFrame";
import CreateOrUpdateSpendingTransactionFrame from "@/transactions/workspace/CreateOrUpdateSpendingTransactionFrame";
import CreateOrUpdateTransactionDetailsFrame from "@/transactions/workspace/CreateOrUpdateTransactionDetailsFrame";
import CreateOrUpdateTransactionFromToFrame from "@/transactions/workspace/CreateOrUpdateTransactionFromToFrame";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import updateTransaction from "@/transactions/workspace/updateTransaction";
import { updateUnassignedFundAmount } from "@/funds/FundAssignmentEntryFrame";

/**
 * Props for the UpdateTransactionForm component.
 */
interface UpdateTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly transactionDebitAccount: Account | null;
  readonly transactionCreditAccount: Account | null;
  readonly transactionDebitFund: Fund | null;
  readonly transactionCreditFund: Fund | null;
  readonly funds: Fund[];
  readonly redirectUrl: string;
}

/**
 * Component that displays the form for updating a transaction.
 */
const UpdateTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  transactionDebitAccount,
  transactionCreditAccount,
  transactionDebitFund,
  transactionCreditFund,
  funds,
  redirectUrl,
}: UpdateTransactionFormProps): JSX.Element {
  const unassignedFund =
    funds.find((fund) => fund.name === "Unassigned") ?? null;

  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [location, setLocation] = useState<string>(transaction.location);
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [amount, setAmount] = useState<number | null>(transaction.amount);

  const [incomeFundAssignments, setIncomeFundAssignments] = useState<
    FundAmount[]
  >(
    transaction.transactionType === TransactionType.Income &&
      "fundAssignments" in transaction
      ? updateUnassignedFundAmount(
          unassignedFund,
          transaction.amount,
          transaction.fundAssignments,
        )
      : [],
  );

  const [spendingFundAssignments, setSpendingFundAssignments] = useState<
    FundAmount[]
  >(
    transaction.transactionType === TransactionType.Spending &&
      "fundAssignments" in transaction
      ? updateUnassignedFundAmount(
          unassignedFund,
          transaction.amount,
          transaction.fundAssignments,
        )
      : [],
  );

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

  let request: UpdateTransactionRequest | null = null;
  if (
    date !== null &&
    location !== "" &&
    description !== "" &&
    amount !== null &&
    amount > 0
  ) {
    if (
      transaction.transactionType === TransactionType.Income &&
      isIncomeTransactionComplete(incomeFundAssignments)
    ) {
      request = {
        type: UpdateIncomeTransactionType.Income,
        date: date.format("YYYY-MM-DD"),
        location,
        description,
        amount,
        fundAssignments: incomeFundAssignments
          .filter((fundAmount) => fundAmount.fundName !== "Unassigned")
          .map((fundAmount) => ({
            fundId: fundAmount.fundId,
            amount: fundAmount.amount,
          })),
      };
    } else if (
      transaction.transactionType === TransactionType.Spending &&
      isSpendingTransactionComplete(spendingFundAssignments)
    ) {
      request = {
        type: UpdateSpendingTransactionType.Spending,
        date: date.format("YYYY-MM-DD"),
        location,
        description,
        amount,
        fundAssignments: spendingFundAssignments
          .filter((fundAmount) => fundAmount.fundName !== "Unassigned")
          .map((fundAmount) => ({
            fundId: fundAmount.fundId,
            amount: fundAmount.amount,
          })),
      };
    } else if (transaction.transactionType === TransactionType.Account) {
      request = {
        type: UpdateAccountTransactionType.Account,
        date: date.format("YYYY-MM-DD"),
        location,
        description,
        amount,
      };
    } else if (transaction.transactionType === TransactionType.Fund) {
      request = {
        type: UpdateFundTransactionType.Fund,
        date: date.format("YYYY-MM-DD"),
        location,
        description,
        amount,
      };
    }
  }

  const [state, action, pending] = useActionState(updateTransaction, {});

  const reset = function (): void {
    setDate(dayjs(transaction.date));
    setLocation(transaction.location);
    setDescription(transaction.description);
    setAmount(transaction.amount);
    if (transaction.transactionType === TransactionType.Income) {
      setIncomeFundAssignments(
        "fundAssignments" in transaction
          ? updateUnassignedFundAmount(
              unassignedFund,
              transaction.amount,
              transaction.fundAssignments,
            )
          : [],
      );
    } else if (transaction.transactionType === TransactionType.Spending) {
      setSpendingFundAssignments(
        "fundAssignments" in transaction
          ? updateUnassignedFundAmount(
              unassignedFund,
              transaction.amount,
              transaction.fundAssignments,
            )
          : [],
      );
    }
  };

  return (
    <Stack spacing={2}>
      <Stack spacing={2} sx={{ maxWidth: "600px" }}>
        <CreateOrUpdateTransactionDetailsFrame
          accountingPeriods={[]}
          accountingPeriod={transactionAccountingPeriod}
          setAccountingPeriod={null}
          date={date}
          setDate={setDate}
          location={location}
          setLocation={setLocation}
          description={description}
          setDescription={setDescription}
          amount={amount}
          setAmount={onAmountChange}
        />
        <CreateOrUpdateTransactionFromToFrame
          accounts={[]}
          debitAccount={transactionDebitAccount}
          creditAccount={transactionCreditAccount}
          funds={funds}
          debitFund={transactionDebitFund}
          creditFund={transactionCreditFund}
          setDebitFrom={null}
          setCreditTo={null}
        />
        {transaction.transactionType === TransactionType.Income && (
          <CreateOrUpdateIncomeTransactionFrame
            funds={funds}
            amount={amount}
            incomeFundAssignments={incomeFundAssignments}
            setIncomeFundAssignments={setIncomeFundAssignments}
          />
        )}
        {transaction.transactionType === TransactionType.Spending && (
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
                action({ transactionId: transaction.id, redirectUrl, request });
              });
            }}
          >
            Update
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

export default UpdateTransactionForm;
