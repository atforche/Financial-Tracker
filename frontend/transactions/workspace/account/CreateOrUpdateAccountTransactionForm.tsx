"use client";

import {
  type AccountDestinationDraft,
  type AccountSourceDraft,
  buildDestinationAccountFilter,
  buildSourceAccountFilter,
  createEmptyDestination,
} from "@/transactions/workspace/account/helpers";
import type { Dispatch, JSX, RefObject, SetStateAction } from "react";
import {
  appendDestinationWithAutofilledAmount,
  syncDestinationAmountsToSource,
} from "@/transactions/workspace/helpers";
import type { Account } from "@/accounts/types";
import AccountTransactionDestinationFrame from "@/transactions/workspace/account/AccountTransactionDestinationFormFrame";
import AccountTransactionSourceFrame from "@/transactions/workspace/account/AccountTransactionSourceFormFrame";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateOrUpdateTransactionForm from "@/transactions/workspace/CreateOrUpdateTransactionForm";
import type { Dayjs } from "dayjs";

/**
 * Represents the state of the account transaction form.
 */
interface TransactionFormState {
  readonly success?: boolean;
  readonly transactionId?: string | null;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Props for the CreateOrUpdateAccountTransactionForm component.
 */
interface CreateOrUpdateAccountTransactionFormProps<RequestPayload> {
  readonly formRef: RefObject<HTMLDivElement | null>;
  readonly accounts: Account[];
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
  readonly source: AccountSourceDraft;
  readonly setSource: Dispatch<SetStateAction<AccountSourceDraft>>;
  readonly destinations: AccountDestinationDraft[];
  readonly setDestinations: Dispatch<SetStateAction<AccountDestinationDraft[]>>;
  readonly submitLabel: string;
  readonly state: TransactionFormState;
  readonly pending: boolean;
  readonly request: RequestPayload | null;
  readonly onReset: () => void;
  readonly onSubmit: (request: RequestPayload) => void;
}

/**
 * Displays the shared account transaction form layout used by create and update flows.
 */
const CreateOrUpdateAccountTransactionForm = function <RequestPayload>({
  formRef,
  accounts,
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
}: CreateOrUpdateAccountTransactionFormProps<RequestPayload>): JSX.Element {
  const setDestinationAmount = function (
    destination: AccountDestinationDraft,
    amount: number | null,
  ): AccountDestinationDraft {
    return {
      ...destination,
      amount,
    };
  };

  const updateDestination = function (
    index: number,
    recipe: (current: AccountDestinationDraft) => AccountDestinationDraft,
  ): void {
    setDestinations((currentDestinations) =>
      currentDestinations.map((currentDestination, currentIndex) =>
        currentIndex === index
          ? recipe(currentDestination)
          : currentDestination,
      ),
    );
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
        <AccountTransactionSourceFrame
          accounts={accounts}
          account={source.account}
          setAccount={(account): void => {
            setSource((currentSource) => ({
              ...currentSource,
              account,
              location: account === null ? currentSource.location : "",
            }));
          }}
          location={source.location}
          setLocation={(location): void => {
            setSource((currentSource) => ({
              ...currentSource,
              location,
            }));
          }}
          accountFilter={buildSourceAccountFilter(accounts, destinations)}
          amount={source.amount}
          setAmount={(nextAmount): void => {
            setSource((currentSource) => ({
              ...currentSource,
              amount: nextAmount,
            }));
            setDestinations((currentDestinations) =>
              syncDestinationAmountsToSource(
                currentDestinations,
                source.amount,
                nextAmount,
                setDestinationAmount,
              ),
            );
          }}
        />
      }
      destinationContent={
        <>
          {destinations.map((destination, index) => (
            <AccountTransactionDestinationFrame
              key={`account-destination-${index}`}
              index={index}
              accounts={accounts}
              account={destination.account}
              setAccount={(account): void => {
                updateDestination(index, (currentDestination) => ({
                  ...currentDestination,
                  account,
                  location: account === null ? currentDestination.location : "",
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
                }));
              }}
              onAdd={index === 0 ? addDestination : null}
              accountFilter={buildDestinationAccountFilter(
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
      submitLabel={submitLabel}
      state={state}
      pending={pending}
      request={request}
      onReset={onReset}
      onSubmit={onSubmit}
    />
  );
};

export default CreateOrUpdateAccountTransactionForm;
