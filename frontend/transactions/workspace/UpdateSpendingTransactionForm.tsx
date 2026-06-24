"use client";

import { type Account, isTrackedAccountType } from "@/accounts/types";
import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import { Button, Stack, Typography } from "@mui/material";
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
import type { AccountingPeriod } from "@/accounting-periods/types";
import { AddCircleOutline } from "@mui/icons-material";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import SpendingTransactionDestinationFrame from "@/transactions/workspace/SpendingTransactionDestinationFrame";
import SpendingTransactionSourceFrame from "@/transactions/workspace/SpendingTransactionSourceFrame";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import type { components } from "@/framework/data/api";
import updateTransaction from "@/transactions/workspace/updateTransaction";
import { updateUnassignedFundAmount } from "@/funds/fundAssignment";
import { useRouter } from "next/navigation";

interface UpdateSpendingTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
  readonly redirectUrl: string;
}

interface SpendingDestinationDraft {
  readonly account: Account | null;
  readonly location: string;
  readonly amount: number | null;
  readonly fundAssignments: FundAmount[];
  readonly baselineFundAssignments: FundAmount[];
}

type SpendingTransactionModel =
  components["schemas"]["TransactionModelSpendingTransactionModel"];

type SpendingTransactionDestinationModel =
  components["schemas"]["SpendingTransactionDestinationModel"];

const createEmptyDestination = function (): SpendingDestinationDraft {
  return {
    account: null,
    location: "",
    amount: null,
    fundAssignments: [],
    baselineFundAssignments: [],
  };
};

const formatDestinationTotal = function (value: number): string {
  return value.toLocaleString([], {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
};

/**
 * Displays the dedicated update form for spending transactions.
 */
const UpdateSpendingTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  accounts,
  funds,
  assignmentGoals,
  spendingGoals,
  redirectUrl,
}: UpdateSpendingTransactionFormProps): JSX.Element {
  const unassignedFund =
    funds.find((fund) => fund.name === "Unassigned") ?? null;
  const formRef = useRef<HTMLDivElement | null>(null);
  const spendingTransaction: SpendingTransactionModel | null =
    transaction.transactionType === TransactionType.Spending
      ? // eslint-disable-next-line @typescript-eslint/no-unsafe-type-assertion
        (transaction as SpendingTransactionModel)
      : null;

  const buildDestinationDraft = function (
    destination: SpendingTransactionDestinationModel,
  ): SpendingDestinationDraft {
    const baselineFundAssignments = destination.fundAssignments.map(
      (fundAssignment) => ({
        fundId: fundAssignment.fundId,
        fundName: fundAssignment.fundName,
        amount: fundAssignment.amount,
      }),
    );

    return {
      account:
        destination.account === null
          ? null
          : (accounts.find(
              (account) => account.id === destination.account?.accountId,
            ) ?? null),
      location: destination.location ?? "",
      amount: destination.amount,
      fundAssignments: updateUnassignedFundAmount(
        unassignedFund,
        destination.amount,
        baselineFundAssignments,
      ),
      baselineFundAssignments,
    };
  };

  const buildDestinationsFromTransaction =
    function (): SpendingDestinationDraft[] {
      if (spendingTransaction === null) {
        return [createEmptyDestination()];
      }

      return spendingTransaction.destinations.map(buildDestinationDraft);
    };

  const buildSourceAccountFromTransaction = function (): Account | null {
    if (spendingTransaction === null) {
      return null;
    }

    return (
      accounts.find(
        (account) =>
          account.id === spendingTransaction.source.account.accountId,
      ) ?? null
    );
  };

  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [amount, setAmount] = useState<number | null>(transaction.amount);
  const [sourceAccount, setSourceAccount] = useState<Account | null>(
    buildSourceAccountFromTransaction(),
  );
  const [destinations, setDestinations] = useState<SpendingDestinationDraft[]>(
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
    setDescription(transaction.description);
    setAmount(transaction.amount);
    setSourceAccount(buildSourceAccountFromTransaction());
    setDestinations(buildDestinationsFromTransaction());
  };

  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

  const updateDestination = function (
    index: number,
    recipe: (current: SpendingDestinationDraft) => SpendingDestinationDraft,
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

  const destinationTotal = destinations.reduce(
    (total, destination) => total + (destination.amount ?? 0),
    0,
  );

  const areDestinationsComplete = destinations.every((destination) => {
    const normalizedLocation = destination.location.trim();
    return (
      destination.amount !== null &&
      destination.amount > 0 &&
      (destination.account !== null || normalizedLocation !== "") &&
      isSpendingTransactionComplete(destination.fundAssignments)
    );
  });

  let request: UpdateTransactionRequest | null = null;
  if (
    spendingTransaction !== null &&
    date !== null &&
    description !== "" &&
    amount !== null &&
    amount > 0 &&
    sourceAccount !== null &&
    destinations.length > 0 &&
    destinationTotal === amount &&
    areDestinationsComplete
  ) {
    request = {
      type: UpdateSpendingTransactionType.Spending,
      date: date.format("YYYY-MM-DD"),
      description,
      amount,
      source: {
        accountId: sourceAccount.id,
      },
      destinations: destinations.map((destination) => ({
        accountId: destination.account?.id ?? null,
        location:
          destination.account === null
            ? destination.location.trim() || null
            : null,
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
          title="Spending Flow"
          description="Edit the spending source and each destination. The destination amounts should add up to the transaction amount."
        >
          <Stack spacing={2}>
            <SpendingTransactionSourceFrame
              accounts={accounts}
              account={sourceAccount}
              setAccount={setSourceAccount}
              filter={(account) => {
                const selectedAccount =
                  accounts.find((candidate) => candidate.id === account.id) ??
                  null;
                return (
                  selectedAccount !== null &&
                  isTrackedAccountType(selectedAccount.type)
                );
              }}
            />
            {destinations.map((destination, index) => (
              <SpendingTransactionDestinationFrame
                key={`spending-destination-${index}`}
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
                    location:
                      account === null ? currentDestination.location : "",
                  }));
                }}
                location={destination.location}
                setLocation={(location) => {
                  updateDestination(index, (currentDestination) => ({
                    ...currentDestination,
                    location,
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
                  const selectedAccount =
                    accounts.find((candidate) => candidate.id === account.id) ??
                    null;
                  const accountUsedElsewhere = destinations.some(
                    (currentDestination, currentIndex) =>
                      currentIndex !== index &&
                      currentDestination.account?.id === account.id,
                  );
                  return (
                    selectedAccount !== null &&
                    !isTrackedAccountType(selectedAccount.type) &&
                    account.id !== sourceAccount?.id &&
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
                amount !== null && destinationTotal !== amount
                  ? "error.main"
                  : "text.secondary"
              }
            >
              Destination total: ${formatDestinationTotal(destinationTotal)}
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
            Update Spending Transaction
          </Button>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default UpdateSpendingTransactionForm;
