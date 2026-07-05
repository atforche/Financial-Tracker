"use client";

import type { Dispatch, JSX, RefObject, SetStateAction } from "react";
import {
  type FundAssignmentDraft,
  updateUnassignedFundAmount,
} from "@/funds/assignmentPlanner/helpers";
import {
  type IncomeDestinationDraft,
  type IncomeSourceDraft,
  buildDestinationAccountFilter,
  buildSourceAccountFilter,
  createEmptyDestination,
  getNetIncomeAmount,
  validateDestination,
  validateFundAssignments,
  validateSource,
} from "@/transactions/workspace/income/helpers";
import {
  appendDestinationWithAutofilledAmount,
  syncDestinationAmountsToSource,
} from "@/transactions/workspace/helpers";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { AssignmentGoal } from "@/goals/types";
import CreateOrUpdateTransactionForm from "@/transactions/workspace/CreateOrUpdateTransactionForm";
import type { Dayjs } from "dayjs";
import type { Fund } from "@/funds/types";
import IncomeTransactionDestinationFrame from "@/transactions/workspace/income/IncomeTransactionDestinationFormFrame";
import IncomeTransactionSourceFrame from "@/transactions/workspace/income/IncomeTransactionSourceFormFrame";

/**
 * Represents the state of the income transaction form.
 */
interface TransactionFormState {
  readonly success?: boolean;
  readonly transactionId?: string | null;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Props for the CreateOrUpdateIncomeTransactionForm component.
 */
interface CreateOrUpdateIncomeTransactionFormProps<RequestPayload> {
  readonly formRef: RefObject<HTMLDivElement | null>;
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
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
  readonly submitLabel: string;
  readonly state: TransactionFormState;
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
    fundAssignments: FundAssignmentDraft[],
  ): FundAssignmentDraft[] {
    return updateUnassignedFundAmount(
      unassignedFund,
      destinationAmount,
      fundAssignments,
    );
  };

  const setDestinationAmount = function (
    destination: IncomeDestinationDraft,
    amount: number | null,
  ): IncomeDestinationDraft {
    return {
      ...destination,
      amount,
      fundAssignments: syncDestinationFundAssignments(
        amount,
        destination.fundAssignments,
      ),
    };
  };

  const addDestination = function (): void {
    setDestinations((currentDestinations) =>
      appendDestinationWithAutofilledAmount(
        currentDestinations,
        createEmptyDestination(),
        sourceNetAmount,
        setDestinationAmount,
      ),
    );
  };

  const sourceIsValid = validateSource(source);

  return (
    <CreateOrUpdateTransactionForm
      formRef={formRef}
      accountingPeriods={accountingPeriods}
      accountingPeriod={accountingPeriod}
      setAccountingPeriod={setAccountingPeriod}
      date={date}
      setDate={setDate}
      defaultDate={defaultDate}
      description={description}
      setDescription={setDescription}
      sourceContent={
        <IncomeTransactionSourceFrame
          color={sourceIsValid ? "info" : "error"}
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
            const nextSource = {
              ...source,
              incomeLines,
            };
            setSource((currentSource) => ({
              ...currentSource,
              incomeLines,
            }));
            setDestinations((currentDestinations) =>
              syncDestinationAmountsToSource(
                currentDestinations,
                sourceNetAmount,
                getNetIncomeAmount(nextSource),
                setDestinationAmount,
              ),
            );
          }}
          incomeDeductions={source.incomeDeductions}
          setIncomeDeductions={(incomeDeductions): void => {
            const nextSource = {
              ...source,
              incomeDeductions,
            };
            setSource((currentSource) => ({
              ...currentSource,
              incomeDeductions,
            }));
            setDestinations((currentDestinations) =>
              syncDestinationAmountsToSource(
                currentDestinations,
                sourceNetAmount,
                getNetIncomeAmount(nextSource),
                setDestinationAmount,
              ),
            );
          }}
          accountFilter={buildSourceAccountFilter(accounts, destinations)}
        />
      }
      destinationContent={
        <>
          {destinations.map((destination, index) => (
            <IncomeTransactionDestinationFrame
              key={`income-destination-${index}`}
              color={validateDestination(destination) ? "info" : "error"}
              fundAssignmentsValid={validateFundAssignments(destination)}
              index={index}
              accounts={accounts}
              funds={funds}
              assignmentGoals={currentAssignmentGoals}
              account={destination.account}
              setAccount={(account): void => {
                updateDestination(index, (currentDestination) => ({
                  ...currentDestination,
                  account,
                }));
              }}
              amount={destination.amount}
              setAmount={(nextAmount): void => {
                updateDestination(index, (currentDestination) =>
                  setDestinationAmount(currentDestination, nextAmount),
                );
              }}
              fundAssignments={destination.fundAssignments}
              setFundAssignments={(fundAssignments): void => {
                updateDestination(index, (currentDestination) => ({
                  ...currentDestination,
                  fundAssignments,
                }));
              }}
              baselineFundAssignments={destination.baselineFundAssignments}
              onAdd={index === 0 ? addDestination : null}
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
        </>
      }
      sourceAmount={sourceNetAmount}
      destinationAmount={destinationTotal}
      destinationCount={destinations.length}
      submitLabel={submitLabel}
      state={state}
      pending={pending}
      request={request}
      onReset={onReset}
      onSubmit={onSubmit}
    />
  );
};

export default CreateOrUpdateIncomeTransactionForm;
