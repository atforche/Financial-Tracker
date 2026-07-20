import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import type { JSX } from "react";

/**
 * Props for the FundPlanTypeEntryField component.
 */
interface FundPlanTypeEntryFieldProps<T> {
  readonly label: string;
  readonly options: readonly T[];
  readonly value: T | null;
  readonly setValue: ((newValue: T | null) => void) | null;
  readonly formatOptionLabel: (value: T) => string;
  readonly errorMessage?: string | null;
}

/**
 * Presents an entry field for selecting a Funding Plan type.
 */
const FundPlanTypeEntryField = function <T>({
  label,
  options,
  value,
  setValue,
  formatOptionLabel,
  errorMessage = null,
}: FundPlanTypeEntryFieldProps<T>): JSX.Element {
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

export default FundPlanTypeEntryField;
