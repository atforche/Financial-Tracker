"use client";

import previewTransaction, {
  type TransactionPreviewPayload,
} from "@/transactions/workspace/previewTransaction";
import { useEffect, useRef, useState } from "react";
import type { Transaction } from "@/transactions/types";

/**
 * Delay after the last edit before requesting a preview, so typing does not send a request per keystroke.
 */
const previewDelayMilliseconds = 300;

/**
 * A preview and the serialized payload it was produced for.
 */
interface TransactionPreview {
  readonly payloadKey: string;
  readonly transaction: Transaction;
}

/**
 * Previews the transaction the provided payload would produce once saved, including its balances.
 * Returns null while there is no valid payload or the latest preview does not match the current payload,
 * so callers only ever display balances that match what is being edited. When editing a saved
 * transaction, it serves as the preview for the initial payload, since that payload reproduces it.
 */
const useTransactionPreview = function (
  payload: TransactionPreviewPayload | null,
  savedTransaction: Transaction | null = null,
): Transaction | null {
  const payloadKey = payload === null ? null : JSON.stringify(payload);
  const [preview, setPreview] = useState<TransactionPreview | null>(() =>
    payloadKey === null || savedTransaction === null
      ? null
      : { payloadKey, transaction: savedTransaction },
  );
  const latestPayload = useRef(payload);
  const latestPreviewKey = useRef(preview?.payloadKey ?? null);

  useEffect(() => {
    latestPayload.current = payload;
    latestPreviewKey.current = preview?.payloadKey ?? null;
  });

  useEffect(() => {
    const requestedPayload = latestPayload.current;
    if (
      payloadKey === null ||
      requestedPayload === null ||
      latestPreviewKey.current === payloadKey
    ) {
      return undefined;
    }
    let cancelled = false;
    const timeout = setTimeout(() => {
      previewTransaction(requestedPayload)
        .then((transaction) => {
          if (!cancelled && transaction !== null) {
            setPreview({ payloadKey, transaction });
          }
        })
        .catch(() => {
          // The form keeps showing current balances when a preview cannot be produced.
        });
    }, previewDelayMilliseconds);
    return (): void => {
      cancelled = true;
      clearTimeout(timeout);
    };
  }, [payloadKey]);

  return preview?.payloadKey === payloadKey ? preview.transaction : null;
};

export default useTransactionPreview;
