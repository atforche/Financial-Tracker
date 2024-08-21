import { useEffect, useState } from "react";

/**
 * Custom hook to handle data fetching from the API in a component.
 * @param {Function} callback - Callback that performs the data fetching.
 * @returns {T} The result of the data callback, or null.
 */
const useData = function <T>(callback: () => Promise<T | null>): T | null {
  const [data, setData] = useState<T | null>(null);
  useEffect(() => {
    let ignore = false;
    callback()
      .then((result) => {
        if (!ignore) {
          setData(result);
        }
      })
      .catch(() => {
        setData(null);
      });
    return (): void => {
      ignore = true;
    };
  }, [callback]);
  return data;
};

export default useData;
