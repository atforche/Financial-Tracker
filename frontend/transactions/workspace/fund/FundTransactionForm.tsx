"use client";

import type { Dispatch, JSX, RefObject, SetStateAction } from "react";
import {
  type FundDestinationDraft,
  type FundSourceDraft,
  buildDestinationFundFilter,
  buildSourceFundFilter,
  createEmptyDestination,
  validateDestination,
  validateSource,
} from "@/transactions/workspace/fund/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Dayjs } from "dayjs";
import FundTransactionDestinationFrame from "@/transactions/workspace/fund/FundTransactionDestinationFrame";
import FundTransactionSourceFrame from "@/transactions/workspace/fund/FundTransactionSourceFrame";
import type { FundWithBalance } from "@/funds/types";
import TransactionForm from "@/transactions/workspace/TransactionForm";
import { getCurrencyTotal } from "@/framework/currencyHelpers";

/**
 * Represents the state of the fund transaction form.
 */
interface TransactionFormState {
  readonly success?: boolean;
  readonly transactionId?: string | null;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Props for the FundTransactionForm component.
 */
interface FundTransactionFormProps<RequestPayload> {
  readonly formRef: RefObject<HTMLDivElement | null>;
  readonly funds: FundWithBalance[];
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
  readonly source: FundSourceDraft;
  readonly setSource: Dispatch<SetStateAction<FundSourceDraft>>;
  readonly destinations: FundDestinationDraft[];
  readonly setDestinations: Dispatch<SetStateAction<FundDestinationDraft[]>>;
  readonly submitLabel: string;
  readonly state: TransactionFormState;
  readonly pending: boolean;
  readonly request: RequestPayload | null;
  readonly onReset: () => void;
  readonly onSubmit: (request: RequestPayload) => void;
}

/**
 * Displays the shared fund transaction form layout used by create and update flows.
 */
const FundTransactionForm = function <RequestPayload>({
  formRef,
  funds,
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
}: FundTransactionFormProps<RequestPayload>): JSX.Element {
  const updateDestination = function (
    index: number,
    recipe: (current: FundDestinationDraft) => FundDestinationDraft,
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
        <FundTransactionSourceFrame
          color={sourceIsValid ? "info" : "error"}
          funds={funds}
          fund={source.fund}
          setFund={(fund): void => {
            setSource((currentSource) => ({
              ...currentSource,
              fund,
            }));
          }}
          filter={buildSourceFundFilter(destinations)}
          amount={destinationTotal}
          setAmount={null}
        />
      }
      destinationContent={
        <>
          {destinations.map((destination, index) => (
            <FundTransactionDestinationFrame
              key={`fund-destination-${index}`}
              color={
                validateDestination(destination, source.fund) ? "info" : "error"
              }
              index={index}
              funds={funds}
              fund={destination.fund}
              setFund={(fund): void => {
                updateDestination(index, (currentDestination) => ({
                  ...currentDestination,
                  fund,
                }));
              }}
              amount={destination.amount}
              setAmount={(nextAmount: number | null): void => {
                updateDestination(index, (currentDestination) => ({
                  ...currentDestination,
                  amount: nextAmount,
                }));
              }}
              onAdd={index === 0 ? addDestination : null}
              filter={buildDestinationFundFilter(
                destinations,
                index,
                source.fund,
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
      sourceAmount={destinationTotal}
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

export default FundTransactionForm;
