import { type ApiError, isApiError } from "@data/ApiError";
import { useCallback, useEffect, useState } from "react";

/**
 * Interface representing the arguments to the useApiRequest hook.
 * @param apiRequestFunction - Function to execute the API request.
 */
interface UseApiRequestArgs<T> {
  apiRequestFunction: () => Promise<T | ApiError | null>;
}

/**
 * Hook used to execute an API request in an asynchronous manner.
 * @param args - Arguments to use to execute the API request.
 * @returns Running state, success state, current error, and function to execute the API request.
 */
const useApiRequest = function <T>({
  apiRequestFunction,
}: UseApiRequestArgs<T>): {
  isRunning: boolean;
  isSuccess: boolean;
  response: T | null;
  error: ApiError | null;
  execute: () => void;
} {
  const [isRunning, setIsRunning] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [response, setResponse] = useState<T | null>(null);
  const [error, setError] = useState<ApiError | null>(null);
  const [shouldRun, setShouldRun] = useState(false);

  const execute = useCallback(() => {
    setShouldRun(true);
  }, []);

  useEffect(() => {
    if (!shouldRun) {
      return;
    }
    setShouldRun(false);
    setIsRunning(true);
    setError(null);
    apiRequestFunction()
      .then((result) => {
        if (isApiError(result)) {
          setError(result);
        } else {
          setResponse(result);
          setIsSuccess(true);
        }
      })
      .catch((err: unknown) => {
        setError({
          message: `An unknown error occurred: ${String(err)}`,
          details: [],
        });
      })
      .finally(() => {
        setIsRunning(false);
      });
  }, [apiRequestFunction, shouldRun]);

  return { isRunning, isSuccess, response, error, execute };
};

export default useApiRequest;
