"use client";

import type {
  CreateTransactionRequest,
  Transaction,
  UpdateTransactionRequest,
} from "@/transactions/types";
import {
  type Dispatch,
  type RefObject,
  type SetStateAction,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import dayjs, { type Dayjs } from "dayjs";
import {
  getDefaultDate,
  redirectWithSelectedTransaction,
} from "@/transactions/workspace/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { TransactionDetails } from "@/transactions/workspace/TransactionForm";
import createTransaction from "@/transactions/workspace/createTransaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import updateTransaction from "@/transactions/workspace/updateTransaction";
import { useRouter } from "next/navigation";
import useTransactionPreview from "@/transactions/workspace/useTransactionPreview";

/**
 * Props shared by every transaction type's create/update form.
 */
interface TransactionEditorFormProps<RequestPayload> {
  readonly formRef: RefObject<HTMLDivElement | null>;
  readonly accountingPeriods: AccountingPeriod[];
  readonly accountingPeriod: AccountingPeriod | null;
  readonly setAccountingPeriod: Dispatch<
    SetStateAction<AccountingPeriod | null>
  > | null;
  readonly date: Dayjs | null;
  readonly setDate: Dispatch<SetStateAction<Dayjs | null>>;
  readonly defaultDate: Dayjs | null;
  readonly description: string;
  readonly setDescription: Dispatch<SetStateAction<string>>;
  readonly submitLabel: string;
  readonly state: {
    readonly success?: boolean;
    readonly transactionId?: string | null;
    readonly errorTitle?: string | null;
    readonly unmappedErrors?: string | null;
  };
  readonly pending: boolean;
  readonly request: RequestPayload | null;
  readonly preview: Transaction | null;
  readonly onReset: () => void;
  readonly onSubmit: (request: RequestPayload) => void;
}

/**
 * Manages the shared state and lifecycle for a transaction creation form.
 */
const useCreateTransactionEditor = function ({
  accountingPeriods,
  details,
  redirectUrl,
  resetDraft,
  buildRequest,
}: {
  readonly accountingPeriods: AccountingPeriod[];
  readonly details: TransactionDetails;
  readonly redirectUrl: string;
  readonly resetDraft: () => void;
  readonly buildRequest: (
    accountingPeriod: AccountingPeriod | null,
    date: Dayjs | null,
    defaultDate: Dayjs | null,
    description: string,
  ) => CreateTransactionRequest | null;
}): TransactionEditorFormProps<CreateTransactionRequest> {
  const router = useRouter();
  const formRef = useRef<HTMLDivElement | null>(null);
  const [state, action, pending] = useActionState(createTransaction, {});

  useEffect(() => {
    if (state.success === true && state.transactionId !== null) {
      router.replace(
        redirectWithSelectedTransaction(redirectUrl, state.transactionId ?? ""),
      );
    }
  }, [redirectUrl, router, state]);

  const { accountingPeriod, date, description } = details;
  const defaultDate = getDefaultDate(accountingPeriod);
  const request = buildRequest(
    accountingPeriod,
    date,
    defaultDate,
    description,
  );
  const preview = useTransactionPreview(
    request === null ? null : { transactionId: null, request },
  );

  return {
    formRef,
    accountingPeriods,
    accountingPeriod,
    setAccountingPeriod: details.setAccountingPeriod,
    date,
    setDate: details.setDate,
    defaultDate,
    description,
    setDescription: details.setDescription,
    submitLabel: "Create",
    state,
    pending,
    request,
    preview,
    onReset: (): void => {
      details.reset();
      resetDraft();
      focusFirstEntryControl(formRef.current);
    },
    onSubmit: (submittedRequest): void => {
      action({ redirectUrl, request: submittedRequest });
    },
  };
};

/**
 * Manages the shared state and lifecycle for a transaction update form.
 */
const useUpdateTransactionEditor = function ({
  transaction,
  transactionAccountingPeriod,
  redirectUrl,
  resetDraft,
  buildRequest,
}: {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly redirectUrl: string;
  readonly resetDraft: () => void;
  readonly buildRequest: (
    accountingPeriod: AccountingPeriod,
    date: Dayjs | null,
    description: string,
  ) => UpdateTransactionRequest | null;
}): TransactionEditorFormProps<UpdateTransactionRequest> {
  const router = useRouter();
  const formRef = useRef<HTMLDivElement | null>(null);
  const [state, action, pending] = useActionState(updateTransaction, {});
  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [description, setDescription] = useState<string>(
    transaction.description,
  );

  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl);
    }
  }, [redirectUrl, router, state.success]);

  const request = buildRequest(transactionAccountingPeriod, date, description);
  const preview = useTransactionPreview(
    request === null ? null : { transactionId: transaction.id, request },
    transaction,
  );

  return {
    formRef,
    accountingPeriods: [transactionAccountingPeriod],
    accountingPeriod: transactionAccountingPeriod,
    setAccountingPeriod: null,
    date,
    setDate,
    defaultDate: null,
    description,
    setDescription,
    submitLabel: "Update",
    state,
    pending,
    request,
    preview,
    onReset: (): void => {
      setDate(dayjs(transaction.date));
      setDescription(transaction.description);
      resetDraft();
      focusFirstEntryControl(formRef.current);
    },
    onSubmit: (submittedRequest): void => {
      action({
        transactionId: transaction.id,
        redirectUrl,
        request: submittedRequest,
      });
    },
  };
};

export { useCreateTransactionEditor, useUpdateTransactionEditor };
