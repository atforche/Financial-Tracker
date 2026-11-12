"use client";

import {
  type JSX,
  type ReactNode,
  createContext,
  useContext,
  useMemo,
} from "react";
import { MenuItem, TextField } from "@mui/material";
import {
  getTransactionTypeLabel,
  transactionTypeOptions,
} from "@/transactions/transactionTypeLabels";
import ReadOnlyField from "@/framework/forms/ReadOnlyField";
import { TransactionType } from "@/transactions/types";
import parseEnumValue from "@/framework/data/parseEnumValue";

/**
 * Carries the transaction's type to the details frame, which shows it as its last field.
 */
const TransactionTypeContext = createContext<{
  readonly type: TransactionType;
  readonly setType: ((type: TransactionType) => void) | null;
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
  const value = useMemo(() => ({ type, setType }), [type, setType]);
  return (
    <TransactionTypeContext.Provider value={value}>
      {children}
    </TransactionTypeContext.Provider>
  );
};

/**
 * Displays the transaction's type: a dropdown while creating, and a read-only value otherwise.
 */
const TransactionTypeField = function (): JSX.Element | null {
  const context = useContext(TransactionTypeContext);
  if (context === null) {
    return null;
  }
  const { type, setType } = context;
  if (setType === null) {
    return <ReadOnlyField label="Type" value={getTransactionTypeLabel(type)} />;
  }
  return (
    <TextField
      select
      label="Type"
      value={type}
      onChange={(event) => {
        const nextType = parseEnumValue(TransactionType, event.target.value);
        if (nextType !== null) {
          setType(nextType);
        }
      }}
      sx={{ width: "100%" }}
    >
      {transactionTypeOptions.map((option) => (
        <MenuItem key={option} value={option}>
          {getTransactionTypeLabel(option)}
        </MenuItem>
      ))}
    </TextField>
  );
};

export { TransactionTypeProvider };
export default TransactionTypeField;
