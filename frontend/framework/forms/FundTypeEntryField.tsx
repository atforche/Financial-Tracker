import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import { FundType } from "@/data/fundTypes";
import type { JSX } from "react";

/**
 * Props for the FundTypeEntryField component.
 */
interface FundTypeEntryFieldProps {
  readonly label: string;
  readonly value: FundType | null;
  readonly setValue: (newValue: FundType | null) => void;
  readonly errorMessage?: string | null;
}

/**
 * Component that presents the user with an entry field where they can select a Fund Type.
 */
const FundTypeEntryField = function ({
  label,
  value,
  setValue,
  errorMessage = null,
}: FundTypeEntryFieldProps): JSX.Element {
  return (
    <ComboBoxEntryField<FundType>
      label={label}
      options={Object.values(FundType)
        .map((type) => ({
          label: type,
          value: type,
        }))
        .filter((option) => option.value !== FundType.Unassigned)}
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

export default FundTypeEntryField;
