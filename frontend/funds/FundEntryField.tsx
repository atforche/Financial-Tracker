import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import type { FundIdentifier } from "@/funds/types";
import type { JSX } from "react";

/**
 * Props for the FundEntryField component.
 */
interface FundEntryFieldProps {
  readonly label: string;
  readonly options: FundIdentifier[];
  readonly value: FundIdentifier | null;
  readonly setValue: ((newValue: FundIdentifier | null) => void) | null;
  readonly filter: ((fund: FundIdentifier) => boolean) | null;
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
  autoFocus = false,
}: FundEntryFieldProps): JSX.Element {
  return (
    <ComboBoxEntryField<FundIdentifier>
      label={label}
      options={options
        .filter((fund) => (filter ? filter(fund) : true))
        .map((fund) => ({
          label: fund.name,
          value: fund,
        }))}
      value={
        value === null
          ? { label: "", value: null }
          : { label: value.name, value }
      }
      setValue={
        setValue === null
          ? null
          : (newValue): void => {
              setValue(newValue.value);
            }
      }
      autoFocus={autoFocus}
    />
  );
};

export default FundEntryField;
