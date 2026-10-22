"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";
import type { Transaction } from "@/transactions/types";
import previewTransactionPosting from "@/transactions/workspace/previewTransactionPosting";

/**
 * Delay after the last posting date change before requesting a preview.
 */
const previewDelayMilliseconds = 300;

/**
 * Reports the date an account is about to be posted on, or null when it is not being posted.
 */
type ReportPostingDate = (accountId: string, date: string | null) => void;

/**
 * Lets posting controls report their posting dates to the view that previews them.
 */
const PostingDateReporterContext = createContext<ReportPostingDate | null>(
  null,
);

/**
 * Gets the reporter for posting dates, or null when no view previews postings.
 */
const usePostingDateReporter = function (): ReportPostingDate | null {
  return useContext(PostingDateReporterContext);
};

/**
 * Gets the key identifying a posting preview. Previews depend on everything about the saved transaction,
 * including which accounts are already posted and the balances around it, so the whole transaction is part
 * of the key and any change to it is previewed again.
 */
const getPostingKey = (
  transactionKey: string,
  accountId: string,
  date: string,
): string => `${transactionKey}|${accountId}:${date}`;

/**
 * Previews posting the transaction to each account on its reported posting date. Returns the preview for
 * each account whose latest preview matches its current posting date, keyed by account ID.
 */
const useTransactionPostingPreviews = function (transaction: Transaction): {
  readonly reportPostingDate: ReportPostingDate;
  readonly previews: Readonly<Record<string, Transaction>>;
} {
  const transactionId = transaction.id;
  const transactionKey = JSON.stringify(transaction);
  const [postingDates, setPostingDates] = useState<
    Readonly<Record<string, string | null>>
  >({});
  const [previewsByKey, setPreviewsByKey] = useState<
    Readonly<Record<string, Transaction>>
  >({});
  const postingDatesKey = JSON.stringify(postingDates);
  const latestPostingDates = useRef(postingDates);
  const latestPreviewsByKey = useRef(previewsByKey);
  const requestedKeys = useRef(new Set<string>());
  const isMounted = useRef(true);

  useEffect(() => {
    latestPostingDates.current = postingDates;
    latestPreviewsByKey.current = previewsByKey;
  });

  useEffect(() => {
    isMounted.current = true;
    return (): void => {
      isMounted.current = false;
    };
  }, []);

  useEffect(() => {
    // Only the delay is cancelled when the dates change again; requests already sent are kept, so they
    // Are not discarded and sent a second time.
    const timeout = setTimeout(() => {
      Object.entries(latestPostingDates.current).forEach(
        ([accountId, date]) => {
          if (date === null) {
            return;
          }
          const key = getPostingKey(transactionKey, accountId, date);
          if (
            key in latestPreviewsByKey.current ||
            requestedKeys.current.has(key)
          ) {
            return;
          }
          requestedKeys.current.add(key);
          previewTransactionPosting({ transactionId, accountId, date })
            .then((preview) => {
              if (preview === null) {
                requestedKeys.current.delete(key);
              } else if (isMounted.current) {
                setPreviewsByKey((current) => ({ ...current, [key]: preview }));
              }
            })
            .catch(() => {
              // The view keeps showing the posted balances, and the posting can be previewed again later.
              requestedKeys.current.delete(key);
            });
        },
      );
    }, previewDelayMilliseconds);
    return (): void => {
      clearTimeout(timeout);
    };
  }, [postingDatesKey, transactionKey, transactionId]);

  const reportPostingDate = useCallback<ReportPostingDate>(
    (accountId, date) => {
      setPostingDates((current) =>
        current[accountId] === date
          ? current
          : { ...current, [accountId]: date },
      );
    },
    [],
  );

  const previews = Object.fromEntries(
    Object.entries(postingDates).flatMap(([accountId, date]) => {
      const preview =
        date === null
          ? undefined
          : previewsByKey[getPostingKey(transactionKey, accountId, date)];
      return preview === undefined ? [] : [[accountId, preview]];
    }),
  );
  return { reportPostingDate, previews };
};

export {
  PostingDateReporterContext,
  usePostingDateReporter,
  useTransactionPostingPreviews,
};
