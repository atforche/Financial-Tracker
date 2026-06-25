"use client";

import {
  type AccountDestinationDraft,
  type AccountSourceDraft,
  buildDestinationAccountFilter,
  buildSourceAccountFilter,
  createEmptyDestination,
} from "@/transactions/workspace/account/createOrUpdateAccountTransaction";
import { Button, Stack, Typography } from "@mui/material";
import {
  type Dispatch,
  type JSX,
  type RefObject,
  type SetStateAction,
  startTransition,
} from "react";
import type { Account } from "@/accounts/types";
import AccountTransactionDestinationFrame from "@/transactions/workspace/account/AccountTransactionDestinationFormFrame";
import AccountTransactionSourceFrame from "@/transactions/workspace/account/AccountTransactionSourceFormFrame";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { AddCircleOutline } from "@mui/icons-material";
import type { Dayjs } from "dayjs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Represents the state of the account transaction form.
 */
interface AccountTransactionFormState {
  readonly success?: boolean;
  readonly transactionId?: string | null;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Props for the CreateOrUpdateAccountTransactionForm component.
 */
interface CreateOrUpdateAccountTransactionFormProps<RequestPayload> {
  readonly formRef?: RefObject<HTMLDivElement | null>;
  readonly accounts: Account[];
  readonly accountingPeriods: AccountingPeriod[];
  readonly accountingPeriod: AccountingPeriod | null;
  readonly setAccountingPeriod?:
    | Dispatch<SetStateAction<AccountingPeriod | null>>
    | null;
  readonly date: Dayjs | null;
  readonly setDate: Dispatch<SetStateAction<Dayjs | null>>;
  readonly defaultDate: Dayjs | null;
  readonly description: string;
  readonly setDescription: Dispatch<SetStateAction<string>>;
  readonly source: AccountSourceDraft;
  readonly setSource: Dispatch<SetStateAction<AccountSourceDraft>>;
  readonly destinations: AccountDestinationDraft[];
  readonly setDestinations: Dispatch<SetStateAction<AccountDestinationDraft[]>>;
  readonly transferFlowDescription: string;
  readonly submitLabel: string;
  readonly state: AccountTransactionFormState;
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
  transferFlowDescription,
  submitLabel,
  state,
  pending,
  request,
  onReset,
  onSubmit,
}: CreateOrUpdateAccountTransactionFormProps<RequestPayload>): JSX.Element {
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
          title="Transfer Flow"
          description={transferFlowDescription}
        >
          <Stack spacing={2}>
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
              }}
            />
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
                  }));
                }}
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

export default CreateOrUpdateAccountTransactionForm;
