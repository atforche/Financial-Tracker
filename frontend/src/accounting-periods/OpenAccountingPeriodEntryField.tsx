import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import type { ApiErrorDetail } from "@data/ApiError";
import ComboBoxEntryField from "@framework/dialog/ComboBoxEntryField";
import type { JSX } from "react";
import useGetAllOpenAccountingPeriods from "@accounting-periods/useGetAllOpenAccountingPeriods";

/**
 * Props for the OpenAccountingPeriodEntryField component.
 * @param label - Label for this Open Accounting Period Entry Field.
 * @param value - Current value for this Open Accounting Period Entry Field.
 * @param setValue - Callback to update the value in this Open Accounting Period Entry Field. If null, this field is read-only.
 * @param error - Error detail to display for this Open Accounting Period Entry Field.
 */
interface OpenAccountingPeriodEntryFieldProps {
  readonly label: string;
  readonly value: AccountingPeriod | null;
  readonly setValue?: ((newValue: AccountingPeriod | null) => void) | null;
  readonly error?: ApiErrorDetail | null;
}

/**
 * Component that presents the user with an entry field where they can select an Open Accounting Period.
 * @param props - Props for the OpenAccountingPeriodEntryField component.
 * @returns JSX element representing the OpenAccountingPeriodEntryField component.
 */
const OpenAccountingPeriodEntryField = function ({
  label,
  value,
  setValue = null,
  error = null,
}: OpenAccountingPeriodEntryFieldProps): JSX.Element {
  const {
    accountingPeriods,
    isLoading,
    error: fetchError,
  } = useGetAllOpenAccountingPeriods();
  return (
    <ComboBoxEntryField<AccountingPeriod>
      label={label}
      options={accountingPeriods.map((period) => ({
        label: period.name,
        value: period,
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
      error={error ?? fetchError}
    />
  );
};

export default OpenAccountingPeriodEntryField;
