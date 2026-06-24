"use client";

import { Button, Stack, Typography } from "@mui/material";
import {
  type FundTransaction,
  type Transaction,
  UpdateFundTransactionType,
  type UpdateTransactionRequest,
  asFundTransaction,
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
import type { AccountingPeriod } from "@/accounting-periods/types";
import { AddCircleOutline } from "@mui/icons-material";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/funds/types";
import FundTransactionDestinationFrame from "@/transactions/workspace/FundTransactionDestinationFrame";
import FundTransactionSourceFrame from "@/transactions/workspace/FundTransactionSourceFrame";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import updateTransaction from "@/transactions/workspace/updateTransaction";
import { useRouter } from "next/navigation";

interface UpdateFundTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly funds: Fund[];
  readonly redirectUrl: string;
}

interface FundDestinationDraft {
  readonly fund: Fund | null;
  readonly amount: number | null;
}

type FundTransactionDestinationModel = FundTransaction["destinations"][number];

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
 * Displays the dedicated update form for fund transfer transactions.
 */
const UpdateFundTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  funds,
  redirectUrl,
}: UpdateFundTransactionFormProps): JSX.Element {
  const formRef = useRef<HTMLDivElement | null>(null);
  const fundTransaction: FundTransaction | null =
    asFundTransaction(transaction);

  const getFundById = function (fundId: string): Fund | null {
    return funds.find((fund) => fund.id === fundId) ?? null;
  };

  const buildSourceFundFromTransaction = function (): Fund | null {
    if (fundTransaction === null) {
      return null;
    }

    return getFundById(fundTransaction.source.fund.fundId);
  };

  const buildDestinationsFromTransaction = function (): FundDestinationDraft[] {
    if (fundTransaction === null) {
      return [createEmptyDestination()];
    }

    return fundTransaction.destinations.map(
      (destination: FundTransactionDestinationModel) => ({
        fund: getFundById(destination.fund.fundId),
        amount: destination.fund.amount,
      }),
    );
  };

  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [amount, setAmount] = useState<number | null>(transaction.amount);
  const [sourceFund, setSourceFund] = useState<Fund | null>(
    buildSourceFundFromTransaction(),
  );
  const [destinations, setDestinations] = useState<FundDestinationDraft[]>(
    buildDestinationsFromTransaction(),
  );

  const router = useRouter();
  const [state, action, pending] = useActionState(updateTransaction, {});

  const reset = function (): void {
    setDate(dayjs(transaction.date));
    setDescription(transaction.description);
    setAmount(transaction.amount);
    setSourceFund(buildSourceFundFromTransaction());
    setDestinations(buildDestinationsFromTransaction());
  };

  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

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

  let request: UpdateTransactionRequest | null = null;
  if (
    fundTransaction !== null &&
    date !== null &&
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
      type: UpdateFundTransactionType.Fund,
      date: date.format("YYYY-MM-DD"),
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
          description="Edit the source and each destination fund. The destination amounts should add up to the transaction amount."
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
                action({ transactionId: transaction.id, redirectUrl, request });
              });
            }}
          >
            Update Fund Transfer
          </Button>
        </Stack>
      </Stack>
    </Stack>
  );
};

export default UpdateFundTransactionForm;
