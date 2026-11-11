"use client";

import {
  type Dispatch,
  type SetStateAction,
  useCallback,
  useLayoutEffect,
  useState,
} from "react";
import { getCurrentVisit, saveTrail } from "@/framework/routes/visitTrail";

/**
 * Like useState, but the value is kept with the current page visit, so returning to the page restores it. The value
 * must survive JSON, and isValue checks a restored value before it is used. A fresh visit starts from the initial
 * value. The key must be unique within the page.
 */
const usePageState = function <T>(
  key: string,
  initialValue: T,
  isValue: (value: unknown) => value is T,
): [T, Dispatch<SetStateAction<T>>] {
  const [value, setValue] = useState(initialValue);

  // Runs after PageRestoration records the arrival, and before paint so a restored value never flashes
  useLayoutEffect(() => {
    const saved = getCurrentVisit()?.state[key];
    if (isValue(saved)) {
      setValue(saved);
    }
    // Restores once per mount; the guard is not expected to change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [key]);

  const update = useCallback<Dispatch<SetStateAction<T>>>(
    (action) => {
      setValue((current) => {
        const next = action instanceof Function ? action(current) : action;
        const visit = getCurrentVisit();
        if (visit !== undefined) {
          visit.state[key] = next;
          saveTrail();
        }
        return next;
      });
    },
    [key],
  );

  return [value, update];
};

/**
 * Checks whether a restored value is a boolean.
 */
const isBoolean = function (value: unknown): value is boolean {
  return typeof value === "boolean";
};

/**
 * Checks whether a restored value is a string.
 */
const isString = function (value: unknown): value is string {
  return typeof value === "string";
};

/**
 * Checks whether a restored value is a list of strings.
 */
const isStringArray = function (value: unknown): value is string[] {
  return Array.isArray(value) && value.every(isString);
};

/**
 * Checks whether a restored value is an object of booleans.
 */
const isBooleanRecord = function (
  value: unknown,
): value is Record<string, boolean> {
  return (
    typeof value === "object" &&
    value !== null &&
    !Array.isArray(value) &&
    Object.values(value).every(isBoolean)
  );
};

export { isBoolean, isBooleanRecord, isString, isStringArray };
export default usePageState;
