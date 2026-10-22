"use client";

import type {
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import {
  type Dispatch,
  type JSX,
  type RefObject,
  type SetStateAction,
  useLayoutEffect,
} from "react";
import type { Fund, FundWithBalance } from "@/funds/types";
import {
  type IncomeDestinationDraft,
  type IncomeSourceDraft,
  buildDestinationAccountFilter,
  buildSourceAccountFilter,
  createEmptyDestination,
  getNetIncomeAmount,
  isTrackedAccount,
  validateDestination,
  validateFundAssignments,
  validateSource,
} from "@/transactions/workspace/income/helpers";
import {
  compareCurrencyAmounts,
  getCurrencyTotal,
} from "@/framework/currencyHelpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CollectionEditor from "@/framework/view/CollectionEditor";
import type { Dayjs } from "dayjs";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import IncomeTransactionDestinationFrame from "@/transactions/workspace/income/IncomeTransactionDestinationFrame";
import IncomeTransactionSourceFrame from "@/transactions/workspace/income/IncomeTransactionSourceFrame";
import TransactionForm from "@/transactions/workspace/TransactionForm";
import { getUnassignedFund } from "@/funds/helpers";
import { updateUnassignedFundAmount } from "@/funds/assignmentPlanner/helpers";

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
 * Props for the IncomeTransactionForm component.
 */
interface IncomeTransactionFormProps<RequestPayload> {
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
 * Updates the destination amount and keeps its unassigned remainder in sync.
 */
const setDestinationAmount = function (
  destination: IncomeDestinationDraft,
  amount: number | null,
  unassignedFund: Fund | null,
): IncomeDestinationDraft {
  return {
    ...destination,
    amount,
    fundAssignments:
      isTrackedAccount(destination.account) === false
        ? []
        : updateUnassignedFundAmount(
            unassignedFund,
            amount,
            destination.fundAssignments,
          ),
  };
};

/**
 * Sets the amount of a single destination to the net income amount.
 */
const syncSingleDestinationAmount = function (
  destinations: IncomeDestinationDraft[],
  netAmount: number,
  unassignedFund: Fund | null,
): IncomeDestinationDraft[] {
  const [destination] = destinations;
  if (destinations.length !== 1 || destination === undefined) {
    return destinations;
  }
  const amount = compareCurrencyAmounts(netAmount, 0) > 0 ? netAmount : null;
  if (
    amount === destination.amount ||
    (amount !== null &&
      destination.amount !== null &&
      compareCurrencyAmounts(amount, destination.amount) === 0)
  ) {
    return destinations;
  }
  return [setDestinationAmount(destination, amount, unassignedFund)];
};

/**
 * Displays the shared income transaction form layout used by create and update flows.
 */
const IncomeTransactionForm = function <RequestPayload>({
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
}: IncomeTransactionFormProps<RequestPayload>): JSX.Element {
  const unassignedFund = getUnassignedFund(funds);
  const currentFundGoals = fundGoals.filter(
    (fundGoal) => fundGoal.accountingPeriod?.id === accountingPeriod?.id,
  );
  const sourceNetAmount = getNetIncomeAmount(source);
  const destinationTotal = getCurrencyTotal(
    destinations.map((destination) => destination.amount),
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

  const setSourceAccount = function (
    account: AccountBalanceEventDraft | null,
  ): void {
    setSource((currentSource) => ({
      ...currentSource,
      account,
      location: account === null ? currentSource.location : null,
    }));
  };

  // Sync after render so the single destination amount always uses the latest source.
  useLayoutEffect(() => {
    setDestinations((currentDestinations) =>
      syncSingleDestinationAmount(
        currentDestinations,
        sourceNetAmount,
        unassignedFund,
      ),
    );
  }, [sourceNetAmount, destinations.length, unassignedFund, setDestinations]);

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
        <IncomeTransactionSourceFrame
          color={sourceIsValid ? "info" : "error"}
          accounts={accounts}
          transaction={null}
          account={source.account}
          setAccount={setSourceAccount}
          location={source.location}
          setLocation={(location): void => {
            setSource((currentSource) => ({
              ...currentSource,
              location,
            }));
          }}
          incomeLines={source.incomeLines}
          setIncomeLines={(incomeLines): void => {
            setSource((currentSource) => ({ ...currentSource, incomeLines }));
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
      }
      destinationContent={
        <CollectionEditor
          items={destinations}
          label="Destinations"
          setItems={setDestinations}
          createItem={createEmptyDestination}
          addLabel="Add another destination"
          canDeleteItem={(_, __, items) => items.length > 1}
          renderItem={(destination, index, controls) => (
            <IncomeTransactionDestinationFrame
              key={`income-destination-${index}`}
              color={validateDestination(destination) ? "info" : "error"}
              fundAssignmentsValid={validateFundAssignments(destination)}
              index={index}
              accounts={accounts}
              funds={funds}
              fundGoals={currentFundGoals}
              transaction={null}
              account={destination.account}
              setAccount={(account): void => {
                updateDestination(index, (currentDestination) => ({
                  ...currentDestination,
                  account,
                  fundAssignments:
                    isTrackedAccount(account) === true
                      ? currentDestination.fundAssignments
                      : [],
                }));
              }}
              amount={destination.amount}
              setAmount={
                destinations.length === 1
                  ? null
                  : (nextAmount): void => {
                      updateDestination(index, (currentDestination) =>
                        setDestinationAmount(
                          currentDestination,
                          nextAmount,
                          unassignedFund,
                        ),
                      );
                    }
              }
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
              autoFocus={controls.autoFocus}
              onRemove={destinations.length > 1 ? controls.onRemove : null}
            />
          )}
        />
      }
      sourceAmount={sourceNetAmount}
      destinationAmount={destinationTotal}
      destinationCount={destinations.length}
      showSummary
      submitLabel={submitLabel}
      state={state}
      pending={pending}
      request={request}
      onReset={onReset}
      onSubmit={onSubmit}
    />
  );
};

export default IncomeTransactionForm;
