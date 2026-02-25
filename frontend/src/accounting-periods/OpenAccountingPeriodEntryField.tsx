import type { AccountingPeriodIdentifier } from "@accounting-periods/ApiTypes";
import ApiErrorHandler from "@data/ApiErrorHandler";
import { ComboBoxEntryField } from "@framework/dialog/ComboBoxEntryField";
import type { JSX } from "react";
import useGetAllOpenAccountingPeriods from "@accounting-periods/useGetAllOpenAccountingPeriods";

/**
 * Props for the OpenAccountingPeriodEntryField component.
 */
interface OpenAccountingPeriodEntryFieldProps {
  readonly label: string;
  readonly value: AccountingPeriodIdentifier | null;
  readonly setValue?:
    | ((newValue: AccountingPeriodIdentifier | null) => void)
    | null;
  readonly errorHandler?: ApiErrorHandler | null;
  readonly errorKey?: string | null;
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
  errorHandler = null,
  errorKey = null,
}: OpenAccountingPeriodEntryFieldProps): JSX.Element {
  const {
    accountingPeriods,
    isLoading,
    error: fetchError,
  } = useGetAllOpenAccountingPeriods();
  const dualErrorHandler = errorHandler ?? (fetchError ? new ApiErrorHandler(fetchError) : null);
  return (
    <ComboBoxEntryField<AccountingPeriodIdentifier>
      label={label}
      options={
        accountingPeriods?.map((period) => ({
          label: period.name,
          value: { id: period.id, name: period.name },
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
      errorHandler={dualErrorHandler}
      errorKey={errorKey}
    />
  );
};

export default OpenAccountingPeriodEntryField;
