import type { AccountingPeriod } from "@/accounting-periods/types";
import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import type { JSX } from "react";

/**
 * Props for the AccountingPeriodEntryField component.
 */
interface AccountingPeriodEntryFieldProps {
  readonly label: string;
  readonly options: AccountingPeriod[];
  readonly value: AccountingPeriod | null;
  readonly setValue?: ((newValue: AccountingPeriod | null) => void) | null;
  readonly errorMessage?: string | null;
}

/**
 * Component that presents the user with an entry field where they can select an Accounting Period.
 */
const AccountingPeriodEntryField = function ({
  label,
  options,
  value,
  setValue = null,
  errorMessage = null,
}: AccountingPeriodEntryFieldProps): JSX.Element {
  return (
    <ComboBoxEntryField<AccountingPeriod>
      label={label}
      options={options.map((accountingPeriod) => ({
        label: accountingPeriod.name,
        value: accountingPeriod,
      }))}
      value={
        value === null
          ? { label: "", value: null }
          : { label: value.name, value }
      }
      setValue={
        setValue !== null
          ? (newValue): void => {
              setValue(newValue?.value ?? null);
            }
          : null
      }
      errorMessage={errorMessage}
    />
  );
};

export default AccountingPeriodEntryField;
