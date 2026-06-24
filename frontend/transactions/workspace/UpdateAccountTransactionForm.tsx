"use client";

import { type Account, isTrackedAccountType } from "@/accounts/types";
import {
  type AccountTransaction,
  type Transaction,
  UpdateAccountTransactionType,
  type UpdateTransactionRequest,
  asAccountTransaction,
} from "@/transactions/types";
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
import AccountTransactionDestinationFrame from "@/transactions/workspace/AccountTransactionDestinationFrame";
import AccountTransactionSourceFrame from "@/transactions/workspace/AccountTransactionSourceFrame";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { AddCircleOutline } from "@mui/icons-material";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import updateTransaction from "@/transactions/workspace/updateTransaction";
import { useRouter } from "next/navigation";

interface UpdateAccountTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly accounts: Account[];
  readonly redirectUrl: string;
}

interface AccountDestinationDraft {
  readonly account: Account | null;
  readonly location: string;
  readonly amount: number | null;
}

type AccountTransactionDestinationModel =
  AccountTransaction["destinations"][number];

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
 * Displays the dedicated update form for account transactions.
 */
const UpdateAccountTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  accounts,
  redirectUrl,
}: UpdateAccountTransactionFormProps): JSX.Element {
  const formRef = useRef<HTMLDivElement | null>(null);
  const accountTransaction: AccountTransaction | null =
    asAccountTransaction(transaction);

  const getAccountById = function (accountId: string): Account | null {
    return accounts.find((account) => account.id === accountId) ?? null;
  };

  const buildSourceAccountFromTransaction = function (): Account | null {
    if (
      accountTransaction === null ||
      typeof accountTransaction.source.account !== "object" ||
      accountTransaction.source.account === null
    ) {
      return null;
    }
    return getAccountById(accountTransaction.source.account.accountId);
  };

  const buildDestinationsFromTransaction =
    function (): AccountDestinationDraft[] {
      if (accountTransaction === null) {
        return [createEmptyDestination()];
      }
      return accountTransaction.destinations.map(
        (destination: AccountTransactionDestinationModel) => ({
          account:
            destination.account === null ||
            typeof destination.account === "undefined"
              ? null
              : getAccountById(destination.account.accountId),
          location: destination.location ?? "",
          amount: destination.amount,
        }),
      );
    };

  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [amount, setAmount] = useState<number | null>(transaction.amount);
  const [sourceAccount, setSourceAccount] = useState<Account | null>(
    buildSourceAccountFromTransaction(),
  );
  const [sourceLocation, setSourceLocation] = useState<string>(
    accountTransaction?.source.location ?? "",
  );
  const [destinations, setDestinations] = useState<AccountDestinationDraft[]>(
    buildDestinationsFromTransaction(),
  );

  const router = useRouter();
  const [state, action, pending] = useActionState(updateTransaction, {});

  const reset = function (): void {
    setDate(dayjs(transaction.date));
    setDescription(transaction.description);
    setAmount(transaction.amount);
    setSourceAccount(buildSourceAccountFromTransaction());
    setSourceLocation(accountTransaction?.source.location ?? "");
    setDestinations(buildDestinationsFromTransaction());
  };

  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

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

  let request: UpdateTransactionRequest | null = null;
  if (
    accountTransaction !== null &&
    date !== null &&
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
      type: UpdateAccountTransactionType.Account,
      date: date.format("YYYY-MM-DD"),
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
          accountingPeriods={[transactionAccountingPeriod]}
          accountingPeriod={transactionAccountingPeriod}
          setAccountingPeriod={null}
          date={date}
          setDate={setDate}
          descriptionValue={description}
          setDescriptionValue={setDescription}
          amount={amount}
          setAmount={setAmount}
        />
        <TransactionSection
          title="Transfer Flow"
          description="Edit the source and each destination. The destination amounts should add up to the transaction amount."
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
                action({ transactionId: transaction.id, redirectUrl, request });
              });
            }}
          >
            Update Account Transaction
          </Button>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default UpdateAccountTransactionForm;
