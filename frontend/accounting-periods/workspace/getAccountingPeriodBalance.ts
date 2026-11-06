"use server";

import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import createApiClient from "@/framework/data/createApiClient";

/**
 * Loads an accounting period's balance for a form that can do without it, or null when it can't be loaded.
 */
const getAccountingPeriodBalance = async function (
  accountingPeriodId: string,
): Promise<AccountingPeriodWithBalance | null> {
  const apiClient = await createApiClient();
  const { data } = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: { path: { accountingPeriodId } },
    },
  );
  return data ?? null;
};

export default getAccountingPeriodBalance;
