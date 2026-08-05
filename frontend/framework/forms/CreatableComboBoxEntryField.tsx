import {
  ComboBoxEntryField,
  type ComboBoxOption,
} from "@/framework/forms/ComboBoxEntryField";
import type { JSX } from "react";

/**
 * Props for the CreatableComboBoxEntryField component.
 */
interface CreatableComboBoxEntryFieldProps {
  readonly label: string;
  readonly options: readonly string[];
  readonly value: string | null;
  readonly setValue?: ((newValue: string | null) => void) | null;
  readonly errorMessage?: string | null;
  readonly autoFocus?: boolean;
  readonly size?: "small" | "medium";
}

/**
 * Represents a value selected from the existing options or explicitly added by the user.
 */
interface CreatableValue {
  readonly kind: "existing" | "new";
  readonly value: string;
}

const sortValues = function (values: readonly string[]): string[] {
  return [...new Set(values)].sort((left, right) =>
    left < right ? -1 : left > right ? 1 : 0,
  );
};

/**
 * Presents existing string values with an explicit option to add a new value.
 */
const CreatableComboBoxEntryField = function ({
  label,
  options,
  value,
  setValue = null,
  errorMessage = null,
  autoFocus = false,
  size = "medium",
}: CreatableComboBoxEntryFieldProps): JSX.Element {
  const availableOptions = sortValues(
    value === null ? options : [...options, value],
  );
  const toOption = function (
    optionValue: string | null,
  ): ComboBoxOption<CreatableValue> {
    return optionValue === null
      ? { label: "", value: null }
      : {
          label: optionValue,
          value: { kind: "existing", value: optionValue },
        };
  };

  return (
    <ComboBoxEntryField<CreatableValue>
      label={label}
      options={availableOptions.map((optionValue) => ({
        label: optionValue,
        value: { kind: "existing", value: optionValue },
      }))}
      value={toOption(value)}
      setValue={
        setValue === null
          ? null
          : (newValue): void => {
              setValue(newValue?.value?.value ?? null);
            }
      }
      errorMessage={errorMessage}
      autoFocus={autoFocus}
      size={size}
      createOption={(inputValue) => {
        const normalizedValue = inputValue.trim();
        const matchesExistingValue = availableOptions.some(
          (optionValue) =>
            optionValue.toLocaleLowerCase() ===
            normalizedValue.toLocaleLowerCase(),
        );
        return normalizedValue === "" || matchesExistingValue
          ? null
          : {
              label: `Add "${normalizedValue}"`,
              value: { kind: "new", value: normalizedValue },
            };
      }}
      isOptionEqualToValue={(option, selectedValue) =>
        option.value?.kind === selectedValue.value?.kind &&
        option.value?.value === selectedValue.value?.value
      }
    />
  );
};

export default CreatableComboBoxEntryField;
