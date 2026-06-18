"use client";

import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import { Button, Stack } from "@mui/material";
import type { Fund, FundAmount } from "@/funds/types";
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
  TransactionType,
  UpdateIncomeTransactionType,
  type UpdateTransactionRequest,
  isIncomeTransactionComplete,
} from "@/transactions/types";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FundAssignmentPlanner from "@/funds/FundAssignmentPlanner";
import TransactionAccountPairSection from "@/transactions/workspace/TransactionAccountPairSection";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import updateTransaction from "@/transactions/workspace/updateTransaction";
import { updateUnassignedFundAmount } from "@/funds/fundAssignment";

interface UpdateIncomeTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly transactionDebitAccount: Account | null;
  readonly transactionCreditAccount: Account | null;
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated update form for income transactions.
 */
const UpdateIncomeTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  transactionDebitAccount,
  transactionCreditAccount,
  funds,
  assignmentGoals,
  spendingGoals,
  redirectUrl,
}: UpdateIncomeTransactionFormProps): JSX.Element {
  const unassignedFund =
    funds.find((fund) => fund.name === "Unassigned") ?? null;
  const formRef = useRef<HTMLDivElement | null>(null);

  const sourceLocation = transaction.transactionType === TransactionType.Income &&
      "sourceLocation" in transaction
      ? (transaction.sourceLocation ?? "")
      : "";

  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [amount, setAmount] = useState<number | null>(transaction.amount);
  const [fundAssignments, setFundAssignments] = useState<FundAmount[]>(
    transaction.transactionType === TransactionType.Income &&
      "fundAssignments" in transaction
      ? updateUnassignedFundAmount(
          unassignedFund,
          transaction.amount,
          transaction.fundAssignments,
        )
      : [],
  );
  const currentAssignmentGoals = assignmentGoals.filter(
    (goal) => goal.accountingPeriodId === transactionAccountingPeriod.id,
  );
  const currentSpendingGoals = spendingGoals.filter(
    (goal) => goal.accountingPeriodId === transactionAccountingPeriod.id,
  );

  const [state, action, pending] = useActionState(updateTransaction, {});

  const reset = function (): void {
    setDate(dayjs(transaction.date));
    setDescription(transaction.description);
    setAmount(transaction.amount);
    setFundAssignments(
      transaction.transactionType === TransactionType.Income &&
        "fundAssignments" in transaction
        ? updateUnassignedFundAmount(
            unassignedFund,
            transaction.amount,
            transaction.fundAssignments,
          )
        : [],
    );
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (state.success === true) {
      reset();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state]);

  const onAmountChange = function (newAmount: number | null): void {
    setAmount(newAmount);
    setFundAssignments(
      updateUnassignedFundAmount(unassignedFund, newAmount, fundAssignments),
    );
  };

  const normalizedSourceLocation = sourceLocation.trim();

  let request: UpdateTransactionRequest | null = null;
  if (
    date !== null &&
    description !== "" &&
    amount !== null &&
    amount > 0 &&
    (transactionDebitAccount !== null || normalizedSourceLocation !== "") &&
    isIncomeTransactionComplete(fundAssignments)
  ) {
    request = {
      type: UpdateIncomeTransactionType.Income,
      date: date.format("YYYY-MM-DD"),
      description,
      amount,
      fundAssignments: fundAssignments
        .filter((fundAmount) => fundAmount.fundName !== "Unassigned")
        .map((fundAmount) => ({
          fundId: fundAmount.fundId,
          amount: fundAmount.amount,
        })),
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
          locationLabel={
            transactionDebitAccount === null ? "Source Location" : null
          }
          locationValue={sourceLocation}
          setLocationValue={null}
          descriptionValue={description}
          setDescriptionValue={setDescription}
          amount={amount}
          setAmount={onAmountChange}
        />
        <TransactionAccountPairSection
          title="Money Flow"
          description="Review the tracked deposit account and the original source for this income transaction."
          accounts={[transactionDebitAccount, transactionCreditAccount].filter(
            (account): account is Account => account !== null,
          )}
          leftLabel="Source Account"
          rightLabel="Deposit To"
          leftAccount={transactionDebitAccount}
          rightAccount={transactionCreditAccount}
          setLeftAccount={null}
          setRightAccount={null}
        />
        <FundAssignmentPlanner
          title="Fund Allocation"
          tone="income"
          funds={funds}
          assignmentGoals={currentAssignmentGoals}
          spendingGoals={currentSpendingGoals}
          totalAmountToAssign={amount}
          baselineValue={
            transaction.transactionType === TransactionType.Income &&
            "fundAssignments" in transaction
              ? transaction.fundAssignments
              : []
          }
          value={fundAssignments}
          setValue={setFundAssignments}
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
            Update Income Transaction
          </Button>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default UpdateIncomeTransactionForm;
