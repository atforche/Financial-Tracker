"use client";

import { Button, Stack, Typography } from "@mui/material";
import {
  type Dispatch,
  type JSX,
  type RefObject,
  type SetStateAction,
  startTransition,
} from "react";
import {
  type FundDestinationDraft,
  type FundSourceDraft,
  buildDestinationFundFilter,
  buildSourceFundFilter,
  createEmptyDestination,
} from "@/transactions/workspace/fund/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { AddCircleOutline } from "@mui/icons-material";
import type { Dayjs } from "dayjs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { Fund } from "@/funds/types";
import FundTransactionDestinationFormFrame from "@/transactions/workspace/fund/FundTransactionDestinationFormFrame";
import FundTransactionSourceFormFrame from "@/transactions/workspace/fund/FundTransactionSourceFormFrame";
import TransactionDetailsSection from "@/transactions/workspace/TransactionDetailsSection";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Represents the state of the fund transaction form.
 */
interface FundTransactionFormState {
  readonly success?: boolean;
  readonly transactionId?: string | null;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Props for the CreateOrUpdateFundTransactionForm component.
 */
interface CreateOrUpdateFundTransactionFormProps<RequestPayload> {
  readonly formRef?: RefObject<HTMLDivElement | null>;
  readonly funds: Fund[];
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
  readonly transferFlowDescription: string;
  readonly submitLabel: string;
  readonly state: FundTransactionFormState;
  readonly pending: boolean;
  readonly request: RequestPayload | null;
  readonly onReset: () => void;
  readonly onSubmit: (request: RequestPayload) => void;
}

/**
 * Displays the shared fund transaction form layout used by create and update flows.
 */
const CreateOrUpdateFundTransactionForm = function <RequestPayload>({
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
  transferFlowDescription,
  submitLabel,
  state,
  pending,
  request,
  onReset,
  onSubmit,
}: CreateOrUpdateFundTransactionFormProps<RequestPayload>): JSX.Element {
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
            <FundTransactionSourceFormFrame
              funds={funds}
              fund={source.fund}
              setFund={(fund): void => {
                setSource((currentSource) => ({
                  ...currentSource,
                  fund,
                }));
              }}
              filter={buildSourceFundFilter(destinations)}
              amount={source.amount}
              setAmount={(nextAmount: number | null): void => {
                setSource((currentSource) => ({
                  ...currentSource,
                  amount: nextAmount,
                }));
              }}
            />
            {destinations.map((destination, index) => (
              <FundTransactionDestinationFormFrame
                key={`fund-destination-${index}`}
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

export default CreateOrUpdateFundTransactionForm;
