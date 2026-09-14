import type { AccountingPeriod } from "@/accounting-periods/types";
import createApiClient from "@/framework/data/createApiClient";
import loadAllPages from "@/framework/data/loadAllPages";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/** Finds chronological neighbors among all saved accounting periods. */
const getAdjacentAccountingPeriods = async function (
  currentPeriodId: string,
): Promise<{
  previousPeriod: AccountingPeriod | null;
  nextPeriod: AccountingPeriod | null;
}> {
  const apiClient = await createApiClient();
  const periods = await loadAllPages(async (limit, offset) =>
    unwrapApiResponse(
      await apiClient.GET("/accounting-periods", {
        params: { query: { Limit: limit, Offset: offset } },
      }),
      "Failed to fetch accounting period navigation",
    ),
  );
  periods.sort((left, right) =>
    left.year === right.year
      ? left.month - right.month
      : left.year - right.year,
  );
  const currentIndex = periods.findIndex(
    (period) => period.id === currentPeriodId,
  );
  return {
    previousPeriod: periods[currentIndex - 1] ?? null,
    nextPeriod: periods[currentIndex + 1] ?? null,
  };
};

export default getAdjacentAccountingPeriods;
