"use client";

import {
  type AccountDestinationDraft,
  buildDestinationAccountFilter,
  buildRequest,
  buildSourceAccountFilter,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/account/createOrUpdateAccountTransaction";
import { Button, Stack, Typography } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@/accounts/types";
import AccountTransactionDestinationFrame from "@/transactions/workspace/account/AccountTransactionDestinationFormFrame";
import AccountTransactionSourceFrame from "@/transactions/workspace/account/AccountTransactionSourceFormFrame";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { AddCircleOutline } from "@mui/icons-material";
import type { CreateTransactionRequest } from "@/transactions/transaction";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import createTransaction from "@/transactions/workspace/createTransaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import formatCurrency from "@/framework/formatCurrency";
import { useRouter } from "next/navigation";

/**
 * Props for the CreateAccountTransactionForm component.
 */
interface CreateAccountTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: Account[];
  readonly redirectUrl: string;
}

/**
 * Gets the default accounting period from a list of accounting periods.
 */
const getDefaultAccountingPeriod = function (
  accountingPeriods: AccountingPeriod[],
): AccountingPeriod | null {
  return accountingPeriods.length > 0
    ? (accountingPeriods[accountingPeriods.length - 1] ?? null)
    : null;
};

/**
 * Gets the default date from an accounting period.
 */
const getDefaultDate = function (
  accountingPeriod: AccountingPeriod | null,
): Dayjs | null {
  return accountingPeriod !== null
    ? dayjs(`${accountingPeriod.year}-${accountingPeriod.month}-01`)
    : null;
};

/**
 * Displays the dedicated create form for account transactions.
 */
const CreateAccountTransactionForm = function ({
  accountingPeriods,
  accounts,
  redirectUrl,
}: CreateAccountTransactionFormProps): JSX.Element {
  const router = useRouter();
  const formRef = useRef<HTMLDivElement | null>(null);

  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(
      getDefaultAccountingPeriod(accountingPeriods),
    );
  const defaultDate = getDefaultDate(accountingPeriod);
  const [date, setDate] = useState<Dayjs | null>(null);
  const [description, setDescription] = useState<string>("");
  const [source, setSource] = useState(createEmptySource());
  const [destinations, setDestinations] = useState<AccountDestinationDraft[]>([
    createEmptyDestination(),
  ]);

  const [state, action, pending] = useActionState(createTransaction, {});

  const reset = function (): void {
    setAccountingPeriod(getDefaultAccountingPeriod(accountingPeriods));
    setDate(null);
    setDescription("");
    setSource(createEmptySource());
    setDestinations([createEmptyDestination()]);
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (state.success === true && state.transactionId !== null) {
      const [pathname, search = ""] = redirectUrl.split("?");
      const params = new URLSearchParams(search);
      params.set("selectedTransactionId", state.transactionId ?? "");
      const query = params.toString();
      const nextUrl =
        query === ""
          ? `${pathname}/${state.transactionId ?? ""}`
          : `${pathname}/${state.transactionId ?? ""}?${query}`;
      router.replace(nextUrl, { scroll: false });
    }
  }, [redirectUrl, router, state]);

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

  const request: CreateTransactionRequest | null = buildRequest(
    accountingPeriod,
    date,
    defaultDate,
    description,
    source,
    destinations,
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
          description="Build one source and one or more destinations. The destination amounts should add up to the transaction amount."
        >
          <Stack spacing={2}>
            <AccountTransactionSourceFrame
              accounts={accounts}
              account={source.account}
              setAccount={(account) => {
                setSource((currentSource) => ({
                  ...currentSource,
                  account,
                  location: account === null ? currentSource.location : "",
                }));
              }}
              location={source.location}
              setLocation={(location) => {
                setSource((currentSource) => ({
                  ...currentSource,
                  location,
                }));
              }}
              accountFilter={buildSourceAccountFilter(accounts, destinations)}
              amount={source.amount}
              setAmount={(nextAmount) => {
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
                action({ redirectUrl, request });
              });
            }}
          >
            Create
          </Button>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default CreateAccountTransactionForm;
