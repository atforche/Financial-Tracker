"use client";

import { useEffect, useState } from "react";
import { getPostedBalanceOnDate } from "@/transactions/workspace/balance-adjustment/balanceAdjustmentLookups";

/**
 * A posted balance and the account and date it was retrieved for.
 */
interface PostedBalance {
  readonly key: string;
  readonly balance: number | null;
}

/**
 * Gets an account's posted balance through the end of the provided date. The balance is null while the account or date
 * is missing, while the balance for the current account and date is loading, or when it could not be retrieved, which
 * is reported as failed.
 */
const usePostedBalance = function (
  accountId: string | null,
  date: string | null,
): { readonly balance: number | null; readonly failed: boolean } {
  const key =
    accountId === null || date === null ? null : `${accountId}|${date}`;
  const [postedBalance, setPostedBalance] = useState<PostedBalance | null>(
    null,
  );
  useEffect(() => {
    if (accountId === null || date === null || key === null) {
      return undefined;
    }
    let cancelled = false;
    getPostedBalanceOnDate({ accountId, date })
      .then((balance) => {
        if (!cancelled) {
          setPostedBalance({ key, balance });
        }
      })
      .catch(() => {
        if (!cancelled) {
          setPostedBalance({ key, balance: null });
        }
      });
    return (): void => {
      cancelled = true;
    };
  }, [accountId, date, key]);
  const current = postedBalance?.key === key ? postedBalance : null;
  return {
    balance: current?.balance ?? null,
    failed: current !== null && current.balance === null,
  };
};

export default usePostedBalance;
