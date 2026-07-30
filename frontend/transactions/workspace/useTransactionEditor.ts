"use client";

import type {
  CreateTransactionRequest,
  UpdateTransactionRequest,
} from "@/transactions/types";
import { useActionState, useEffect, useRef } from "react";
import createTransaction from "@/transactions/workspace/createTransaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import { redirectWithSelectedTransaction } from "@/transactions/workspace/helpers";
import updateTransaction from "@/transactions/workspace/updateTransaction";
import { useRouter } from "next/navigation";

/**
 * Shared lifecycle configuration for a transaction editor.
 */
interface TransactionEditorOptions {
  readonly redirectUrl: string;
  readonly resetDraft: () => void;
}

/**
 * Shared lifecycle values for a transaction editor.
 */
interface TransactionEditorLifecycle {
  readonly formRef: { readonly current: HTMLDivElement | null };
  readonly state: {
    readonly success?: boolean;
    readonly transactionId?: string | null;
    readonly errorTitle?: string | null;
    readonly unmappedErrors?: string | null;
  };
  readonly pending: boolean;
  readonly reset: () => void;
}

/**
 * Manages the shared lifecycle for a transaction creation editor.
 */
const useCreateTransactionEditor = function ({
  redirectUrl,
  resetDraft,
}: TransactionEditorOptions): TransactionEditorLifecycle & {
  readonly submit: (request: CreateTransactionRequest) => void;
} {
  const router = useRouter();
  const formRef = useRef<HTMLDivElement | null>(null);
  const [state, action, pending] = useActionState(createTransaction, {});

  useEffect(() => {
    if (state.success === true && state.transactionId !== null) {
      router.replace(
        redirectWithSelectedTransaction(redirectUrl, state.transactionId ?? ""),
        { scroll: false },
      );
    }
  }, [redirectUrl, router, state]);

  const reset = function (): void {
    resetDraft();
    focusFirstEntryControl(formRef.current);
  };

  return {
    formRef,
    state,
    pending,
    reset,
    submit: (request): void => {
      action({ redirectUrl, request });
    },
  };
};

/**
 * Manages the shared lifecycle for a transaction update editor.
 */
const useUpdateTransactionEditor = function ({
  transactionId,
  redirectUrl,
  resetDraft,
}: TransactionEditorOptions & {
  readonly transactionId: string;
}): TransactionEditorLifecycle & {
  readonly submit: (request: UpdateTransactionRequest) => void;
} {
  const router = useRouter();
  const formRef = useRef<HTMLDivElement | null>(null);
  const [state, action, pending] = useActionState(updateTransaction, {});

  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

  const reset = function (): void {
    resetDraft();
    focusFirstEntryControl(formRef.current);
  };

  return {
    formRef,
    state,
    pending,
    reset,
    submit: (request): void => {
      action({ transactionId, redirectUrl, request });
    },
  };
};

export { useCreateTransactionEditor, useUpdateTransactionEditor };
