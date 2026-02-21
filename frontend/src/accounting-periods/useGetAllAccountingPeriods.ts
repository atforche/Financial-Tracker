import type {
  AccountingPeriod,
  AccountingPeriodSortOrder,
} from "@accounting-periods/ApiTypes";
import type { ApiError } from "@data/ApiError";
import getApiClient from "@data/getApiClient";
import { useCallback } from "react";
import useQuery from "@data/useQuery";

/**
 * Arguments for the useGetAllAccountingPeriods hook.
 */
interface UseGetAllAccountingPeriodsArgs {
  readonly sortBy: AccountingPeriodSortOrder | null;
  readonly page: number;
  readonly rowsPerPage: number;
}

/**
 * Hook used to retrieve all Accounting Periods via the API.
 * @param args - The arguments for the useGetAllAccountingPeriods hook.
 * @returns Retrieved Accounting Periods, loading state, current error, and function to refetch the Accounting Periods.
 */
const useGetAllAccountingPeriods = function ({
  sortBy,
  page,
  rowsPerPage,
}: UseGetAllAccountingPeriodsArgs): {
  accountingPeriods: AccountingPeriod[] | null;
  totalCount: number | null;
  isLoading: boolean;
  error: ApiError | null;
  refetch: () => void;
} {
  const getAllAccountingPeriodsCallback = useCallback<
    () => Promise<{ items: AccountingPeriod[]; totalCount: number } | ApiError>
  >(async () => {
    const client = getApiClient();
    const { data, error } = await client.GET("/accounting-periods", {
      params: {
        query: {
          SortBy: sortBy,
          Limit: rowsPerPage,
          Offset: page * rowsPerPage,
        },
      },
    });
    if (typeof error !== "undefined") {
      return error;
    }
    return data;
  }, [sortBy, page, rowsPerPage]);

  const { data, isLoading, error, refetch } = useQuery<{
    items: AccountingPeriod[];
    totalCount: number;
  }>({
    queryFunction: getAllAccountingPeriodsCallback,
  });
  return {
    accountingPeriods: data?.items ?? null,
    totalCount: data?.totalCount ?? null,
    isLoading,
    error,
    refetch,
  };
};

export default useGetAllAccountingPeriods;
