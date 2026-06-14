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
  UpdateSpendingTransactionType,
  type UpdateTransactionRequest,
  isSpendingTransactionComplete,
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

interface UpdateSpendingTransactionFormProps {
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
 * Displays the dedicated update form for spending transactions.
 */
const UpdateSpendingTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  transactionDebitAccount,
  transactionCreditAccount,
  funds,
  assignmentGoals,
  spendingGoals,
  redirectUrl,
}: UpdateSpendingTransactionFormProps): JSX.Element {
  const unassignedFund =
    funds.find((fund) => fund.name === "Unassigned") ?? null;
  const formRef = useRef<HTMLDivElement | null>(null);
  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [location, setLocation] = useState<string>(transaction.location);
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [amount, setAmount] = useState<number | null>(transaction.amount);
  const [fundAssignments, setFundAssignments] = useState<FundAmount[]>(
    transaction.transactionType === TransactionType.Spending &&
      "fundAssignments" in transaction
      ? updateUnassignedFundAmount(
          unassignedFund,
          transaction.amount,
          transaction.fundAssignments,
        )
      : [],
  );
  const currentAssignmentGoal = assignmentGoals.filter(
    (goal) => goal.accountingPeriodId === transactionAccountingPeriod.id,
  );
  const currentSpendingGoal = spendingGoals.filter(
    (goal) => goal.accountingPeriodId === transactionAccountingPeriod.id,
  );

  const [state, action, pending] = useActionState(updateTransaction, {});

  const reset = function (): void {
    setDate(dayjs(transaction.date));
    setLocation(transaction.location);
    setDescription(transaction.description);
    setAmount(transaction.amount);
    setFundAssignments(
      transaction.transactionType === TransactionType.Spending &&
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

  let request: UpdateTransactionRequest | null = null;
  if (
    date !== null &&
    location !== "" &&
    description !== "" &&
    amount !== null &&
    amount > 0 &&
    isSpendingTransactionComplete(fundAssignments)
  ) {
    request = {
      type: UpdateSpendingTransactionType.Spending,
      date: date.format("YYYY-MM-DD"),
      location,
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
          location={location}
          setLocation={setLocation}
          descriptionValue={description}
          setDescriptionValue={setDescription}
          amount={amount}
          setAmount={onAmountChange}
        />
        <TransactionAccountPairSection
          title="Money Flow"
          description="Choose which tracked account is being charged and optionally the untracked destination account."
          accounts={[transactionDebitAccount, transactionCreditAccount].filter(
            (account): account is Account => account !== null,
          )}
          leftLabel="Spend From"
          rightLabel="Pay To"
          leftAccount={transactionDebitAccount}
          rightAccount={transactionCreditAccount}
          setLeftAccount={null}
          setRightAccount={null}
        />
        <FundAssignmentPlanner
          title="Fund Allocation"
          tone="spending"
          funds={funds}
          assignmentGoals={currentAssignmentGoal}
          spendingGoals={currentSpendingGoal}
          totalAmountToAssign={amount}
          baselineValue={
            transaction.transactionType === TransactionType.Spending &&
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
            Update Spending Transaction
          </Button>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default UpdateSpendingTransactionForm;
