"use client";

import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import { Button, Stack, Typography } from "@mui/material";
import {
  type Dispatch,
  type JSX,
  type RefObject,
  type SetStateAction,
  startTransition,
} from "react";
import type { Fund, FundAmount } from "@/funds/types";
import {
  type SpendingDestinationDraft,
  type SpendingSourceDraft,
  buildDestinationAccountFilter,
  buildSourceAccountFilter,
  createEmptyDestination,
} from "@/transactions/workspace/spending/helpers";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { AddCircleOutline } from "@mui/icons-material";
import type { Dayjs } from "dayjs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import SpendingTransactionDestinationFrame from "@/transactions/workspace/spending/SpendingTransactionDestinationFrame";
import SpendingTransactionSourceFrame from "@/transactions/workspace/spending/SpendingTransactionSourceFormFrame";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import formatCurrency from "@/framework/formatCurrency";
import { updateUnassignedFundAmount } from "@/funds/fundAssignment";

/**
 * Represents the state of the spending transaction form.
 */
interface SpendingTransactionFormState {
  readonly success?: boolean;
  readonly transactionId?: string | null;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Props for the CreateOrUpdateSpendingTransactionForm component.
 */
interface CreateOrUpdateSpendingTransactionFormProps<RequestPayload> {
  readonly formRef?: RefObject<HTMLDivElement | null>;
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
  readonly accountingPeriods: AccountingPeriod[];
  readonly accountingPeriod: AccountingPeriod | null;
  readonly setAccountingPeriod?: Dispatch<
    SetStateAction<AccountingPeriod | null>
  > | null;
  readonly date: Dayjs | null;
  readonly setDate: Dispatch<SetStateAction<Dayjs | null>>;
  readonly defaultDate: Dayjs | null;
  readonly description: string;
  readonly setDescription: Dispatch<SetStateAction<string>>;
  readonly source: SpendingSourceDraft;
  readonly setSource: Dispatch<SetStateAction<SpendingSourceDraft>>;
  readonly destinations: SpendingDestinationDraft[];
  readonly setDestinations: Dispatch<
    SetStateAction<SpendingDestinationDraft[]>
  >;
  readonly spendingFlowDescription: string;
  readonly submitLabel: string;
  readonly state: SpendingTransactionFormState;
  readonly pending: boolean;
  readonly request: RequestPayload | null;
  readonly onReset: () => void;
  readonly onSubmit: (request: RequestPayload) => void;
}

/**
 * Displays the shared spending transaction form layout used by create and update flows.
 */
const CreateOrUpdateSpendingTransactionForm = function <RequestPayload>({
  formRef,
  accounts,
  funds,
  assignmentGoals,
  spendingGoals,
  accountingPeriods,
  accountingPeriod,
  setAccountingPeriod = null,
  date,
  setDate,
  defaultDate,
  description,
  setDescription,
  source,
  setSource,
  destinations,
  setDestinations,
  spendingFlowDescription,
  submitLabel,
  state,
  pending,
  request,
  onReset,
  onSubmit,
}: CreateOrUpdateSpendingTransactionFormProps<RequestPayload>): JSX.Element {
  const unassignedFund =
    funds.find((fund) => fund.name === "Unassigned") ?? null;
  const currentAssignmentGoals = assignmentGoals.filter(
    (goal) => goal.accountingPeriodId === accountingPeriod?.id,
  );
  const currentSpendingGoals = spendingGoals.filter(
    (goal) => goal.accountingPeriodId === accountingPeriod?.id,
  );

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
        />
        <TransactionSection
          title="Spending Flow"
          description={spendingFlowDescription}
        >
          <Stack spacing={2}>
            <SpendingTransactionSourceFrame
              accounts={accounts}
              account={source.account}
              setAccount={(account): void => {
                setSource((currentSource) => ({
                  ...currentSource,
                  account,
                }));
              }}
              amount={source.amount}
              setAmount={(amount): void => {
                setSource((currentSource) => ({
                  ...currentSource,
                  amount,
                }));
              }}
              accountFilter={buildSourceAccountFilter(accounts, destinations)}
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
                setAccount={(account): void => {
                  updateDestination(index, (currentDestination) => ({
                    ...currentDestination,
                    account,
                    location:
                      account === null ? currentDestination.location : "",
                  }));
                }}
                location={destination.location}
                setLocation={(location): void => {
                  updateDestination(index, (currentDestination) => ({
                    ...currentDestination,
                    location,
                  }));
                }}
                amount={destination.amount}
                setAmount={(nextAmount): void => {
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
                setFundAssignments={(fundAssignments): void => {
                  updateDestination(index, (currentDestination) => ({
                    ...currentDestination,
                    fundAssignments,
                  }));
                }}
                baselineFundAssignments={destination.baselineFundAssignments}
                filter={buildDestinationAccountFilter(
                  accounts,
                  destinations,
                  index,
                  source.account,
                )}
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
              onClick={(): void => {
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
                source.amount !== null && destinationTotal !== source.amount
                  ? "error.main"
                  : "text.secondary"
              }
            >
              Destination total: {formatCurrency(destinationTotal)}
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
          <Button variant="outlined" onClick={onReset}>
            Reset
          </Button>
          <Button
            variant="contained"
            loading={pending}
            disabled={request === null}
            onClick={(): void => {
              if (request === null) {
                return;
              }
              startTransition(() => {
                onSubmit(request);
              });
            }}
          >
            {submitLabel}
          </Button>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default CreateOrUpdateSpendingTransactionForm;
