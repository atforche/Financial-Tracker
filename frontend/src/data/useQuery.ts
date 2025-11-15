import { useCallback, useEffect, useState } from "react";

/**
 * Interface representing the arguments to the useQuery hook.
 * @param {() => Promise<T>} queryFunction - Function to execute the asynchronous operation.
 * @param {T} initialData - Placeholder data to use until the asynchronous operation has completed.
 */
interface UseQueryArgs<T> {
  queryFunction: () => Promise<T>;
  initialData: T;
}

/**
 * Hook used to query data in an asynchronous manner.
 * @param {UseQueryArgs<T>} queryArgs - Arguments to use to query the data.
 * @returns {{T, boolean, boolean}} - Data that was retrieved, the loading state, and the error state.
 */
const useQuery = function <T>({
  queryFunction,
  initialData,
}: UseQueryArgs<T>): {
  data: T;
  isLoading: boolean;
  isError: boolean;
  refetch: () => void;
} {
  const [data, setData] = useState<T>(initialData);
  const [isLoading, setIsLoading] = useState(false);
  const [isError, setIsError] = useState(false);
  const [refetchIndex, setRefetchIndex] = useState(0);

  const refetch = useCallback(() => {
    setRefetchIndex((prev) => prev + 1);
  }, []);

  useEffect(() => {
    setIsError(false);
    setIsLoading(true);
    queryFunction()
      .then((result) => {
        setData(result);
      })
      .catch(() => {
        setIsError(true);
      })
      .finally(() => {
        setIsLoading(false);
      });
  }, [queryFunction, refetchIndex]);

  return { data, isLoading, isError, refetch };
};

export { useQuery };
