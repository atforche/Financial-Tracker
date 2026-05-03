"use client";

import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import { GoalType } from "@/goals/types";
import type { JSX } from "react";

/**
 * Props for the GoalTypeEntryField component.
 */
interface GoalTypeEntryFieldProps {
  readonly label: string;
  readonly value: GoalType | null;
  readonly setValue: (newValue: GoalType | null) => void;
  readonly errorMessage?: string | null;
}

/**
 * Component that presents the user with an entry field where they can select a Goal Type.
 */
const GoalTypeEntryField = function ({
  label,
  value,
  setValue,
  errorMessage = null,
}: GoalTypeEntryFieldProps): JSX.Element {
  return (
    <ComboBoxEntryField<GoalType>
      label={label}
      options={Object.values(GoalType).map((goalType) => ({
        label: goalType,
        value: goalType,
      }))}
      value={
        value === null ? { label: "", value: null } : { label: value, value }
      }
      setValue={(newValue): void => {
        setValue(newValue?.value ?? null);
      }}
      errorMessage={errorMessage}
    />
  );
};

export default GoalTypeEntryField;
