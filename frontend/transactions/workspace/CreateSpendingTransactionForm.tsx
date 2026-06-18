"use client";

import { type Account, isTrackedAccountType } from "@/accounts/types";
import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import { Button, Stack } from "@mui/material";
import {
  CreateSpendingTransactionType,
  type CreateTransactionRequest,
  isSpendingTransactionComplete,
} from "@/transactions/types";
import type { Fund, FundAmount } from "@/funds/types";
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
import FundAssignmentPlanner from "@/funds/FundAssignmentPlanner";
import TransactionAccountPairSection from "@/transactions/workspace/TransactionAccountPairSection";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import createTransaction from "@/transactions/workspace/createTransaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import { updateUnassignedFundAmount } from "@/funds/fundAssignment";
import { useRouter } from "next/navigation";

interface CreateSpendingTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated create form for spending transactions.
 */
const CreateSpendingTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  assignmentGoals,
  spendingGoals,
  redirectUrl,
}: CreateSpendingTransactionFormProps): JSX.Element {
  const router = useRouter();
  const unassignedFund =
    funds.find((fund) => fund.name === "Unassigned") ?? null;
  const formRef = useRef<HTMLDivElement | null>(null);

  const getAccountById = function (accountId: string): Account | null {
    return accounts.find((account) => account.id === accountId) ?? null;
  };

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
  const [destinationLocation, setDestinationLocation] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [amount, setAmount] = useState<number | null>(null);
  const [paymentAccount, setPaymentAccount] = useState<Account | null>(null);
  const [destinationAccount, setDestinationAccount] = useState<Account | null>(
    null,
  );
  const [fundAssignments, setFundAssignments] = useState<FundAmount[]>([]);
  const currentAssignmentGoals = assignmentGoals.filter(
    (goal) => goal.accountingPeriodId === accountingPeriod?.id,
  );
  const currentSpendingGoals = spendingGoals.filter(
    (goal) => goal.accountingPeriodId === accountingPeriod?.id,
  );

  const [state, action, pending] = useActionState(createTransaction, {});

  const reset = function (): void {
    setAccountingPeriod(
      accountingPeriods.length > 0
        ? (accountingPeriods[accountingPeriods.length - 1] ?? null)
        : null,
    );
    setDate(null);
    setDestinationLocation("");
    setDescription("");
    setAmount(null);
    setPaymentAccount(null);
    setDestinationAccount(null);
    setFundAssignments([]);
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (state.success === true && state.transactionId !== null) {
      const [pathname, search = ""] = redirectUrl.split("?");
      const params = new URLSearchParams(search);
      params.set("selectedTransactionId", state.transactionId ?? "");
      params.set("action", "post");
      const nextUrl = `${pathname}?${params.toString()}`;
      router.replace(nextUrl, { scroll: false });
      reset();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [redirectUrl, router, state]);

  const onAmountChange = function (newAmount: number | null): void {
    setAmount(newAmount);
    setFundAssignments(
      updateUnassignedFundAmount(unassignedFund, newAmount, fundAssignments),
    );
  };

  const onDestinationAccountChange = function (account: Account | null): void {
    setDestinationAccount(account);
    if (account !== null) {
      setDestinationLocation("");
    }
  };

  const normalizedDestinationLocation = destinationLocation.trim();

  let request: CreateTransactionRequest | null = null;
  if (
    accountingPeriod !== null &&
    (date !== null || defaultDate !== null) &&
    description !== "" &&
    amount !== null &&
    amount > 0 &&
    paymentAccount !== null &&
    (destinationAccount !== null || normalizedDestinationLocation !== "") &&
    isSpendingTransactionComplete(fundAssignments)
  ) {
    request = {
      type: CreateSpendingTransactionType.Spending,
      accountingPeriodId: accountingPeriod.id,
      date:
        date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
      description,
      amount,
      debitAccountId: paymentAccount.id,
      creditAccountId: destinationAccount?.id ?? null,
      destinationLocation:
        destinationAccount === null
          ? normalizedDestinationLocation || null
          : null,
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
          accountingPeriods={accountingPeriods}
          accountingPeriod={accountingPeriod}
          setAccountingPeriod={setAccountingPeriod}
          date={date ?? defaultDate}
          setDate={setDate}
          locationLabel={
            destinationAccount === null ? "Destination Location" : null
          }
          locationValue={destinationLocation}
          setLocationValue={
            destinationAccount === null ? setDestinationLocation : null
          }
          descriptionValue={description}
          setDescriptionValue={setDescription}
          amount={amount}
          setAmount={onAmountChange}
        />
        <TransactionAccountPairSection
          title="Money Flow"
          description="Choose the tracked account being charged and either the untracked destination account or a destination location."
          accounts={accounts}
          leftLabel="Spend From"
          rightLabel="Pay To"
          leftAccount={paymentAccount}
          rightAccount={destinationAccount}
          setLeftAccount={setPaymentAccount}
          setRightAccount={onDestinationAccountChange}
          leftFilter={(account) => {
            const selectedAccount = getAccountById(account.id);
            return (
              selectedAccount !== null &&
              isTrackedAccountType(selectedAccount.type) &&
              account.id !== destinationAccount?.id
            );
          }}
          rightFilter={(account) => {
            const selectedAccount = getAccountById(account.id);
            return (
              selectedAccount !== null &&
              !isTrackedAccountType(selectedAccount.type) &&
              account.id !== paymentAccount?.id
            );
          }}
        />
        <FundAssignmentPlanner
          title="Fund Allocation"
          tone="spending"
          funds={funds}
          assignmentGoals={currentAssignmentGoals}
          spendingGoals={currentSpendingGoals}
          totalAmountToAssign={amount}
          baselineValue={[]}
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
                action({ redirectUrl, request });
              });
            }}
          >
            Create Spending Transaction
          </Button>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default CreateSpendingTransactionForm;
