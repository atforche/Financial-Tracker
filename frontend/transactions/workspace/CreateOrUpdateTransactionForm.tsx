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
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Dayjs } from "dayjs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import TransactionDetailsFrame from "@/transactions/workspace/TransactionDetailsFrame";
import TransactionSourceDestinationLayout from "@/transactions/workspace/TransactionSourceDestinationLayout";

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
 * Props for the CreateOrUpdateTransactionForm component.
 */
interface CreateOrUpdateTransactionFormProps<RequestPayload> {
  readonly formRef?: RefObject<HTMLDivElement | null>;
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
  readonly sourceContent: JSX.Element;
  readonly destinationContent: ReactNode;
  readonly flowFooterContent?: ReactNode;
  readonly submitLabel: string;
  readonly state: TransactionFormState;
  readonly pending: boolean;
  readonly request: RequestPayload | null;
  readonly onReset: () => void;
  readonly onSubmit: (request: RequestPayload) => void;
}

/**
 * Displays the shared outer layout used by transaction create and update flows.
 */
const CreateOrUpdateTransactionForm = function <RequestPayload>({
  formRef,
  accountingPeriods,
  accountingPeriod,
  setAccountingPeriod = null,
  date,
  setDate,
  defaultDate,
  description,
  setDescription,
  sourceContent,
  destinationContent,
  flowFooterContent = null,
  submitLabel,
  state,
  pending,
  request,
  onReset,
  onSubmit,
}: CreateOrUpdateTransactionFormProps<RequestPayload>): JSX.Element {
  return (
    <Stack ref={formRef} spacing={3}>
      <Stack spacing={3} sx={{ width: "100%" }}>
        <TransactionDetailsFrame
          accountingPeriods={accountingPeriods}
          accountingPeriod={accountingPeriod}
          setAccountingPeriod={setAccountingPeriod}
          date={date ?? defaultDate}
          setDate={setDate}
          descriptionValue={description}
          setDescriptionValue={setDescription}
        />
        <TransactionSourceDestinationLayout
          sourceFrame={sourceContent}
          destinationFrames={[
            <Stack key="destination-content" spacing={2}>
              {destinationContent}
            </Stack>,
          ]}
        />
        {flowFooterContent}
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

export type { TransactionFormState };
export default CreateOrUpdateTransactionForm;
