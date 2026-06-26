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
  type IncomeDestinationDraft,
  type IncomeSourceDraft,
  buildDestinationAccountFilter,
  buildSourceAccountFilter,
  createEmptyDestination,
  getNetIncomeAmount,
} from "@/transactions/workspace/income/helpers";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { AddCircleOutline } from "@mui/icons-material";
import type { Dayjs } from "dayjs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import IncomeTransactionDestinationFrame from "@/transactions/workspace/income/IncomeTransactionDestinationFrame";
import IncomeTransactionSourceFrame from "@/transactions/workspace/income/IncomeTransactionSourceFormFrame";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import formatCurrency from "@/framework/formatCurrency";
import { updateUnassignedFundAmount } from "@/funds/fundAssignment";

/**
 * Represents the state of the income transaction form.
 */
interface IncomeTransactionFormState {
  readonly success?: boolean;
  readonly transactionId?: string | null;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Props for the CreateOrUpdateIncomeTransactionForm component.
 */
interface CreateOrUpdateIncomeTransactionFormProps<RequestPayload> {
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
  readonly source: IncomeSourceDraft;
  readonly setSource: Dispatch<SetStateAction<IncomeSourceDraft>>;
  readonly destinations: IncomeDestinationDraft[];
  readonly setDestinations: Dispatch<SetStateAction<IncomeDestinationDraft[]>>;
  readonly incomeFlowDescription: string;
  readonly submitLabel: string;
  readonly state: IncomeTransactionFormState;
  readonly pending: boolean;
  readonly request: RequestPayload | null;
  readonly onReset: () => void;
  readonly onSubmit: (request: RequestPayload) => void;
}

/**
 * Displays the shared income transaction form layout used by create and update flows.
 */
const CreateOrUpdateIncomeTransactionForm = function <RequestPayload>({
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
  incomeFlowDescription,
  submitLabel,
  state,
  pending,
  request,
  onReset,
  onSubmit,
}: CreateOrUpdateIncomeTransactionFormProps<RequestPayload>): JSX.Element {
  const unassignedFund =
    funds.find((fund) => fund.name === "Unassigned") ?? null;
  const currentAssignmentGoals = assignmentGoals.filter(
    (goal) => goal.accountingPeriodId === accountingPeriod?.id,
  );
  const currentSpendingGoals = spendingGoals.filter(
    (goal) => goal.accountingPeriodId === accountingPeriod?.id,
  );
  const sourceNetAmount = getNetIncomeAmount(source);
  const destinationTotal = destinations.reduce(
    (total, destination) => total + (destination.amount ?? 0),
    0,
  );

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
          title="Income Flow"
          description={incomeFlowDescription}
        >
          <Stack spacing={2}>
            <IncomeTransactionSourceFrame
              accounts={accounts}
              account={source.account}
              setAccount={(account): void => {
                setSource((currentSource) => ({
                  ...currentSource,
                  account,
                  location: account === null ? currentSource.location : null,
                }));
              }}
              location={source.location}
              setLocation={(location): void => {
                setSource((currentSource) => ({
                  ...currentSource,
                  location,
                }));
              }}
              incomeLines={source.incomeLines}
              setIncomeLines={(incomeLines): void => {
                setSource((currentSource) => ({
                  ...currentSource,
                  incomeLines,
                }));
              }}
              incomeDeductions={source.incomeDeductions}
              setIncomeDeductions={(incomeDeductions): void => {
                setSource((currentSource) => ({
                  ...currentSource,
                  incomeDeductions,
                }));
              }}
              accountFilter={buildSourceAccountFilter(accounts, destinations)}
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
                setAccount={(account): void => {
                  updateDestination(index, (currentDestination) => ({
                    ...currentDestination,
                    account,
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
            <Typography variant="body2" color="text.secondary">
              Source net total: {formatCurrency(sourceNetAmount)}
            </Typography>
            <Typography
              variant="body2"
              color={
                destinationTotal !== sourceNetAmount
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

export default CreateOrUpdateIncomeTransactionForm;
