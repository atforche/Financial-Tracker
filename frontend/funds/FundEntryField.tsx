import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";

/**
 * Props for the FundEntryField component.
 */
interface FundEntryFieldProps {
  readonly label: string;
  readonly options: Fund[];
  readonly value: Fund | null;
  readonly setValue: ((newValue: Fund | null) => void) | null;
  readonly filter: ((fund: Fund) => boolean) | null;
  readonly getOptionSecondaryLabel?: ((fund: Fund) => string | null) | null;
  readonly sortComparator?: ((left: Fund, right: Fund) => number) | null;
  readonly autoFocus?: boolean;
}

/**
 * Component that presents the user with an entry field where they can select a Fund.
 */
const FundEntryField = function ({
  label,
  options,
  value,
  setValue,
  filter,
  getOptionSecondaryLabel = null,
  sortComparator = null,
  autoFocus = false,
}: FundEntryFieldProps): JSX.Element {
  const filteredOptions = options.filter((fund) =>
    filter ? filter(fund) : true,
  );
  const orderedOptions =
    sortComparator === null
      ? filteredOptions
      : [...filteredOptions].sort(sortComparator);

  return (
    <ComboBoxEntryField<Fund>
      label={label}
      options={orderedOptions.map((fund) => ({
        label: fund.name,
        secondaryLabel: getOptionSecondaryLabel?.(fund) ?? null,
        value: fund,
      }))}
      value={
        value === null
          ? { label: "", value: null }
          : { label: value.name, value }
      }
      isOptionEqualToValue={(option, selectedValue) =>
        option.value?.id === selectedValue.value?.id
      }
      setValue={
        setValue === null
          ? null
          : (newValue): void => {
              setValue(newValue?.value ?? null);
            }
      }
      autoFocus={autoFocus}
    />
  );
};

export default FundEntryField;
