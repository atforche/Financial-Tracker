"use client";

import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import { FundGoalType } from "@/data/fundTypes";
import type { JSX } from "react";

/**
 * Props for the FundGoalTypeEntryField component.
 */
interface FundGoalTypeEntryFieldProps {
  readonly label: string;
  readonly value: FundGoalType | null;
  readonly setValue: (newValue: FundGoalType | null) => void;
  readonly errorMessage?: string | null;
}

/**
 * Component that presents the user with an entry field where they can select a Fund Goal Type.
 */
const FundGoalTypeEntryField = function ({
  label,
  value,
  setValue,
  errorMessage = null,
}: FundGoalTypeEntryFieldProps): JSX.Element {
  return (
    <ComboBoxEntryField<FundGoalType>
      label={label}
      options={Object.values(FundGoalType).map((goalType) => ({
        label: goalType,
        value: goalType,
      }))}
      value={
        value === null ? { label: "", value: null } : { label: value, value }
      }
      setValue={(newValue): void => {
        setValue(newValue.value);
      }}
      errorMessage={errorMessage}
    />
  );
};

export default FundGoalTypeEntryField;
