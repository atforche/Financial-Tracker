import { useEffect, useState } from "react";

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
}: UseQueryArgs<T>): { data: T; isLoading: boolean; isError: boolean } {
  const [data, setData] = useState<T>(initialData);
  const [isLoading, setIsLoading] = useState(false);
  const [isError, setIsError] = useState(false);

  useEffect(() => {
    const fetchData = async function (): Promise<void> {
      setIsError(false);
      setIsLoading(true);

      try {
        const result = await queryFunction();
        setData(result);
      } catch (error) {
        setIsError(true);
      }

      setIsLoading(false);
    };

    fetchData().catch(() => null);
  }, [queryFunction]);

  return { data, isLoading, isError };
};

export { useQuery };
