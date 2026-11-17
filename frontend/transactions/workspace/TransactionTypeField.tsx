"use client";

import {
  type JSX,
  type ReactNode,
  createContext,
  useContext,
  useEffect,
  useMemo,
  useRef,
} from "react";
import {
  getTransactionTypeLabel,
  transactionTypeOptions,
} from "@/transactions/transactionTypeLabels";
import { Box } from "@mui/material";
import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import ReadOnlyField from "@/framework/forms/ReadOnlyField";
import type { TransactionType } from "@/transactions/types";

/**
 * Carries the transaction's type to the details frame, which shows it as its first field.
 */
const TransactionTypeContext = createContext<{
  readonly type: TransactionType;
  readonly setType: ((type: TransactionType) => void) | null;

  /**
   * Changing the type swaps the whole form, which drops focus, so the new form's type field takes it back.
   * Keyboard focus then carries on natively, so Tab still reaches the next field.
   */
  readonly requestFocus: () => void;
  readonly takeFocusRequest: () => boolean;
} | null>(null);

/**
 * Props for the TransactionTypeProvider component.
 */
interface TransactionTypeProviderProps {
  readonly type: TransactionType;
  readonly setType?: ((type: TransactionType) => void) | null;
  readonly children: ReactNode;
}

/**
 * Provides the transaction's type to the details frame. Without setType, the type is shown but cannot be changed.
 */
const TransactionTypeProvider = function ({
  type,
  setType = null,
  children,
}: TransactionTypeProviderProps): JSX.Element {
  const pendingFocusRef = useRef(false);
  const value = useMemo(
    () => ({
      type,
      setType,
      requestFocus: (): void => {
        pendingFocusRef.current = true;
      },
      takeFocusRequest: (): boolean => {
        const requested = pendingFocusRef.current;
        pendingFocusRef.current = false;
        return requested;
      },
    }),
    [type, setType],
  );
  return (
    <TransactionTypeContext.Provider value={value}>
      {children}
    </TransactionTypeContext.Provider>
  );
};

/**
 * Displays the transaction's type: a type-ahead combo box while creating, and a read-only value otherwise.
 */
const TransactionTypeField = function (): JSX.Element | null {
  const context = useContext(TransactionTypeContext);
  const containerRef = useRef<HTMLDivElement | null>(null);
  const takeFocusRequest = context?.takeFocusRequest ?? null;

  useEffect(() => {
    if (takeFocusRequest?.() === true) {
      containerRef.current?.querySelector("input")?.focus();
    }
  }, [takeFocusRequest]);

  if (context === null) {
    return null;
  }
  const { type, setType, requestFocus } = context;
  if (setType === null) {
    return <ReadOnlyField label="Type" value={getTransactionTypeLabel(type)} />;
  }
  return (
    <Box ref={containerRef}>
      <ComboBoxEntryField<TransactionType>
        label="Type"
        options={transactionTypeOptions.map((option) => ({
          label: getTransactionTypeLabel(option),
          value: option,
        }))}
        value={{ label: getTransactionTypeLabel(type), value: type }}
        setValue={(newValue): void => {
          // A transaction always has a type, so ignore attempts to clear it
          if (
            newValue?.value !== null &&
            newValue?.value !== undefined &&
            newValue.value !== type
          ) {
            requestFocus();
            setType(newValue.value);
          }
        }}
        disableClearable
      />
    </Box>
  );
};

export { TransactionTypeProvider };
export default TransactionTypeField;
