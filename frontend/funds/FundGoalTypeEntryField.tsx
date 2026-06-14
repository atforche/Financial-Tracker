import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import type { JSX } from "react";

/**
 * Props for the FundGoalTypeEntryField component.
 */
interface FundGoalTypeEntryFieldProps<T> {
  readonly label: string;
  readonly options: readonly T[];
  readonly value: T | null;
  readonly setValue: ((newValue: T | null) => void) | null;
  readonly formatOptionLabel: (value: T) => string;
  readonly errorMessage?: string | null;
}

/**
 * Component that presents the user with an entry field where they can select a goal type.
 */
const FundGoalTypeEntryField = function <T>({
  label,
  options,
  value,
  setValue,
  formatOptionLabel,
  errorMessage = null,
}: FundGoalTypeEntryFieldProps<T>): JSX.Element {
  return (
    <ComboBoxEntryField<T>
      label={label}
      options={options.map((option) => ({
        label: formatOptionLabel(option),
        value: option,
      }))}
      value={
        value === null
          ? { label: "", value: null }
          : {
              label: formatOptionLabel(value),
              value,
            }
      }
      setValue={
        setValue === null
          ? null
          : (newValue): void => {
              setValue(newValue?.value ?? null);
            }
      }
      errorMessage={errorMessage}
    />
  );
};

export default FundGoalTypeEntryField;
