import ComboBoxEntryField from "@framework/dialog/ComboBoxEntryField";
import type { Fund } from "@funds/ApiTypes";
import type { JSX } from "react";
import useGetAllFunds from "@funds/useGetAllFunds";

/**
 * Props for the FundEntryField component.
 * @param label - Label for this Fund Entry Field.
 * @param value - Current value for this Fund Entry Field.
 * @param setValue - Callback to update the value in this Fund Entry Field. If null, this field is read-only.
 */
interface FundEntryFieldProps {
  readonly label: string;
  readonly value: Fund | null;
  readonly setValue?: ((newValue: Fund | null) => void) | null;
}

/**
 * Component that presents the user with an entry field where they can select a Fund.
 * @param props - Props for the FundEntryField component.
 * @returns JSX element representing the FundEntryField component.
 */
const FundEntryField = function ({
  label,
  value,
  setValue = null,
}: FundEntryFieldProps): JSX.Element {
  const { funds, isLoading, error } = useGetAllFunds();
  return (
    <ComboBoxEntryField<Fund>
      label={label}
      options={funds.map((fund) => ({
        label: fund.name,
        value: fund,
      }))}
      value={
        value === null
          ? { label: "", value: null }
          : { label: value.name, value }
      }
      setValue={
        setValue
          ? (newValue): void => {
              setValue(newValue.value);
            }
          : null
      }
      loading={isLoading}
      error={error}
    />
  );
};

export default FundEntryField;
