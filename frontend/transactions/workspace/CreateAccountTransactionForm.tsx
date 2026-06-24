"use client";

import { type Account, isTrackedAccountType } from "@/accounts/types";
import { Button, Stack, Typography } from "@mui/material";
import {
  CreateAccountTransactionType,
  type CreateTransactionRequest,
} from "@/transactions/types";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import dayjs, { type Dayjs } from "dayjs";
import AccountTransactionDestinationFrame from "@/transactions/workspace/AccountTransactionDestinationFrame";
import AccountTransactionSourceFrame from "@/transactions/workspace/AccountTransactionSourceFrame";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { AddCircleOutline } from "@mui/icons-material";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import createTransaction from "@/transactions/workspace/createTransaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import { useRouter } from "next/navigation";

interface CreateAccountTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: Account[];
  readonly redirectUrl: string;
}

interface AccountDestinationDraft {
  readonly account: Account | null;
  readonly location: string;
  readonly amount: number | null;
}

const createEmptyDestination = function (): AccountDestinationDraft {
  return {
    account: null,
    location: "",
    amount: null,
  };
};

const formatTotal = function (value: number): string {
  return value.toLocaleString([], {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
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
      accountingPeriods.length > 0
        ? (accountingPeriods[accountingPeriods.length - 1] ?? null)
        : null,
    );
  const defaultDate =
    accountingPeriod !== null
      ? dayjs(`${accountingPeriod.year}-${accountingPeriod.month}-01`)
      : null;
  const [date, setDate] = useState<Dayjs | null>(null);
  const [description, setDescription] = useState<string>("");
  const [amount, setAmount] = useState<number | null>(null);
  const [sourceAccount, setSourceAccount] = useState<Account | null>(null);
  const [sourceLocation, setSourceLocation] = useState<string>("");
  const [destinations, setDestinations] = useState<AccountDestinationDraft[]>([
    createEmptyDestination(),
  ]);

  const [state, action, pending] = useActionState(createTransaction, {});

  const reset = function (): void {
    setAccountingPeriod(
      accountingPeriods.length > 0
        ? (accountingPeriods[accountingPeriods.length - 1] ?? null)
        : null,
    );
    setDate(null);
    setDescription("");
    setAmount(null);
    setSourceAccount(null);
    setSourceLocation("");
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

  const normalizedSourceLocation = sourceLocation.trim();
  const sourceHasAccount = sourceAccount !== null;
  const sourceIsTracked =
    sourceAccount !== null && isTrackedAccountType(sourceAccount.type);
  const destinationTotal = destinations.reduce(
    (total, destination) => total + (destination.amount ?? 0),
    0,
  );
  const destinationAccountIds = destinations
    .map((destination) => destination.account?.id ?? null)
    .filter((accountId): accountId is string => accountId !== null);
  const destinationLocations = destinations
    .map((destination) => destination.location.trim())
    .filter((location) => location !== "");
  const hasUniqueDestinationAccounts =
    new Set(destinationAccountIds).size === destinationAccountIds.length;
  const hasUniqueDestinationLocations =
    new Set(destinationLocations).size === destinationLocations.length;
  const hasValidSource =
    (sourceHasAccount && normalizedSourceLocation === "") ||
    (!sourceHasAccount && normalizedSourceLocation !== "");
  const areDestinationsComplete = destinations.every((destination) => {
    const normalizedLocation = destination.location.trim();
    const hasAccount = destination.account !== null;
    const hasLocation = normalizedLocation !== "";
    const destinationIsTracked =
      destination.account !== null &&
      isTrackedAccountType(destination.account.type);

    if (destination.amount === null || destination.amount <= 0) {
      return false;
    }
    if ((hasAccount && hasLocation) || (!hasAccount && !hasLocation)) {
      return false;
    }
    if (destination.account?.id === sourceAccount?.id) {
      return false;
    }
    if (sourceIsTracked) {
      return hasAccount && destinationIsTracked;
    }
    if (!sourceHasAccount) {
      return !hasAccount || !destinationIsTracked;
    }
    return !hasAccount || !destinationIsTracked;
  });

  let request: CreateTransactionRequest | null = null;
  if (
    accountingPeriod !== null &&
    (date !== null || defaultDate !== null) &&
    description !== "" &&
    amount !== null &&
    amount > 0 &&
    hasValidSource &&
    destinations.length > 0 &&
    destinationTotal === amount &&
    hasUniqueDestinationAccounts &&
    hasUniqueDestinationLocations &&
    areDestinationsComplete
  ) {
    request = {
      type: CreateAccountTransactionType.Account,
      accountingPeriodId: accountingPeriod.id,
      date:
        date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
      description,
      amount,
      source: {
        accountId: sourceAccount?.id ?? null,
        location:
          sourceAccount === null ? normalizedSourceLocation || null : null,
      },
      destinations: destinations.map((destination) => ({
        accountId: destination.account?.id ?? null,
        location:
          destination.account === null
            ? destination.location.trim() || null
            : null,
        amount: destination.amount ?? 0,
      })),
    };
  }

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
          amount={amount}
          setAmount={setAmount}
        />
        <TransactionSection
          title="Transfer Flow"
          description="Build one source and one or more destinations. The destination amounts should add up to the transaction amount."
        >
          <Stack spacing={2}>
            <AccountTransactionSourceFrame
              accounts={accounts}
              account={sourceAccount}
              setAccount={(account) => {
                setSourceAccount(account);
                if (account !== null) {
                  setSourceLocation("");
                }
              }}
              location={sourceLocation}
              setLocation={setSourceLocation}
              filter={(account) => {
                const selectedAccount =
                  accounts.find((candidate) => candidate.id === account.id) ??
                  null;
                return (
                  selectedAccount !== null &&
                  !destinations.some(
                    (destination) => destination.account?.id === account.id,
                  )
                );
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
                filter={(account) => {
                  const selectedAccount =
                    accounts.find((candidate) => candidate.id === account.id) ??
                    null;
                  const accountUsedElsewhere = destinations.some(
                    (currentDestination, currentIndex) =>
                      currentIndex !== index &&
                      currentDestination.account?.id === account.id,
                  );
                  if (selectedAccount === null || accountUsedElsewhere) {
                    return false;
                  }
                  if (account.id === sourceAccount?.id) {
                    return false;
                  }
                  if (sourceIsTracked) {
                    return isTrackedAccountType(selectedAccount.type);
                  }
                  return !isTrackedAccountType(selectedAccount.type);
                }}
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
                amount !== null && destinationTotal !== amount
                  ? "error.main"
                  : "text.secondary"
              }
            >
              Destination total: ${formatTotal(destinationTotal)}
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
            Create Account Transaction
          </Button>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default CreateAccountTransactionForm;
