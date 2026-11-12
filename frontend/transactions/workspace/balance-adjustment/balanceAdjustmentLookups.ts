"use server";

import {
  TransactionSortModel,
  TransactionTypeModel,
} from "@/framework/data/api";
import createApiClient from "@/framework/data/createApiClient";

/**
 * Server action that gets an account's posted balance through the end of the provided date, or null when it cannot be
 * retrieved.
 */
const getPostedBalanceOnDate = async function ({
  accountId,
  date,
}: {
  readonly accountId: string;
  readonly date: string;
}): Promise<number | null> {
  const client = await createApiClient();
  const { data } = await client.GET("/accounts/date-range", {
    params: {
      query: {
        "Range.Start": date,
        "Range.End": date,
        "Filter.Ids": [accountId],
      },
    },
  });
  return data?.accounts.items[0]?.endingBalance ?? null;
};

/**
 * Server action that gets the description of the provided account's most recent balance adjustment, or null when
 * it has none.
 */
const getLatestBalanceAdjustmentDescription = async function (
  accountId: string,
): Promise<string | null> {
  const client = await createApiClient();
  const { data } = await client.GET("/transactions", {
    params: {
      query: {
        "Filter.AccountIds": [accountId],
        "Filter.Types": [TransactionTypeModel.BalanceAdjustment],
        Sort: TransactionSortModel.DateDescending,
        Limit: 1,
      },
    },
  });
  return data?.items[0]?.description ?? null;
};

export { getLatestBalanceAdjustmentDescription, getPostedBalanceOnDate };
