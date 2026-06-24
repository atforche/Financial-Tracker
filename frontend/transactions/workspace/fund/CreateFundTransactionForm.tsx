"use client";

import { Button, Stack, Typography } from "@mui/material";
import {
  CreateFundTransactionType,
  type CreateTransactionRequest,
} from "@/transactions/transaction";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { AddCircleOutline } from "@mui/icons-material";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/funds/types";
import FundTransactionDestinationFrame from "@/transactions/workspace/fund/FundTransactionDestinationFrame";
import FundTransactionSourceFrame from "@/transactions/workspace/fund/FundTransactionSourceFrame";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import createTransaction from "@/transactions/workspace/createTransaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import { useRouter } from "next/navigation";

interface CreateFundTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly funds: Fund[];
  readonly redirectUrl: string;
}

interface FundDestinationDraft {
  readonly fund: Fund | null;
  readonly amount: number | null;
}

const createEmptyDestination = function (): FundDestinationDraft {
  return {
    fund: null,
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
 * Displays the dedicated create form for fund transfer transactions.
 */
const CreateFundTransactionForm = function ({
  accountingPeriods,
  funds,
  redirectUrl,
}: CreateFundTransactionFormProps): JSX.Element {
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
  const [sourceFund, setSourceFund] = useState<Fund | null>(null);
  const [destinations, setDestinations] = useState<FundDestinationDraft[]>([
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
    setSourceFund(null);
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

  const destinationTotal = destinations.reduce(
    (total, destination) => total + (destination.amount ?? 0),
    0,
  );
  const destinationFundIds = destinations
    .map((destination) => destination.fund?.id ?? null)
    .filter((fundId): fundId is string => fundId !== null);
  const hasUniqueDestinationFunds =
    new Set(destinationFundIds).size === destinationFundIds.length;
  const areDestinationsComplete = destinations.every(
    (destination) =>
      destination.fund !== null &&
      destination.amount !== null &&
      destination.amount > 0 &&
      destination.fund.id !== sourceFund?.id,
  );

  let request: CreateTransactionRequest | null = null;
  if (
    accountingPeriod !== null &&
    (date !== null || defaultDate !== null) &&
    description !== "" &&
    amount !== null &&
    amount > 0 &&
    sourceFund !== null &&
    destinations.length > 0 &&
    destinationTotal === amount &&
    hasUniqueDestinationFunds &&
    areDestinationsComplete
  ) {
    request = {
      type: CreateFundTransactionType.Fund,
      accountingPeriodId: accountingPeriod.id,
      date:
        date?.format("YYYY-MM-DD") ?? defaultDate?.format("YYYY-MM-DD") ?? "",
      description,
      amount,
      source: {
        fundId: sourceFund.id,
      },
      destinations: destinations.map((destination) => ({
        fundId: destination.fund?.id ?? "",
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
          description="Build one source and one or more destination funds. The destination amounts should add up to the transaction amount."
        >
          <Stack spacing={2}>
            <FundTransactionSourceFrame
              funds={funds}
              fund={sourceFund}
              setFund={setSourceFund}
              filter={(fund) =>
                !destinations.some(
                  (destination) => destination.fund?.id === fund.id,
                )
              }
            />
            {destinations.map((destination, index) => (
              <FundTransactionDestinationFrame
                key={`fund-destination-${index}`}
                index={index}
                funds={funds}
                fund={destination.fund}
                setFund={(fund) => {
                  updateDestination(index, (currentDestination) => ({
                    ...currentDestination,
                    fund,
                  }));
                }}
                amount={destination.amount}
                setAmount={(nextAmount) => {
                  updateDestination(index, (currentDestination) => ({
                    ...currentDestination,
                    amount: nextAmount,
                  }));
                }}
                filter={(fund) => {
                  const fundUsedElsewhere = destinations.some(
                    (currentDestination, currentIndex) =>
                      currentIndex !== index &&
                      currentDestination.fund?.id === fund.id,
                  );
                  return !fundUsedElsewhere && fund.id !== sourceFund?.id;
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
            Create Fund Transfer
          </Button>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default CreateFundTransactionForm;
