import { ComboBoxEntryField } from "@framework/dialog/ComboBoxEntryField";
import type { FundIdentifier } from "@funds/ApiTypes";
import type { JSX } from "react";
import useGetAllFunds from "@funds/useGetAllFunds";

/**
 * Props for the FundEntryField component.
 * @param label - Label for this Fund Entry Field.
 * @param value - Current value for this Fund Entry Field.
 * @param setValue - Callback to update the value in this Fund Entry Field. If null, this field is read-only.
 * @param filter - Function to filter the available funds for selection.
 */
interface FundEntryFieldProps {
  readonly label: string;
  readonly value: FundIdentifier | null;
  readonly setValue?: ((newValue: FundIdentifier | null) => void) | null;
  readonly filter: ((fund: FundIdentifier) => boolean) | null;
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
  filter = null,
}: FundEntryFieldProps): JSX.Element {
  const { funds, isLoading, error } = useGetAllFunds();
  return (
    <ComboBoxEntryField<FundIdentifier>
      label={label}
      options={
        funds
          ?.filter((fund) => (filter ? filter(fund) : true))
          .map((fund) => ({
            label: fund.name,
            value: fund,
          })) ?? []
      }
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
