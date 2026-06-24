"use client";

import { type Account, isTrackedAccountType } from "@/accounts/types";
import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import { Button, Stack, Typography } from "@mui/material";
import type { Fund, FundAmount } from "@/funds/types";
import {
  type IncomeTransaction,
  type Transaction,
  UpdateIncomeTransactionType,
  type UpdateTransactionRequest,
  asIncomeTransaction,
  isIncomeTransactionComplete,
} from "@/transactions/transaction";
import IncomeTransactionSourceFrame, {
  type IncomeAmountItemDraft,
  createEmptyAmountItem,
} from "@/transactions/workspace/income/IncomeTransactionSourceFrame";
import {
  type JSX,
  startTransition,
  useActionState,
  useCallback,
  useEffect,
  useRef,
  useState,
} from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { AddCircleOutline } from "@mui/icons-material";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import IncomeTransactionDestinationFrame from "@/transactions/workspace/income/IncomeTransactionDestinationFrame";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import updateTransaction from "@/transactions/workspace/updateTransaction";
import { updateUnassignedFundAmount } from "@/funds/fundAssignment";
import { useRouter } from "next/navigation";

interface UpdateIncomeTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
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
  readonly baselineFundAssignments: FundAmount[];
}

type IncomeTransactionDestinationModel =
  IncomeTransaction["destinations"][number];

const createEmptyDestination = function (): IncomeDestinationDraft {
  return {
    account: null,
    amount: null,
    fundAssignments: [],
    baselineFundAssignments: [],
  };
};

const formatTotal = function (value: number): string {
  return value.toLocaleString([], {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
};

/**
 * Displays the dedicated update form for income transactions.
 */
const UpdateIncomeTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  accounts,
  funds,
  assignmentGoals,
  spendingGoals,
  redirectUrl,
}: UpdateIncomeTransactionFormProps): JSX.Element {
  const unassignedFund =
    funds.find((fund) => fund.name === "Unassigned") ?? null;
  const formRef = useRef<HTMLDivElement | null>(null);
  const incomeTransaction: IncomeTransaction | null =
    asIncomeTransaction(transaction);

  const getAccountById = useCallback(
    (accountId: string): Account | null =>
      accounts.find((account) => account.id === accountId) ?? null,
    [accounts],
  );

  const buildSourceAccountFromTransaction = useCallback((): Account | null => {
    if (
      // eslint-disable-next-line @typescript-eslint/prefer-optional-chain
      incomeTransaction === null ||
      incomeTransaction.source.account === null ||
      typeof incomeTransaction.source.account === "undefined"
    ) {
      return null;
    }
    return getAccountById(incomeTransaction.source.account.accountId);
  }, [incomeTransaction, getAccountById]);

  const buildIncomeLinesFromTransaction =
    useCallback((): IncomeAmountItemDraft[] => {
      if (incomeTransaction === null) {
        return [createEmptyAmountItem()];
      }
      return incomeTransaction.source.incomeLines.length > 0
        ? incomeTransaction.source.incomeLines.map((line) => ({
            description: line.description,
            amount: line.amount,
          }))
        : [createEmptyAmountItem()];
    }, [incomeTransaction]);

  const buildIncomeDeductionsFromTransaction =
    useCallback((): IncomeAmountItemDraft[] => {
      if (incomeTransaction === null) {
        return [];
      }
      return incomeTransaction.source.incomeDeductions.map((deduction) => ({
        description: deduction.description,
        amount: deduction.amount,
      }));
    }, [incomeTransaction]);

  const buildDestinationDraft = useCallback(
    (
      destination: IncomeTransactionDestinationModel,
    ): IncomeDestinationDraft => {
      const baselineFundAssignments = destination.fundAssignments.map(
        (fundAssignment) => ({
          fundId: fundAssignment.fundId,
          fundName: fundAssignment.fundName,
          amount: fundAssignment.amount,
        }),
      );

      return {
        account: getAccountById(destination.account.accountId),
        amount: destination.amount,
        fundAssignments: updateUnassignedFundAmount(
          unassignedFund,
          destination.amount,
          baselineFundAssignments,
        ),
        baselineFundAssignments,
      };
    },
    [getAccountById, unassignedFund],
  );

  const buildDestinationsFromTransaction =
    useCallback((): IncomeDestinationDraft[] => {
      if (incomeTransaction === null) {
        return [createEmptyDestination()];
      }

      return incomeTransaction.destinations.map(buildDestinationDraft);
    }, [incomeTransaction, buildDestinationDraft]);

  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [sourceLocation, setSourceLocation] = useState<string>(
    incomeTransaction?.source.location ?? "",
  );
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [amount, setAmount] = useState<number | null>(transaction.amount);
  const [sourceAccount, setSourceAccount] = useState<Account | null>(
    buildSourceAccountFromTransaction(),
  );
  const [incomeLines, setIncomeLines] = useState<IncomeAmountItemDraft[]>(
    buildIncomeLinesFromTransaction(),
  );
  const [incomeDeductions, setIncomeDeductions] = useState<
    IncomeAmountItemDraft[]
  >(buildIncomeDeductionsFromTransaction());
  const [destinations, setDestinations] = useState<IncomeDestinationDraft[]>(
    buildDestinationsFromTransaction(),
  );
  const currentAssignmentGoals = assignmentGoals.filter(
    (goal) => goal.accountingPeriodId === transactionAccountingPeriod.id,
  );
  const currentSpendingGoals = spendingGoals.filter(
    (goal) => goal.accountingPeriodId === transactionAccountingPeriod.id,
  );

  const router = useRouter();
  const [state, action, pending] = useActionState(updateTransaction, {});

  const reset = function (): void {
    setDate(dayjs(transaction.date));
    setSourceLocation(incomeTransaction?.source.location ?? "");
    setDescription(transaction.description);
    setAmount(transaction.amount);
    setSourceAccount(buildSourceAccountFromTransaction());
    setIncomeLines(buildIncomeLinesFromTransaction());
    setIncomeDeductions(buildIncomeDeductionsFromTransaction());
    setDestinations(buildDestinationsFromTransaction());
  };

  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

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

  let request: UpdateTransactionRequest | null = null;
  if (
    incomeTransaction !== null &&
    date !== null &&
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
      type: UpdateIncomeTransactionType.Income,
      date: date.format("YYYY-MM-DD"),
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
        <TransactionSection
          title="Income Flow"
          description="Edit the source and each tracked destination. The net source amount and destination amounts should both add up to the transaction amount."
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
                baselineFundAssignments={destination.baselineFundAssignments}
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
