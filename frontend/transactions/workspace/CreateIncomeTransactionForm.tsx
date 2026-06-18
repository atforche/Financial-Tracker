"use client";

import { type Account, isTrackedAccountType } from "@/accounts/types";
import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import { Button, Stack } from "@mui/material";
import {
  CreateIncomeTransactionType,
  type CreateTransactionRequest,
  isIncomeTransactionComplete,
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

interface CreateIncomeTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated create form for income transactions.
 */
const CreateIncomeTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  assignmentGoals,
  spendingGoals,
  redirectUrl,
}: CreateIncomeTransactionFormProps): JSX.Element {
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
  const [sourceLocation, setSourceLocation] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [amount, setAmount] = useState<number | null>(null);
  const [sourceAccount, setSourceAccount] = useState<Account | null>(null);
  const [depositAccount, setDepositAccount] = useState<Account | null>(null);
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
    setSourceLocation("");
    setDescription("");
    setAmount(null);
    setSourceAccount(null);
    setDepositAccount(null);
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

  const onSourceAccountChange = function (account: Account | null): void {
    setSourceAccount(account);
    if (account !== null) {
      setSourceLocation("");
    }
  };

  const normalizedSourceLocation = sourceLocation.trim();

  let request: CreateTransactionRequest | null = null;
  if (
    accountingPeriod !== null &&
    (date !== null || defaultDate !== null) &&
    description !== "" &&
    amount !== null &&
    amount > 0 &&
    depositAccount !== null &&
    (sourceAccount !== null || normalizedSourceLocation !== "") &&
    isIncomeTransactionComplete(fundAssignments)
  ) {
    request = {
      type: CreateIncomeTransactionType.Income,
      accountingPeriodId: accountingPeriod.id,
      date:
        date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
      description,
      amount,
      debitAccountId: sourceAccount?.id ?? null,
      sourceLocation:
        sourceAccount === null ? normalizedSourceLocation || null : null,
      creditAccountId: depositAccount.id,
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
          locationLabel={sourceAccount === null ? "Source Location" : null}
          locationValue={sourceLocation}
          setLocationValue={sourceAccount === null ? setSourceLocation : null}
          descriptionValue={description}
          setDescriptionValue={setDescription}
          amount={amount}
          setAmount={onAmountChange}
        />
        <TransactionAccountPairSection
          title="Money Flow"
          description="Choose which tracked account receives the income and either the untracked source account or a source location."
          accounts={accounts}
          leftLabel="Source Account"
          rightLabel="Deposit To"
          leftAccount={sourceAccount}
          rightAccount={depositAccount}
          setLeftAccount={onSourceAccountChange}
          setRightAccount={setDepositAccount}
          leftFilter={(account) => {
            const selectedAccount = getAccountById(account.id);
            return (
              selectedAccount !== null &&
              !isTrackedAccountType(selectedAccount.type) &&
              account.id !== depositAccount?.id
            );
          }}
          rightFilter={(account) => {
            const selectedAccount = getAccountById(account.id);
            return (
              selectedAccount !== null &&
              isTrackedAccountType(selectedAccount.type) &&
              account.id !== sourceAccount?.id
            );
          }}
        />
        <FundAssignmentPlanner
          title="Fund Allocation"
          tone="income"
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
            Create Income Transaction
          </Button>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default CreateIncomeTransactionForm;
