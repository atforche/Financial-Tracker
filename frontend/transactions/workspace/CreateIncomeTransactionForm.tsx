"use client";

import { type Account, isTrackedAccountType } from "@/accounts/types";
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

interface CreateIncomeTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated create form for income transactions.
 */
const CreateIncomeTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  redirectUrl,
}: CreateIncomeTransactionFormProps): JSX.Element {
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
  const [location, setLocation] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [amount, setAmount] = useState<number | null>(null);
  const [sourceAccount, setSourceAccount] = useState<Account | null>(null);
  const [depositAccount, setDepositAccount] = useState<Account | null>(null);
  const [fundAssignments, setFundAssignments] = useState<FundAmount[]>([]);

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
    setSourceAccount(null);
    setDepositAccount(null);
    setFundAssignments([]);
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

  let request: CreateTransactionRequest | null = null;
  if (
    accountingPeriod !== null &&
    (date !== null || defaultDate !== null) &&
    location !== "" &&
    description !== "" &&
    amount !== null &&
    amount > 0 &&
    depositAccount !== null &&
    isIncomeTransactionComplete(fundAssignments)
  ) {
    request = {
      type: CreateIncomeTransactionType.Income,
      accountingPeriodId: accountingPeriod.id,
      date:
        date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
      location,
      description,
      amount,
      debitAccountId: sourceAccount?.id ?? null,
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
          location={location}
          setLocation={setLocation}
          descriptionValue={description}
          setDescriptionValue={setDescription}
          amount={amount}
          setAmount={onAmountChange}
        />
        <TransactionAccountPairSection
          title="Money Flow"
          description="Choose which tracked account receives the income and optionally the untracked account where the money came from."
          accounts={accounts}
          leftLabel="Source Account"
          rightLabel="Deposit To"
          leftAccount={sourceAccount}
          rightAccount={depositAccount}
          setLeftAccount={setSourceAccount}
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
          totalAmountToAssign={amount}
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
