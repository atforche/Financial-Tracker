import { type ApiError, isApiError } from "@data/ApiError";
import { useCallback, useEffect, useState } from "react";

/**
 * Interface representing the arguments to the useQuery hook.
 * @param queryFunction - Function to execute the asynchronous operation.
 * @param initialData - Placeholder data to use until the asynchronous operation has completed.
 */
interface UseQueryArgs<T> {
  queryFunction: () => Promise<T | ApiError>;
  initialData: T;
}

/**
 * Hook used to query data in an asynchronous manner.
 * @param args - Arguments to use to query the data.
 * @returns Data that was retrieved, the loading state, and the current error.
 */
const useQuery = function <T>({
  queryFunction,
  initialData,
}: UseQueryArgs<T>): {
  data: T;
  isLoading: boolean;
  error: ApiError | null;
  refetch: () => void;
} {
  const [data, setData] = useState<T>(initialData);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<ApiError | null>(null);
  const [refetchIndex, setRefetchIndex] = useState(0);

  const refetch = useCallback(() => {
    setRefetchIndex((prev) => prev + 1);
  }, []);

  useEffect(() => {
    setError(null);
    setIsLoading(true);
    queryFunction()
      .then((result) => {
        if (isApiError(result)) {
          setError(result);
        } else {
          setData(result);
        }
      })
      .catch((err: unknown) => {
        setError({
          message: `An unknown error occurred: ${String(err)}`,
          details: [],
        });
      })
      .finally(() => {
        setIsLoading(false);
      });
  }, [queryFunction, refetchIndex]);

  return { data, isLoading, error, refetch };
};

export default useQuery;
