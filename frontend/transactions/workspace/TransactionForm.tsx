"use client";

import { Button, Stack } from "@mui/material";
import {
  type Dispatch,
  type JSX,
  type ReactNode,
  type RefObject,
  type SetStateAction,
  startTransition,
} from "react";
import {
  validateDetails,
  validateSummary,
} from "@/transactions/workspace/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { Dayjs } from "dayjs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import TransactionDetailsFrame from "@/transactions/workspace/TransactionDetailsFrame";
import TransactionSourceDestinationLayout from "@/transactions/workspace/TransactionSourceDestinationLayout";
import TransactionSourceDestinationSummary from "@/transactions/workspace/TransactionSourceDestinationSummary";

/**
 * Represents the shared status displayed by transaction create/update forms.
 */
interface TransactionFormState {
  readonly success?: boolean;
  readonly transactionId?: string | null;
  readonly errorTitle?: string | null;
  readonly unmappedErrors?: string | null;
}

/**
 * Props for the TransactionForm component.
 */
interface TransactionFormProps<RequestPayload> {
  readonly readOnly?: boolean;
  readonly formRef?: RefObject<HTMLDivElement | null>;
  readonly accountingPeriods?: AccountingPeriod[];
  readonly accountingPeriod: AccountingPeriod | null;
  readonly setAccountingPeriod?: Dispatch<
    SetStateAction<AccountingPeriod | null>
  > | null;
  readonly date: Dayjs | null;
  readonly setDate?: Dispatch<SetStateAction<Dayjs | null>> | null;
  readonly defaultDate: Dayjs | null;
  readonly description: string;
  readonly setDescription?: Dispatch<SetStateAction<string>> | null;
  readonly headerContent?: ReactNode;
  readonly sourceContent?: JSX.Element;
  readonly destinationContent?: ReactNode;
  readonly sourceAmount?: number | null;
  readonly destinationAmount?: number;
  readonly destinationCount?: number;
  readonly submitLabel?: string;
  readonly state?: TransactionFormState;
  readonly pending?: boolean;
  readonly request?: RequestPayload | null;
  readonly onReset?: (() => void) | null;
  readonly onSubmit?: ((request: RequestPayload) => void) | null;
}

const emptyAccountingPeriods: AccountingPeriod[] = [];
const defaultState: TransactionFormState = {};
const emptyFrame = <Stack />;

/**
 * Displays the shared outer layout used by transaction create, update, and read-only flows.
 */
const TransactionForm = function <RequestPayload>({
  readOnly = false,
  formRef,
  accountingPeriods = emptyAccountingPeriods,
  accountingPeriod,
  setAccountingPeriod = null,
  date,
  setDate = null,
  defaultDate,
  description,
  setDescription = null,
  headerContent,
  sourceContent,
  destinationContent,
  sourceAmount,
  destinationAmount,
  destinationCount = 0,
  submitLabel = "Submit",
  state = defaultState,
  pending = false,
  request = null,
  onReset = null,
  onSubmit = null,
}: TransactionFormProps<RequestPayload>): JSX.Element {
  const detailsAreValid = readOnly
    ? true
    : validateDetails(accountingPeriod, date, defaultDate, description);
  const summaryIsValid = readOnly
    ? true
    : validateSummary(sourceAmount, destinationAmount ?? 0, destinationCount);

  return (
    <Stack ref={formRef} spacing={3}>
      <ConstrainedContent maxWidth={1800}>
        <Stack spacing={3}>
          <TransactionDetailsFrame
            accountingPeriods={accountingPeriods}
            accountingPeriod={accountingPeriod}
            setAccountingPeriod={readOnly ? null : setAccountingPeriod}
            date={date ?? defaultDate}
            setDate={readOnly ? null : setDate}
            descriptionValue={description}
            setDescriptionValue={readOnly ? null : setDescription}
            headerContent={headerContent}
            color={detailsAreValid ? "info" : "error"}
          />
          <TransactionSourceDestinationLayout
            sourceFrame={sourceContent ?? emptyFrame}
            destinationFrames={[
              <Stack key="destination-content" spacing={2}>
                {destinationContent}
              </Stack>,
            ]}
          />
          {typeof destinationAmount === "number" ? (
            <TransactionSourceDestinationSummary
              sourceAmount={sourceAmount ?? 0}
              destinationAmount={destinationAmount}
              isValid={summaryIsValid}
            />
          ) : null}
          {!readOnly ? (
            <>
              <ErrorAlert
                errorMessage={state.errorTitle ?? null}
                unmappedErrors={state.unmappedErrors ?? null}
              />
              <Stack
                direction={{ xs: "column", sm: "row" }}
                spacing={1.5}
                justifyContent="flex-end"
              >
                <Button
                  variant="outlined"
                  onClick={onReset ?? ((): null => null)}
                >
                  Reset
                </Button>
                <Button
                  variant="contained"
                  loading={pending}
                  disabled={request === null || onSubmit === null}
                  onClick={(): void => {
                    if (request === null || onSubmit === null) {
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
            </>
          ) : null}
        </Stack>
      </ConstrainedContent>
    </Stack>
  );
};

export type { TransactionFormState };
export default TransactionForm;
