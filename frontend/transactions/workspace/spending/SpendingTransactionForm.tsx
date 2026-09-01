"use client";

import type {
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import type { Dispatch, JSX, RefObject, SetStateAction } from "react";
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
import type { AccountingPeriod } from "@/accounting-periods/types";
import AddCircleOutline from "@mui/icons-material/AddCircleOutline";
import { Button } from "@mui/material";
import type { Dayjs } from "dayjs";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import SpendingTransactionDestinationFrame from "@/transactions/workspace/spending/SpendingTransactionDestinationFrame";
import SpendingTransactionSourceFrame from "@/transactions/workspace/spending/SpendingTransactionSourceFrame";
import TransactionForm from "@/transactions/workspace/TransactionForm";
import { getCurrencyTotal } from "@/framework/currencyHelpers";
import { updateUnassignedFundAmount } from "@/funds/assignmentPlanner/helpers";

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
 * Props for the SpendingTransactionForm component.
 */
interface SpendingTransactionFormProps<RequestPayload> {
  readonly formRef: RefObject<HTMLDivElement | null>;
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
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
const SpendingTransactionForm = function <RequestPayload>({
  formRef,
  accounts,
  funds,
  fundGoals,
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
}: SpendingTransactionFormProps<RequestPayload>): JSX.Element {
  const unassignedFund =
    funds.find((fund) => fund.name === "Unassigned") ?? null;
  const currentFundGoals = fundGoals.filter(
    (fundGoal) => fundGoal.accountingPeriod?.id === accountingPeriod?.id,
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

  const destinationTotal = getCurrencyTotal(
    destinations.map((destination) => destination.amount),
  );

  const addDestination = function (): void {
    setDestinations((currentDestinations) => [
      ...currentDestinations,
      createEmptyDestination(),
    ]);
  };

  const setDestinationAccount = function (
    index: number,
    account: AccountBalanceEventDraft | null,
  ): void {
    updateDestination(index, (currentDestination) => ({
      ...currentDestination,
      account,
      location: account === null ? currentDestination.location : null,
    }));
  };

  const sourceIsValid = validateSource(source);

  return (
    <TransactionForm
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
        <SpendingTransactionSourceFrame
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
          amount={destinationTotal}
          setAmount={null}
          accountFilter={buildSourceAccountFilter(accounts, destinations)}
        />
      }
      destinationContent={
        <>
          {destinations.map((destination, index) => (
            <SpendingTransactionDestinationFrame
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
              fundGoals={currentFundGoals}
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
                updateDestination(index, (currentDestination) => ({
                  ...currentDestination,
                  amount: nextAmount,
                  fundAssignments: updateUnassignedFundAmount(
                    unassignedFund,
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
        </>
      }
      showSummary={false}
      actionsContent={
        <Button
          variant="outlined"
          startIcon={<AddCircleOutline />}
          onClick={addDestination}
        >
          Add Destination
        </Button>
      }
      submitLabel={submitLabel}
      state={state}
      pending={pending}
      request={request}
      onReset={onReset}
      onSubmit={onSubmit}
    />
  );
};

export default SpendingTransactionForm;
