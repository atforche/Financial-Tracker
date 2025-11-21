import { type ApiError, isApiError } from "@data/ApiError";
import { useCallback, useEffect, useState } from "react";

/**
 * Interface representing the arguments to the useApiRequest hook.
 * @param {() => Promise<void>} apiRequestFunction - Function to execute the API request.
 */
interface UseApiRequestArgs {
  apiRequestFunction: () => Promise<ApiError | null>;
}

/**
 * Hook used to execute an API request in an asynchronous manner.
 * @param {UseApiRequestArgs} args - Arguments to use to execute the API request.
 * @returns {{isRunning: boolean, isSuccess: boolean, error: ApiError | null, execute: () => void}} - Running state, success state, current error, and function to execute the API request.
 */
const useApiRequest = function ({ apiRequestFunction }: UseApiRequestArgs): {
  isRunning: boolean;
  isSuccess: boolean;
  error: ApiError | null;
  execute: () => void;
} {
  const [isRunning, setIsRunning] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
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

  return { isRunning, isSuccess, error, execute };
};

export { useApiRequest };
