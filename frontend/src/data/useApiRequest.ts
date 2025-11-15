import { useCallback, useEffect, useState } from "react";

/**
 * Interface representing the arguments to the useApiRequest hook.
 * @param {() => Promise<void>} apiRequestFunction - Function to execute the API request.
 */
interface UseApiRequestArgs {
  apiRequestFunction: () => Promise<void>;
}

/**
 * Hook used to execute an API request in an asynchronous manner.
 * @param {UseApiRequestArgs} args - Arguments to use to execute the API request.
 * @returns {{isRunning: boolean, isSuccess: boolean, isError: boolean, execute: () => void}} - Running state, success state, error state, and function to execute the API request.
 */
const useApiRequest = function ({ apiRequestFunction }: UseApiRequestArgs): {
  isRunning: boolean;
  isSuccess: boolean;
  isError: boolean;
  execute: () => void;
} {
  const [isRunning, setIsRunning] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [isError, setIsError] = useState(false);
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
    setIsError(false);
    apiRequestFunction()
      .then(() => {
        setIsSuccess(true);
      })
      .catch(() => {
        setIsError(true);
      })
      .finally(() => {
        setIsRunning(false);
      });
  }, [apiRequestFunction, shouldRun]);

  return { isRunning, isSuccess, isError, execute };
};

export { useApiRequest };
