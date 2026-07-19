import type { Account } from "@/accounts/types";
import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import type { JSX } from "react";

/**
 * Props for the AccountEntryField component.
 */
interface AccountEntryFieldProps {
  readonly label: string;
  readonly options: Account[];
  readonly value: Account | null;
  readonly setValue: ((newValue: Account | null) => void) | null;
  readonly errorMessage?: string | null;
  readonly filter?: ((account: Account) => boolean) | null;
}

/**
 * Component that presents the user with an entry field where they can select an Account.
 */
const AccountEntryField = function ({
  label,
  options,
  value,
  setValue,
  errorMessage = null,
  filter = null,
}: AccountEntryFieldProps): JSX.Element {
  return (
    <ComboBoxEntryField<Account>
      label={label}
      options={options
        .filter((account) => (filter ? filter(account) : true))
        .map((account) => ({
          label: account.name,
          value: account,
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
      errorMessage={errorMessage}
    />
  );
};

export default AccountEntryField;
