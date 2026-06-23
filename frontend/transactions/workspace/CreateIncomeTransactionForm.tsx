"use client";

import { type Account, isTrackedAccountType } from "@/accounts/types";
import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import { Button, Stack, Typography } from "@mui/material";
import {
  CreateIncomeTransactionType,
  type CreateTransactionRequest,
  isIncomeTransactionComplete,
} from "@/transactions/types";
import type { Fund, FundAmount } from "@/funds/types";
import IncomeTransactionSourceFrame, {
  type IncomeAmountItemDraft,
  createEmptyAmountItem,
} from "@/transactions/workspace/IncomeTransactionSourceFrame";
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
import { AddCircleOutline } from "@mui/icons-material";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import IncomeTransactionDestinationFrame from "@/transactions/workspace/IncomeTransactionDestinationFrame";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import TransactionSection from "@/transactions/workspace/TransactionSection";
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

interface IncomeDestinationDraft {
  readonly account: Account | null;
  readonly amount: number | null;
  readonly fundAssignments: FundAmount[];
}

const createEmptyDestination = function (): IncomeDestinationDraft {
  return {
    account: null,
    amount: null,
    fundAssignments: [],
  };
};

const formatTotal = function (value: number): string {
  return value.toLocaleString([], {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
};

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
  const [incomeLines, setIncomeLines] = useState<IncomeAmountItemDraft[]>([
    createEmptyAmountItem(),
  ]);
  const [incomeDeductions, setIncomeDeductions] = useState<
    IncomeAmountItemDraft[]
  >([]);
  const [destinations, setDestinations] = useState<IncomeDestinationDraft[]>([
    createEmptyDestination(),
  ]);
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
    setIncomeLines([createEmptyAmountItem()]);
    setIncomeDeductions([]);
    setDestinations([createEmptyDestination()]);
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
      setIncomeLines([createEmptyAmountItem()]);
      setIncomeDeductions([]);
      setDestinations([createEmptyDestination()]);
      focusFirstEntryControl(formRef.current);
    }
  }, [accountingPeriods, redirectUrl, router, state]);

  const onSourceAccountChange = function (account: Account | null): void {
    setSourceAccount(account);
    if (account !== null) {
      setSourceLocation("");
    }
  };

  const updateDestination = function (
    index: number,
    recipe: (current: IncomeDestinationDraft) => IncomeDestinationDraft,
  ): void {
    setDestinations((currentDestinations) =>
      currentDestinations.map((currentDestination, currentIndex) =>
        currentIndex === index
          ? recipe(currentDestination)
          : currentDestination,
      ),
    );
  };

  const syncDestinationFundAssignments = function (
    destinationAmount: number | null,
    fundAssignments: FundAmount[],
  ): FundAmount[] {
    return updateUnassignedFundAmount(
      unassignedFund,
      destinationAmount,
      fundAssignments,
    );
  };

  const normalizedSourceLocation = sourceLocation.trim();
  const sourceNetAmount =
    incomeLines.reduce((total, line) => total + (line.amount ?? 0), 0) -
    incomeDeductions.reduce(
      (total, deduction) => total + (deduction.amount ?? 0),
      0,
    );
  const destinationTotal = destinations.reduce(
    (total, destination) => total + (destination.amount ?? 0),
    0,
  );
  const areIncomeLinesComplete =
    incomeLines.length > 0 &&
    incomeLines.every(
      (line) =>
        line.description.trim() !== "" &&
        line.amount !== null &&
        line.amount > 0,
    );
  const areIncomeDeductionsComplete = incomeDeductions.every(
    (deduction) =>
      deduction.description.trim() !== "" &&
      deduction.amount !== null &&
      deduction.amount > 0,
  );
  const areDestinationsComplete = destinations.every(
    (destination) =>
      destination.account !== null &&
      destination.amount !== null &&
      destination.amount > 0 &&
      isIncomeTransactionComplete(destination.fundAssignments),
  );

  let request: CreateTransactionRequest | null = null;
  if (
    accountingPeriod !== null &&
    (date !== null || defaultDate !== null) &&
    description !== "" &&
    amount !== null &&
    amount > 0 &&
    (sourceAccount !== null || normalizedSourceLocation !== "") &&
    destinations.length > 0 &&
    sourceNetAmount === amount &&
    destinationTotal === amount &&
    areIncomeLinesComplete &&
    areIncomeDeductionsComplete &&
    areDestinationsComplete
  ) {
    request = {
      type: CreateIncomeTransactionType.Income,
      accountingPeriodId: accountingPeriod.id,
      date:
        date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
      description,
      amount,
      source: {
        accountId: sourceAccount?.id ?? null,
        location:
          sourceAccount === null ? normalizedSourceLocation || null : null,
        incomeLines: incomeLines.map((line) => ({
          description: line.description.trim(),
          amount: line.amount ?? 0,
        })),
        incomeDeductions: incomeDeductions.map((deduction) => ({
          description: deduction.description.trim(),
          amount: deduction.amount ?? 0,
        })),
      },
      destinations: destinations.map((destination) => ({
        accountId: destination.account?.id ?? "",
        amount: destination.amount ?? 0,
        fundAssignments: destination.fundAssignments
          .filter((fundAmount) => fundAmount.fundName !== "Unassigned")
          .map((fundAmount) => ({
            fundId: fundAmount.fundId,
            amount: fundAmount.amount,
          })),
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
          descriptionValue={description}
          setDescriptionValue={setDescription}
          amount={amount}
          setAmount={setAmount}
        />
        <TransactionSection
          title="Income Flow"
          description="Build one income source and one or more tracked destinations. The net source amount and destination amounts should both add up to the transaction amount."
        >
          <Stack spacing={2}>
            <IncomeTransactionSourceFrame
              accounts={accounts}
              account={sourceAccount}
              setAccount={onSourceAccountChange}
              location={sourceLocation}
              setLocation={setSourceLocation}
              incomeLines={incomeLines}
              setIncomeLines={setIncomeLines}
              incomeDeductions={incomeDeductions}
              setIncomeDeductions={setIncomeDeductions}
              filter={(account) => {
                const selectedAccount = getAccountById(account.id);
                return (
                  selectedAccount !== null &&
                  !isTrackedAccountType(selectedAccount.type)
                );
              }}
            />
            {destinations.map((destination, index) => (
              <IncomeTransactionDestinationFrame
                key={`income-destination-${index}`}
                index={index}
                accounts={accounts}
                funds={funds}
                assignmentGoals={currentAssignmentGoals}
                spendingGoals={currentSpendingGoals}
                account={destination.account}
                setAccount={(account) => {
                  updateDestination(index, (currentDestination) => ({
                    ...currentDestination,
                    account,
                  }));
                }}
                amount={destination.amount}
                setAmount={(nextAmount) => {
                  updateDestination(index, (currentDestination) => ({
                    ...currentDestination,
                    amount: nextAmount,
                    fundAssignments: syncDestinationFundAssignments(
                      nextAmount,
                      currentDestination.fundAssignments,
                    ),
                  }));
                }}
                fundAssignments={destination.fundAssignments}
                setFundAssignments={(fundAssignments) => {
                  updateDestination(index, (currentDestination) => ({
                    ...currentDestination,
                    fundAssignments,
                  }));
                }}
                filter={(account) => {
                  const selectedAccount = getAccountById(account.id);
                  const accountUsedElsewhere = destinations.some(
                    (currentDestination, currentIndex) =>
                      currentIndex !== index &&
                      currentDestination.account?.id === account.id,
                  );
                  return (
                    selectedAccount !== null &&
                    isTrackedAccountType(selectedAccount.type) &&
                    !accountUsedElsewhere
                  );
                }}
                onRemove={
                  destinations.length > 1
                    ? (): void => {
                        setDestinations((currentDestinations) =>
                          currentDestinations.filter(
                            (_, currentIndex) => currentIndex !== index,
                          ),
                        );
                      }
                    : null
                }
              />
            ))}
            <Button
              variant="outlined"
              startIcon={<AddCircleOutline />}
              onClick={() => {
                setDestinations((currentDestinations) => [
                  ...currentDestinations,
                  createEmptyDestination(),
                ]);
              }}
              sx={{ alignSelf: "flex-start" }}
            >
              Add Destination
            </Button>
            <Typography
              variant="body2"
              color={
                amount !== null && sourceNetAmount !== amount
                  ? "error.main"
                  : "text.secondary"
              }
            >
              Source net total: ${formatTotal(sourceNetAmount)}
            </Typography>
            <Typography
              variant="body2"
              color={
                amount !== null && destinationTotal !== amount
                  ? "error.main"
                  : "text.secondary"
              }
            >
              Destination total: ${formatTotal(destinationTotal)}
            </Typography>
          </Stack>
        </TransactionSection>
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
