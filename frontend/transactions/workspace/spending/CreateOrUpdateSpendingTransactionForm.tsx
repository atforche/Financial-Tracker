"use client";

import type { Dispatch, JSX, RefObject, SetStateAction } from "react";
import {
  type FundAssignmentDraft,
  updateUnassignedFundAmount,
} from "@/funds/assignmentPlanner/helpers";
import {
  type SpendingDestinationDraft,
  type SpendingSourceDraft,
  buildDestinationAccountFilter,
  buildSourceAccountFilter,
  createEmptyDestination,
  validateDestination,
  validateFundAssignments,
  validateSource,
} from "@/transactions/workspace/spending/helpers";
import {
  appendDestinationWithAutofilledAmount,
  syncDestinationAmountsToSource,
} from "@/transactions/workspace/helpers";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateOrUpdateTransactionForm from "@/transactions/workspace/CreateOrUpdateTransactionForm";
import type { Dayjs } from "dayjs";
import type { Fund } from "@/funds/types";
import type { SpendingGoal } from "@/goals/types";
import SpendingTransactionDestinationFormFrame from "@/transactions/workspace/spending/SpendingTransactionDestinationFormFrame";
import SpendingTransactionSourceFormFrame from "@/transactions/workspace/spending/SpendingTransactionSourceFormFrame";
import type { TransactionAccountDraft } from "@/transactions/transaction";

/**
 * Represents the state of the spending transaction form.
 */
interface TransactionFormState {
  readonly success?: boolean;
  readonly transactionId?: string | null;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Props for the CreateOrUpdateSpendingTransactionForm component.
 */
interface CreateOrUpdateSpendingTransactionFormProps<RequestPayload> {
  readonly formRef: RefObject<HTMLDivElement | null>;
  readonly accounts: Account[];
  readonly funds: Fund[];
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
  readonly submitLabel: string;
  readonly state: TransactionFormState;
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
  submitLabel,
  state,
  pending,
  request,
  onReset,
  onSubmit,
}: CreateOrUpdateSpendingTransactionFormProps<RequestPayload>): JSX.Element {
  const unassignedFund =
    funds.find((fund) => fund.name === "Unassigned") ?? null;
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
    fundAssignments: FundAssignmentDraft[],
  ): FundAssignmentDraft[] {
    return updateUnassignedFundAmount(
      unassignedFund,
      destinationAmount,
      fundAssignments,
    );
  };

  const setDestinationAmount = function (
    destination: SpendingDestinationDraft,
    amount: number | null,
  ): SpendingDestinationDraft {
    return {
      ...destination,
      amount,
      fundAssignments: syncDestinationFundAssignments(
        amount,
        destination.fundAssignments,
      ),
    };
  };

  const destinationTotal = destinations.reduce(
    (total, destination) => total + (destination.amount ?? 0),
    0,
  );

  const addDestination = function (): void {
    setDestinations((currentDestinations) =>
      appendDestinationWithAutofilledAmount(
        currentDestinations,
        createEmptyDestination(),
        source.amount,
        setDestinationAmount,
      ),
    );
  };

  const setDestinationAccount = function (
    index: number,
    account: TransactionAccountDraft | null,
  ): void {
    updateDestination(index, (currentDestination) => ({
      ...currentDestination,
      account,
      location: account === null ? currentDestination.location : "",
    }));
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
        <SpendingTransactionSourceFormFrame
          color={sourceIsValid ? "info" : "error"}
          accounts={accounts}
          transaction={null}
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
            setDestinations((currentDestinations) =>
              syncDestinationAmountsToSource(
                currentDestinations,
                source.amount,
                amount,
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
            <SpendingTransactionDestinationFormFrame
              key={`spending-destination-${index}`}
              color={
                validateDestination(destination, source.account)
                  ? "info"
                  : "error"
              }
              fundAssignmentsValid={validateFundAssignments(destination)}
              index={index}
              accounts={accounts}
              funds={funds}
              spendingGoals={currentSpendingGoals}
              transaction={null}
              account={destination.account}
              setAccount={(account): void => {
                setDestinationAccount(index, account);
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
      sourceAmount={source.amount}
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

export default CreateOrUpdateSpendingTransactionForm;
